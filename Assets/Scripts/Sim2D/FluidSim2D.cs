using Seb.Fluid2D.Rendering;
using Seb.Helpers;
using UnityEngine;
using Unity.Mathematics;
using UnityEngine.Serialization;
using System.Collections.Generic;
using System;

namespace Seb.Fluid2D.Simulation
{
    public class FluidSim2D : MonoBehaviour
    {
        public event System.Action SimulationStepCompleted;

        [System.Serializable]
        public struct BoundsData
        {
            public Vector2 size;
            [SerializeField] public Vector2 center;
        }

        [Header("Simulation Settings")]
        public float timeScale = 1;
        public float maxTimestepFPS = 60; // if time-step dips lower than this fps, simulation will run slower (set to 0 to disable)
        public int iterationsPerFrame;
        public float gravity;
        [Range(0, 1)] public float collisionDamping = 0.95f;
        public float smoothingRadius = 2;
        public float targetDensity;
        public float pressureMultiplier;
        public float nearPressureMultiplier;
        public float viscosityStrength;

        [Header("Bounds Settings")]
        public BoundsData bounds;
        public BoundsData obstacle;



        [Header("Interaction Settings")]
        public float interactionRadius;

        public float interactionStrength;

        [Header("References")]
        public ComputeShader compute;

        public Spawner2D spawner2D;

        [Header("Colliders")]
        [NonSerialized] public List<BoxCollider2D> boxColliders = new List<BoxCollider2D>();
        [NonSerialized] public List<CircleCollider2D> circleColliders = new List<CircleCollider2D>();

        [Header("Collider Repulsion")]
        [Tooltip("排斥力强度")]
        [Range(0, 50)] public float colliderRepulsionStrength = 20f;

        [Tooltip("作用范围(相对于平滑半径的比例)")]
        [Range(0.1f, 1f)] public float colliderRepulsionRadius = 0.5f;

        // 添加缓存变量减少GPU回读
        private float2[] _cachedParticlePositions;

        // 添加这个结构体来存储碰撞体数据
        public struct SceneColliderData
        {
            public Vector2 position;
            public Vector2 size;
            public float rotation;
            public int colliderType; // 0 = box, 1 = circle
        }

        private ComputeBuffer sceneCollidersBuffer;
        private SceneColliderData[] sceneCollidersData;

        // Buffers
        public ComputeBuffer positionBuffer { get; private set; }
        public ComputeBuffer velocityBuffer { get; private set; }
        public ComputeBuffer densityBuffer { get; private set; }

        ComputeBuffer sortTarget_Position;
        ComputeBuffer sortTarget_PredicitedPosition;
        ComputeBuffer sortTarget_Velocity;

        ComputeBuffer predictedPositionBuffer;
        SpatialHash spatialHash;

        // Kernel IDs
        const int externalForcesKernel = 0;
        const int spatialHashKernel = 1;
        const int reorderKernel = 2;
        const int copybackKernel = 3;
        const int densityKernel = 4;
        const int pressureKernel = 5;
        const int viscosityKernel = 6;
        const int updatePositionKernel = 7;

        // State
        bool isPaused;
        Spawner2D.ParticleSpawnData spawnData;
        bool pauseNextFrame;

        public int numParticles { get; private set; }


        void Start()
        {
            Debug.Log("Controls: Space = Play/Pause, R = Reset, LMB = Attract, RMB = Repel");

            // 自动收集场景中的碰撞体
            FindCollidersInScene();
            Init();

            SetupSceneColliders();

        }

