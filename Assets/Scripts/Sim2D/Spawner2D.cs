using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;

public class Spawner2D : MonoBehaviour
{
    [Header("手动控制")]
    public Vector2 regionSize = new Vector2(5, 5);    // 区域大小
    public Vector2 regionCenter = Vector2.zero;      // 区域中心
    public Vector2Int gridCount = new Vector2Int(10, 10); // 粒子网格数量
    public float positionJitter = 0.1f;              // 位置随机扰动
    public Color gizmoColor = new Color(0, 1, 1, 0.5f); // 调试颜色

    [Header("初始速度")]
    public Vector2 initialVelocity;

    [Header("调试")]
    public bool showGizmo = true;

    public ParticleSpawnData GetSpawnData()
    {
        var rng = new Unity.Mathematics.Random(42);
        List<float2> positions = new List<float2>();
        List<float2> velocities = new List<float2>();

        for (int y = 0; y < gridCount.y; y++)
        {
            for (int x = 0; x < gridCount.x; x++)
            {
                // 计算标准化坐标 [0,1]
                float tx = gridCount.x > 1 ? x / (float)(gridCount.x - 1) : 0.5f;
                float ty = gridCount.y > 1 ? y / (float)(gridCount.y - 1) : 0.5f;

                // 计算实际位置
                float2 pos = new float2(
                    (tx - 0.5f) * regionSize.x + regionCenter.x,
                    (ty - 0.5f) * regionSize.y + regionCenter.y
                );

                // 添加随机扰动
                float2 jitter = rng.NextFloat2Direction() * positionJitter * rng.NextFloat();
                positions.Add(pos + jitter);
                velocities.Add(initialVelocity);
            }
        }

        return new ParticleSpawnData()
        {
            positions = positions.ToArray(),
            velocities = velocities.ToArray(),
            spawnIndices = new int[positions.Count] // 保持兼容性
        };
    }

    void OnDrawGizmos()
    {
        if (showGizmo && !Application.isPlaying)
        {
            Gizmos.color = gizmoColor;
            Gizmos.DrawWireCube(regionCenter, regionSize);
        }
    }

    public struct ParticleSpawnData
    {
        public float2[] positions;
        public float2[] velocities;
        public int[] spawnIndices;
    }
}