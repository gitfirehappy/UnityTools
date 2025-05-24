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

    #region 闪烁提示动画

    public static void Pulse(Transform target, float scaleMultiplier = 1.2f, float duration = 0.3f)
    {
        Sequence seq = DOTween.Sequence();
        seq.Append(target.DOScale(scaleMultiplier, duration).SetEase(Ease.OutQuad));
        seq.Append(target.DOScale(1f, duration).SetEase(Ease.InQuad));
        seq.SetLoops(-1, LoopType.Yoyo);
    }

    #endregion

    #region 激活面板 & 置顶处理

    /// <summary>
    /// 设置面板为激活状态，并根据层级类型置顶
    /// </summary>
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
            var fa = a.GetComponent<UIFormBase>()?.LayerOrder ?? 0;
            var fb = b.GetComponent<UIFormBase>()?.LayerOrder ?? 0;
            return fa.CompareTo(fb);
        });

        for (int i = 0; i < siblings.Count; i++)
            siblings[i].SetSiblingIndex(i);
    }

    #endregion
}
