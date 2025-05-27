using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 通用场景切换控制器（支持条件判断、保留场景 BuildIndex、自动卸载）
/// </summary>
public class SceneLoader : SingletonMono<SceneLoader>
{
    [Header("这些 Scene BuildIndex 将被视为常驻场景（不会被卸载）")]
    public List<int> persistentSceneIndexes = new List<int> { 0 }; // 默认保留第一个场景

    /// <summary>
    /// 异步加载目标场景，同时卸载所有非保留场景
    /// </summary>
    /// <param name="targetScene">目标场景名称</param>
    /// <param name="conditions">切换条件（可选）</param>
    /// <param name="onComplete">完成回调（可选）</param>
    public void LoadScene(string targetScene, List<Func<bool>> conditions = null, Action onComplete = null)
    {
        StartCoroutine(LoadSceneRoutine(targetScene, conditions, onComplete));
    }

    private IEnumerator LoadSceneRoutine(string targetScene, List<Func<bool>> conditions, Action onComplete)
    {
        // 检查条件
        if (conditions != null)
        {
            foreach (var cond in conditions)
            {
                if (!cond.Invoke())
                {
                    Debug.LogWarning($"[SceneLoader] 条件未满足，取消切换：{cond.Method.Name}");
                    yield break;
                }
            }
        }

        Debug.Log($"[SceneLoader] 开始加载场景：{targetScene}");

        // 异步加载目标场景（Additive）
        AsyncOperation loadOp = SceneManager.LoadSceneAsync(targetScene, LoadSceneMode.Additive);
        while (!loadOp.isDone)
            yield return null;

        Scene target = SceneManager.GetSceneByName(targetScene);
        if (target.IsValid())
            SceneManager.SetActiveScene(target);

        // 卸载所有非持久化场景和非目标场景
        for (int i = 0; i < SceneManager.sceneCount; i++)
        {
            Scene loadedScene = SceneManager.GetSceneAt(i);
            if (!loadedScene.isLoaded || loadedScene.name == targetScene)
                continue;

            int index = SceneUtility.GetBuildIndexByScenePath(loadedScene.path);
            if (persistentSceneIndexes.Contains(index))
                continue;

            Debug.Log($"[SceneLoader] 卸载场景：{loadedScene.name}");
            AsyncOperation unloadOp = SceneManager.UnloadSceneAsync(loadedScene);
            while (!unloadOp.isDone)
                yield return null;
        }

        Debug.Log($"[SceneLoader] 成功切换到场景：{targetScene}");
        onComplete?.Invoke();
    }
}
