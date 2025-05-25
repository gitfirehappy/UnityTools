using Seb.Fluid2D.Simulation;
using UnityEngine;

public class WaterParticleDisplay2D : MonoBehaviour
{
    [Header("References")]
    public FluidSim2D sim;
    public Mesh particleMesh;
    public Texture2D[] noiseTextures;  // 多张黑白噪声图
    public Texture2D[] colorTextures;  // 多张彩色水纹理

    [Header("Water Appearance")]
    public Color waterColor = new Color(0.2f, 0.5f, 1f, 0.8f);
    public Color foamColor = Color.white;
    [Range(0.1f, 5f)] public float noiseScale = 1f;
    [Range(0f, 2f)] public float waveSpeed = 0.5f;
    [Range(0.01f, 0.5f)] public float particleSize = 0.1f;
    [Range(0.1f, 10f)] public float textureChangeSpeed = 1f; // 控制纹理切换速度

    private Material material;
    private ComputeBuffer argsBuffer;
    private Bounds renderBounds;
    private Texture2DArray noiseTexArray;
    private Texture2DArray colorTexArray;
    private float texIndexOffset;

    void Start()
    {
        material = new Material(Shader.Find("Instanced/WaterParticle2D"));
        renderBounds = new Bounds(Vector3.zero, Vector3.one * 20);

        // 创建纹理数组
        CreateTextureArrays();
    }

    void CreateTextureArrays()
    {
        if (noiseTextures.Length > 0 && colorTextures.Length > 0)
        {
            // 创建噪声纹理数组
            noiseTexArray = new Texture2DArray(
                noiseTextures[0].width,
                noiseTextures[0].height,
                noiseTextures.Length,
                noiseTextures[0].format,
                false
            );

            // 创建颜色纹理数组
            colorTexArray = new Texture2DArray(
                colorTextures[0].width,
                colorTextures[0].height,
                colorTextures.Length,
                colorTextures[0].format,
                false
            );

            // 填充噪声纹理数组
            for (int i = 0; i < noiseTextures.Length; i++)
                Graphics.CopyTexture(noiseTextures[i], 0, 0, noiseTexArray, i, 0);

            // 填充颜色纹理数组
            for (int i = 0; i < colorTextures.Length; i++)
                Graphics.CopyTexture(colorTextures[i], 0, 0, colorTexArray, i, 0);

            noiseTexArray.filterMode = FilterMode.Bilinear;
            colorTexArray.filterMode = FilterMode.Bilinear;
        }
        else
        {
            Debug.LogError("Noise or Color Textures array is empty!");
        }
    }

    void Update()
    {
        // 随时间变化纹理索引偏移
        texIndexOffset += Time.deltaTime * textureChangeSpeed;
    }

    void LateUpdate()
    {
        if (material == null || sim == null) return;

        UpdateMaterialProperties();

        if (argsBuffer == null)
            InitializeArgsBuffer();

        Graphics.DrawMeshInstancedIndirect(
            particleMesh,
            0,
            material,
            renderBounds,
            argsBuffer
        );
    }

    void InitializeArgsBuffer()
    {
        argsBuffer = new ComputeBuffer(1, 5 * sizeof(uint), ComputeBufferType.IndirectArguments);
        // 添加ComputeBuffer权限设置
        argsBuffer.SetData(new uint[5] {
        particleMesh.GetIndexCount(0),
        (uint)sim.positionBuffer.count,
        0, 0, 0
    });
    }

    void UpdateMaterialProperties()
    {
        material.SetBuffer("_Positions", sim.positionBuffer);
        material.SetBuffer("_Velocities", sim.velocityBuffer);
        material.SetBuffer("_Densities", sim.densityBuffer);

        // 设置纹理数组
        if (noiseTexArray != null)
            material.SetTexture("_NoiseTexArray", noiseTexArray);
        if (colorTexArray != null)
            material.SetTexture("_ColorTexArray", colorTexArray);

        material.SetFloat("_TexIndexOffset", texIndexOffset);
        material.SetColor("_MainColor", waterColor);
        material.SetColor("_FoamColor", foamColor);
        material.SetFloat("_NoiseScale", noiseScale);
        material.SetFloat("_WaveSpeed", waveSpeed);
        material.SetFloat("_ParticleSize", particleSize);

        // 更新实例数量
        if (argsBuffer != null && sim.positionBuffer != null)
        {
            uint[] currentArgs = new uint[5];
            argsBuffer.GetData(currentArgs);
            if (currentArgs[1] != sim.positionBuffer.count)
            {
                currentArgs[1] = (uint)sim.positionBuffer.count;
                argsBuffer.SetData(currentArgs);
            }
        }
    }

    void OnDestroy()
    {
        if (noiseTexArray != null) Destroy(noiseTexArray);
        if (colorTexArray != null) Destroy(colorTexArray);
        argsBuffer?.Release();
    }

    void OnValidate()
    {
        if (material != null)
            UpdateMaterialProperties();
    }
}