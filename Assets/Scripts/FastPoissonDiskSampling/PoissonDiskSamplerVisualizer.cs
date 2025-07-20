using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 可视化测试 Poisson Disk 采样算法的 MonoBehaviour 脚本
/// 用于学习和调试采样分布效果
/// </summary>
public class PoissonDiskSamplerVisualizer : MonoBehaviour
{
    [Header("采样参数")]
    public float sampleRadius = 1.5f;         // 采样最小间距
    public Vector2 regionSize = new Vector2(20, 20); // 区域尺寸
    public int rejectionSamples = 30;         // 每个 spawn 点尝试次数

    [Header("点可视化")]
    public float pointDisplayRadius = 0.15f;  // Gizmo 点大小
    private List<Vector2> points;             // 最终采样结果

    void OnDrawGizmos()
    {
        Gizmos.color = Color.gray;
        Gizmos.DrawWireCube(transform.position, regionSize); // 显示区域边框

        if (points == null) return;

        Gizmos.color = Color.cyan;
        foreach (var point in points)
        {
            // 将本地坐标偏移区域中心以进行可视化
            Vector3 worldPoint = transform.position - (Vector3)regionSize / 2f + (Vector3)point;
            Gizmos.DrawSphere(worldPoint, pointDisplayRadius);

            // 绘制每个采样点的有效范围
            //Gizmos.color = Color.yellow;
            //Gizmos.DrawWireSphere(worldPoint, sampleRadius);
        }
    }

    /// <summary>
    /// 在游戏开始时自动采样
    /// </summary>
    void Start()
    {
        points = PoissonDiskSampler2D.GeneratePoints(sampleRadius, regionSize, rejectionSamples);
        Debug.Log($"[Sampler] 生成点数: {points.Count}");
    }
}