        void Init()
        {
            float deltaTime = 1 / 60f;
            Time.fixedDeltaTime = deltaTime;

            spawnData = spawner2D.GetSpawnData();
            numParticles = spawnData.positions.Length;
            spatialHash = new SpatialHash(numParticles);

            // Create buffers
            positionBuffer = ComputeHelper.CreateStructuredBuffer<float2>(numParticles);
            predictedPositionBuffer = ComputeHelper.CreateStructuredBuffer<float2>(numParticles);
            velocityBuffer = ComputeHelper.CreateStructuredBuffer<float2>(numParticles);
            densityBuffer = ComputeHelper.CreateStructuredBuffer<float2>(numParticles);

            sortTarget_Position = ComputeHelper.CreateStructuredBuffer<float2>(numParticles);
            sortTarget_PredicitedPosition = ComputeHelper.CreateStructuredBuffer<float2>(numParticles);
            sortTarget_Velocity = ComputeHelper.CreateStructuredBuffer<float2>(numParticles);

            // Set buffer data
            SetInitialBufferData(spawnData);

            // Init compute
            ComputeHelper.SetBuffer(compute, positionBuffer, "Positions", externalForcesKernel, updatePositionKernel, reorderKernel, copybackKernel);
            ComputeHelper.SetBuffer(compute, predictedPositionBuffer, "PredictedPositions", externalForcesKernel, spatialHashKernel, densityKernel, pressureKernel, viscosityKernel, reorderKernel, copybackKernel);
            ComputeHelper.SetBuffer(compute, velocityBuffer, "Velocities", externalForcesKernel, pressureKernel, viscosityKernel, updatePositionKernel, reorderKernel, copybackKernel);
            ComputeHelper.SetBuffer(compute, densityBuffer, "Densities", densityKernel, pressureKernel, viscosityKernel);

            ComputeHelper.SetBuffer(compute, spatialHash.SpatialIndices, "SortedIndices", spatialHashKernel, reorderKernel);
            ComputeHelper.SetBuffer(compute, spatialHash.SpatialOffsets, "SpatialOffsets", spatialHashKernel, densityKernel, pressureKernel, viscosityKernel);
            ComputeHelper.SetBuffer(compute, spatialHash.SpatialKeys, "SpatialKeys", spatialHashKernel, densityKernel, pressureKernel, viscosityKernel);

            ComputeHelper.SetBuffer(compute, sortTarget_Position, "SortTarget_Positions", reorderKernel, copybackKernel);
            ComputeHelper.SetBuffer(compute, sortTarget_PredicitedPosition, "SortTarget_PredictedPositions", reorderKernel, copybackKernel);
            ComputeHelper.SetBuffer(compute, sortTarget_Velocity, "SortTarget_Velocities", reorderKernel, copybackKernel);

            compute.SetInt("numParticles", numParticles);

            // 初始化缓存数组
            _cachedParticlePositions = new float2[numParticles];
        }

        // 碰撞体收集逻辑
        private void CollectColliderData(List<SceneColliderData> collidersList)
        {
            // 处理矩形碰撞体
            foreach (var boxCollider in boxColliders)
            {
                if (boxCollider == null || !boxCollider.enabled) continue;

                collidersList.Add(new SceneColliderData()
                {
                    position = boxCollider.transform.position,
                    size = boxCollider.size * new Vector2(
                        boxCollider.transform.lossyScale.x,
                        boxCollider.transform.lossyScale.y),
                    rotation = boxCollider.transform.eulerAngles.z * Mathf.Deg2Rad,
                    colliderType = 0
                });
            }

            // 处理圆形碰撞体
            foreach (var circleCollider in circleColliders)
            {
                if (circleCollider == null || !circleCollider.enabled) continue;

                float radius = circleCollider.radius * Mathf.Max(
                    circleCollider.transform.lossyScale.x,
                    circleCollider.transform.lossyScale.y);

                collidersList.Add(new SceneColliderData()
                {
                    position = circleCollider.transform.position,
                    size = new Vector2(radius * 2, radius * 2),
                    rotation = 0,
                    colliderType = 1
                });
            }
        }

        // 简化的初始化方法
        void SetupSceneColliders()
        {
            List<SceneColliderData> collidersList = new List<SceneColliderData>();
            CollectColliderData(collidersList);

            sceneCollidersData = collidersList.ToArray();
            ComputeHelper.CreateStructuredBuffer(ref sceneCollidersBuffer, sceneCollidersData);

            // 设置到计算着色器
            SetColliderBuffersToShader();
        }

        // 更新碰撞体数据
        void UpdateSceneColliders()
        {
            List<SceneColliderData> collidersList = new List<SceneColliderData>();
            CollectColliderData(collidersList);

            sceneCollidersData = collidersList.ToArray();
            sceneCollidersBuffer.SetData(sceneCollidersData);
            compute.SetInt("numSceneColliders", sceneCollidersData.Length);
        }

