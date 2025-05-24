using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public static class UIAnimation
{
    #region 淡入淡出动画

    /// <summary>
    /// 淡入动画，结束时调用onComplete
    /// </summary>
    public static void FadeIn(UIFormBase uIForm, Action onComplete = null, float duration = 0.5f)
    {
        FormActiveByType(uIForm);
        var cg = uIForm.gameObject.GetComponent<CanvasGroup>();
        if (cg != null)
        {
            cg.DOFade(1, duration).OnComplete(() => onComplete?.Invoke());
        }
        else
        {
            onComplete?.Invoke();
        }
    }

    /// <summary>
    /// 淡出动画，附带完成回调
    /// </summary>
    public static void FadeOut(UIFormBase uIForm, Action onComplete, float duration = 0.5f)
    {
        var cg = uIForm.gameObject.GetComponent<CanvasGroup>();
        if (cg != null)
        {
            cg.DOFade(0, duration).OnComplete(() =>
            {
                uIForm.gameObject.SetActive(false);
                onComplete?.Invoke();
            });
        }
        else
        {
            uIForm.gameObject.SetActive(false);
            onComplete?.Invoke();
        }
    }

    #endregion

    #region 缩放动画

    /// <summary>
    /// 缩放进入动画，结束时调用onComplete
    /// </summary>
    public static void ZoomIn(UIFormBase uIForm, Action onComplete = null, float duration = 0.5f)
    {
        FormActiveByType(uIForm);
        uIForm.transform.localScale = Vector3.zero;
        uIForm.transform.DOScale(1, duration).OnComplete(() => onComplete?.Invoke());
    }

    /// <summary>
    /// 缩放关闭动画，附带完成回调
    /// </summary>
    public static void ZoomOut(UIFormBase uIForm, Action onComplete = null, float duration = 0.5f)
    {
        uIForm.transform.DOScale(0, duration).OnComplete(() =>
        {
            uIForm.gameObject.SetActive(false);
            onComplete?.Invoke();
        });
    }

    #endregion

    #region 弹跳动画

    public static void PopIn(UIFormBase uIForm, Action onComplete = null, float duration = 0.5f)
    {
        FormActiveByType(uIForm);
        uIForm.transform.localScale = Vector3.zero;
        uIForm.transform.DOScale(1f, duration).SetEase(Ease.OutBack).OnComplete(() => onComplete?.Invoke());
    }

    public static void PopOut(UIFormBase uIForm, Action onComplete = null, float duration = 0.3f)
    {
        uIForm.transform.DOScale(0f, duration).SetEase(Ease.InBack).OnComplete(() =>
        {
            uIForm.gameObject.SetActive(false);
            onComplete?.Invoke();
        });
    }

    #endregion

    #region 滑入滑出动画

    public static void SlideIn(UIFormBase uIForm, Vector3 fromOffset, Action onComplete = null, float duration = 0.5f)
    {
        FormActiveByType(uIForm);
        var t = uIForm.transform;
        Vector3 targetPos = ((UIFormBase)uIForm).originalLocalPos;
        t.localPosition = targetPos + fromOffset;
        t.DOLocalMove(targetPos, duration).SetEase(Ease.OutCubic).OnComplete(() => onComplete?.Invoke());
    }

    public static void SlideOut(UIFormBase uIForm, Vector3 toOffset, Action onComplete = null, float duration = 0.5f)
    {
        var t = uIForm.transform;
        Vector3 startPos = ((UIFormBase)uIForm).originalLocalPos; //使用缓存位置
        Vector3 targetPos = startPos + toOffset;
        t.DOLocalMove(targetPos, duration).SetEase(Ease.InCubic).OnComplete(() =>
        {
            uIForm.gameObject.SetActive(false);
            t.localPosition = startPos; //复位
            onComplete?.Invoke();
        });
    }

    #endregion

    #region 淡入 + 滑动组合动画

    public static void FadeSlideIn(UIFormBase uIForm, Vector3 fromOffset, Action onComplete = null, float duration = 0.5f)
    {
        FormActiveByType(uIForm);
        var t = uIForm.transform;
        var cg = uIForm.GetComponent<CanvasGroup>() ?? uIForm.gameObject.AddComponent<CanvasGroup>();
        cg.alpha = 0;
        Vector3 originalPos = t.localPosition;
        t.localPosition = originalPos + fromOffset;

        Sequence seq = DOTween.Sequence();
        seq.Join(cg.DOFade(1, duration));
        seq.Join(t.DOLocalMove(originalPos, duration).SetEase(Ease.OutQuad));
        seq.OnComplete(() => onComplete?.Invoke());
    }

    #endregion

    #region 闪烁提示动画(闲置）

    /// <summary>
    /// 开始闪烁动画（自动激活对象）
    /// </summary>
    public static void PulseIn(UIFormBase uIForm, Action onComplete = null, float scaleMultiplier = 1.2f, float duration = 0.3f)
    {
        FormActiveByType(uIForm); // 处理层级排序
        var target = uIForm.transform;

        target.DOKill(); // 停止之前的动画
        target.localScale = Vector3.one; // 重置缩放

        Sequence seq = DOTween.Sequence();
        seq.Append(target.DOScale(scaleMultiplier, duration).SetEase(Ease.OutQuad));
        seq.Append(target.DOScale(1f, duration).SetEase(Ease.InQuad));
        seq.SetLoops(-1, LoopType.Yoyo);
        seq.OnPlay(() => {
            target.gameObject.SetActive(true); // 动画开始时激活
        });
        seq.OnComplete(() => onComplete?.Invoke());
    }

    /// <summary>
    /// 停止闪烁动画（自动禁用对象）
    /// </summary>
    public static void PulseOut(UIFormBase uIForm, Action onComplete = null, float fadeDuration = 0.2f)
    {
        var target = uIForm.transform;

        // 停止所有动画但不立即完成
        target.DOKill();

        // 快速平滑地恢复原始大小
        target.DOScale(1f, fadeDuration)
            .OnComplete(() => {
                target.gameObject.SetActive(false);
                onComplete?.Invoke();
            })
            .SetUpdate(true);
    }

    #endregion

    #region 激活面板 & 置顶处理

    /// <summary>
    /// 设置面板为激活状态，并根据层级类型置顶
    /// </summary>
    // 在UIAnimation.cs中修改FormActiveByType方法
    public static void FormActiveByType(UIFormBase formBase)
    {
        var obj = formBase.gameObject;
        obj.SetActive(true);

        var parent = obj.transform.parent;
        if (parent == null) return;

        var siblings = new List<Transform>();
        for (int i = 0; i < parent.childCount; i++)
            siblings.Add(parent.GetChild(i));

        siblings.Sort((a, b) =>
        {
            var fa = a.GetComponent<UIFormBase>();
            var fb = b.GetComponent<UIFormBase>();

            if (fa == null && fb == null) return 0;
            if (fa == null) return -1;
            if (fb == null) return 1;

            // 先比较大层级数字
            int majorCompare = fa.MajorLayerOrder.CompareTo(fb.MajorLayerOrder);
            if (majorCompare != 0) return majorCompare;

            // 大层级相同再比较小层级数字
            return fa.MinorLayerOrder.CompareTo(fb.MinorLayerOrder);
        });

        for (int i = 0; i < siblings.Count; i++)
            siblings[i].SetSiblingIndex(i);
    }

    #endregion
}
