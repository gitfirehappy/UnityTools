using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using Unity.VisualScripting;
using static UnityEngine.Rendering.HableCurve;

public class UIAnimation
{   
    public static void FadeIn(UIFormBase uIForm, float duration = 0.5f)
    {   
        FormActiveByType(uIForm);
        uIForm.gameObject.GetOrAddComponent<CanvasGroup>().DOFade(1, duration).OnComplete(() =>
        {

        });
    }
    public static void FadeOut(UIFormBase uIForm, float duration = 0.5f)
    {
        uIForm.gameObject.GetOrAddComponent<CanvasGroup>().DOFade(0, duration).OnComplete(() =>
        {
            uIForm.gameObject.SetActive(false);
        });
    }
    public static void ZoomIn(UIFormBase uIForm, float duration = 0.5f)
    {
        FormActiveByType(uIForm);
        uIForm.gameObject.transform.DOScale(1, duration).OnComplete(() =>
        {
            
        });
    }
    public static void ZoomOut(UIFormBase uIForm, float duration = 0.5f)
    {
        uIForm.gameObject.transform.DOScale(0, duration).OnComplete(() =>
        {
            uIForm.gameObject.SetActive(false);
        });
    }

    public static void FormActiveByType(UIFormBase formBase)
    {
        var obj = formBase.gameObject;
        obj.gameObject.SetActive(true);
        switch (formBase.formType)
        {
            case FormType.Top:
                obj.transform.SetAsLastSibling();
                break;
        }
    }
}