        // 将碰撞体缓冲区设置到着色器的公共方法
        void SetColliderBuffersToShader()
        {
            compute.SetBuffer(updatePositionKernel, "SceneColliders", sceneCollidersBuffer);
            compute.SetInt("numSceneColliders", sceneCollidersData.Length);

            // 设置到其他需要的kernel
            compute.SetBuffer(externalForcesKernel, "SceneColliders", sceneCollidersBuffer);
        }

        // 自动收集场景中的碰撞体
        void FindCollidersInScene()
        {
            if (Application.isPlaying)
            {
                // 清空现有列表
                boxColliders.Clear();
                circleColliders.Clear();

                // 查找所有激活的BoxCollider2D
                var allBoxColliders = FindObjectsOfType<BoxCollider2D>(false); // false表示只查找激活的对象
                foreach (var collider in allBoxColliders)
                {
                    // 可以添加额外的过滤条件，比如检查特定组件等
                    boxColliders.Add(collider);
                }

                // 查找所有激活的CircleCollider2D
                var allCircleColliders = FindObjectsOfType<CircleCollider2D>(false);
                foreach (var collider in allCircleColliders)
                {
                    // 可以添加额外的过滤条件
                    circleColliders.Add(collider);
                }

                Debug.Log($"Found {boxColliders.Count} box colliders and {circleColliders.Count} circle colliders in scene");
            }
        }

        void Update()
        {
            if (!isPaused)
            {
                float maxDeltaTime = maxTimestepFPS > 0 ? 1 / maxTimestepFPS : float.PositiveInfinity; // If framerate dips too low, run the simulation slower than real-time
                float dt = Mathf.Min(Time.deltaTime * timeScale, maxDeltaTime);
                RunSimulationFrame(dt);
            }

            if (pauseNextFrame)
            {
                isPaused = true;
                pauseNextFrame = false;
            }

            HandleInput();
        }

        void RunSimulationFrame(float frameTime)
        {
            float timeStep = frameTime / iterationsPerFrame;

            UpdateSettings(timeStep);

            for (int i = 0; i < iterationsPerFrame; i++)
            {
                RunSimulationStep();
                SimulationStepCompleted?.Invoke();
            }
        }

        void RunSimulationStep()
        {
            ComputeHelper.Dispatch(compute, numParticles, kernelIndex: externalForcesKernel);

            RunSpatial();

            ComputeHelper.Dispatch(compute, numParticles, kernelIndex: densityKernel);
            ComputeHelper.Dispatch(compute, numParticles, kernelIndex: pressureKernel);
            ComputeHelper.Dispatch(compute, numParticles, kernelIndex: viscosityKernel);
            ComputeHelper.Dispatch(compute, numParticles, kernelIndex: updatePositionKernel);

            // 添加: 每帧模拟完成后更新位置缓存
            positionBuffer.GetData(_cachedParticlePositions);
        }

        void RunSpatial()
        {
            ComputeHelper.Dispatch(compute, numParticles, kernelIndex: spatialHashKernel);
            spatialHash.Run();

            ComputeHelper.Dispatch(compute, numParticles, kernelIndex: reorderKernel);
            ComputeHelper.Dispatch(compute, numParticles, kernelIndex: copybackKernel);
        }

