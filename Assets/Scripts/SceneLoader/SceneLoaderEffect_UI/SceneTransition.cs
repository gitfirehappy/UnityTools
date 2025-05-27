using UnityEngine;
using System;
using System.Collections.Generic;

/// <summary>
/// 场景切换动画类型
/// </summary>
public enum SceneTransitionType
{
    None,
    Fade,
    // 未来可以在这里扩展更多类型
}

/// <summary>
/// 场景切换过渡效果控制器
/// 使用方法：
/// 1. 创建一个全屏黑色图片的预制体
/// 2. 挂载此脚本
/// 3. 设置 formAnimType = FormAnimType.Fade
/// 4. 不勾选 Cached
/// </summary>
public class SceneTransition : UIFormBase
{
    private static SceneTransition instance;
    public static SceneTransition Instance
    {
        get
        {
            if (instance == null)
            {
                var obj = UIManager.Instance.TryShowForm(nameof(SceneTransition));
                instance = obj as SceneTransition;
            }
            return instance;
        }
    }

    protected override void Init()
    {
        base.Init();
        formAnimType = FormAnimType.Fade;
    }

    /// <summary>
    /// 执行场景切换动画
    /// </summary>
    public void PlayTransition(Action onFadeInComplete, Action onFadeOutComplete)
    {
        // 显示黑色遮罩（淡入）
        UIManager.Instance.ShowUIForm<SceneTransition>();

        StartCoroutine(WaitForFadeInThenCallback());

        System.Collections.IEnumerator WaitForFadeInThenCallback()
        {
            yield return new WaitForSeconds(0.5f); // 与淡入动画时长匹配
            onFadeInComplete?.Invoke();

            // 等待场景加载完成后的回调来执行淡出
            onFadeOutComplete += () => UIManager.Instance.HideUIForm<SceneTransition>();
        }
    }
}

/// <summary>
/// SceneLoader的场景切换动画扩展方法
/// </summary>
public static class SceneLoaderExtension
{
    /// <summary>
    /// 使用动画效果加载场景
    /// </summary>
    /// <param name="sceneLoader">SceneLoader实例</param>
    /// <param name="targetScene">目标场景名称</param>
    /// <param name="transitionType">过渡动画类型</param>
    /// <param name="conditions">切换条件（可选）</param>
    /// <param name="onComplete">完成回调（可选）</param>
    public static void LoadSceneWithTransition(
        this SceneLoader sceneLoader,
        string targetScene,
        SceneTransitionType transitionType = SceneTransitionType.None,
        List<Func<bool>> conditions = null,
        Action onComplete = null)
    {
        switch (transitionType)
        {
            case SceneTransitionType.None:
                sceneLoader.LoadScene(targetScene, conditions, onComplete);
                break;

            case SceneTransitionType.Fade:
                SceneTransition.Instance.PlayTransition(
                    // 淡入完成后加载场景
                    onFadeInComplete: () => sceneLoader.LoadScene(targetScene, conditions, onComplete),
                    // 场景加载完成后的回调
                    onFadeOutComplete: onComplete
                );
                break;
        }
    }
} 