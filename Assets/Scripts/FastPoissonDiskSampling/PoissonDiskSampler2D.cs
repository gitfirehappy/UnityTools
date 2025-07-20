using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 简易 2D Poisson Disk 采样器
/// 用于生成一个最小间距的点集合，避免过度密集
/// </summary>
public static class PoissonDiskSampler2D
{
    public static List<Vector2> GeneratePoints(float radius, Vector2 regionSize, int numSamplesBeforeRejection = 30)
    {
        float cellSize = radius / Mathf.Sqrt(2); // 网格单元尺寸
        int[,] grid = new int[Mathf.CeilToInt(regionSize.x / cellSize), Mathf.CeilToInt(regionSize.y / cellSize)];

        List<Vector2> points = new List<Vector2>();//用于存储最终有效的采样点
        List<Vector2> spawnPoints = new List<Vector2>();//存储待处理的 “生成点”，算法会从这些点周围尝试生成新的有效点

        // 初始点：区域中心
        spawnPoints.Add(regionSize / 2);

        while (spawnPoints.Count > 0 && points.Count < 1000)
        {
            int spawnIndex = Random.Range(0, spawnPoints.Count);
            Vector2 spawnCenter = spawnPoints[spawnIndex];//随机选一个点做生成中心
            bool candidateAccepted = false;//是否成功生成一个有效的新点

            for (int i = 0; i < numSamplesBeforeRejection; i++)
            {
                float angle = Random.value * Mathf.PI * 2;
                Vector2 dir = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
                float distance = Random.Range(radius, 2 * radius);
                Vector2 candidate = spawnCenter + dir * distance;

                if (IsValid(candidate, regionSize, cellSize, radius, points, grid))
                {
                    points.Add(candidate);
                    spawnPoints.Add(candidate);
                    int cellX = (int)(candidate.x / cellSize);
                    int cellY = (int)(candidate.y / cellSize);
                    grid[cellX, cellY] = points.Count; // 记录点索引
                    candidateAccepted = true;
                    break;
                }
            }

            if (!candidateAccepted)
            {
                spawnPoints.RemoveAt(spawnIndex);
            }
        }

        return points;
    }

    // 判断 candidate 点是否有效（是否离其他点太近）
    private static bool IsValid(Vector2 candidate, Vector2 regionSize, float cellSize, float radius, List<Vector2> points, int[,] grid)
    {
        if (candidate.x < 0 || candidate.y < 0 || candidate.x >= regionSize.x || candidate.y >= regionSize.y)
            return false;

        int cellX = (int)(candidate.x / cellSize);
        int cellY = (int)(candidate.y / cellSize);

        int searchRadius = 2; // 检查附近 5x5 网格单元
        for (int x = Mathf.Max(0, cellX - searchRadius); x < Mathf.Min(grid.GetLength(0), cellX + searchRadius); x++)
        {
            for (int y = Mathf.Max(0, cellY - searchRadius); y < Mathf.Min(grid.GetLength(1), cellY + searchRadius); y++)
            {
                int pointIndex = grid[x, y] - 1;
                if (pointIndex >= 0 && pointIndex < points.Count)
                {
                    float sqrDist = (candidate - points[pointIndex]).sqrMagnitude;
                    if (sqrDist < radius * radius)
                        return false;
                }
            }
        }

        return true;
    }
}