        void UpdateSettings(float deltaTime)
        {
            // 更新碰撞体数据
            UpdateSceneColliders();

            compute.SetFloat("deltaTime", deltaTime);
            compute.SetFloat("gravity", gravity);
            compute.SetFloat("collisionDamping", collisionDamping);
            compute.SetFloat("smoothingRadius", smoothingRadius);
            compute.SetFloat("targetDensity", targetDensity);
            compute.SetFloat("pressureMultiplier", pressureMultiplier);
            compute.SetFloat("nearPressureMultiplier", nearPressureMultiplier);
            compute.SetFloat("viscosityStrength", viscosityStrength);
            compute.SetVector("boundsSize", bounds.size);
            compute.SetVector("obstacleSize", obstacle.size);
            compute.SetVector("boundsCentre", bounds.center);
            compute.SetVector("obstacleCentre", obstacle.center);
            compute.SetFloat("colliderRepulsionStrength", colliderRepulsionStrength);
            compute.SetFloat("colliderRepulsionRadius", colliderRepulsionRadius);

            compute.SetFloat("Poly6ScalingFactor", 4 / (Mathf.PI * Mathf.Pow(smoothingRadius, 8)));
            compute.SetFloat("SpikyPow3ScalingFactor", 10 / (Mathf.PI * Mathf.Pow(smoothingRadius, 5)));
            compute.SetFloat("SpikyPow2ScalingFactor", 6 / (Mathf.PI * Mathf.Pow(smoothingRadius, 4)));
            compute.SetFloat("SpikyPow3DerivativeScalingFactor", 30 / (Mathf.Pow(smoothingRadius, 5) * Mathf.PI));
            compute.SetFloat("SpikyPow2DerivativeScalingFactor", 12 / (Mathf.Pow(smoothingRadius, 4) * Mathf.PI));

            // Mouse interaction settings:
            Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            bool isPullInteraction = Input.GetMouseButton(0);
            bool isPushInteraction = Input.GetMouseButton(1);
            float currInteractStrength = 0;
            if (isPushInteraction || isPullInteraction)
            {
                currInteractStrength = isPushInteraction ? -interactionStrength : interactionStrength;
            }

            compute.SetVector("interactionInputPoint", mousePos);
            compute.SetFloat("interactionInputStrength", currInteractStrength);
            compute.SetFloat("interactionInputRadius", interactionRadius);
        }

        void SetInitialBufferData(Spawner2D.ParticleSpawnData spawnData)
        {
            float2[] allPoints = new float2[spawnData.positions.Length];
            System.Array.Copy(spawnData.positions, allPoints, spawnData.positions.Length);

            positionBuffer.SetData(allPoints);
            predictedPositionBuffer.SetData(allPoints);
            velocityBuffer.SetData(spawnData.velocities);
        }

        void HandleInput()
        {
            //if (Input.GetKeyDown(KeyCode.Q))
            //{
            //    isPaused = !isPaused;
            //}

            //if (Input.GetKeyDown(KeyCode.RightArrow))
            //{
            //    isPaused = false;
            //    pauseNextFrame = true;
            //}

            //if (Input.GetKeyDown(KeyCode.R))
            //{
            //    isPaused = true;
            //    // Reset positions, the run single frame to get density etc (for debug purposes) and then reset positions again
            //    SetInitialBufferData(spawnData);
            //    RunSimulationStep();
            //    SetInitialBufferData(spawnData);
            //}
        }


        void OnDestroy()
        {

            // 释放缓冲区
            ComputeHelper.Release(sceneCollidersBuffer);
            ComputeHelper.Release(positionBuffer, predictedPositionBuffer, velocityBuffer, densityBuffer, sortTarget_Position, sortTarget_Velocity, sortTarget_PredicitedPosition);
            spatialHash.Release();
        }

        void OnDisable()
        {
            // 清空碰撞体列表，防止残留数据
            boxColliders.Clear();
            circleColliders.Clear();
        }

        void OnDrawGizmos()
        {
            Gizmos.color = new Color(0, 1, 0, 0.4f);
            Gizmos.DrawWireCube(bounds.center, bounds.size);
            Gizmos.DrawWireCube(obstacle.center, obstacle.size);

            if (Application.isPlaying)
            {
                Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                bool isPullInteraction = Input.GetMouseButton(0);
                bool isPushInteraction = Input.GetMouseButton(1);
                bool isInteracting = isPullInteraction || isPushInteraction;
                if (isInteracting)
                {
                    Gizmos.color = isPullInteraction ? Color.green : Color.red;
                    Gizmos.DrawWireSphere(mousePos, interactionRadius);
                }
            }
        }

        public float2[] GetParticlePositions()
        {
            if (_cachedParticlePositions == null || _cachedParticlePositions.Length != numParticles)
            {
                _cachedParticlePositions = new float2[numParticles];
            }
            positionBuffer.GetData(_cachedParticlePositions);
            return _cachedParticlePositions; //返回缓存数组
        }


    }
}