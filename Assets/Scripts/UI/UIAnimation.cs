using System;
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

    #region 激活面板 & 置顶处理

    /// <summary>
    /// 设置面板为激活状态，并根据层级类型置顶
    /// </summary>
    public static void FormActiveByType(UIFormBase formBase)
    {
        var obj = formBase.gameObject;
        obj.SetActive(true);
        switch (formBase.formType)
        {
            case FormType.Top:
                obj.transform.SetAsLastSibling();
                break;
        }
    }

    #endregion
}
