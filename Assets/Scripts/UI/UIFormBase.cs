using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIFormBase : MonoBehaviour, IUIForm
{
    private UIManager uIManager;

    public bool IsOpen;

    public FormType formType = FormType.None;

    public FormAnimType formAnimType;//动画类型
    private void Awake()
    {
        IUIForm uIForm = this;
        uIForm.RegisterForm();
    }
    private void OnDestroy()
    {
        IUIForm uIForm = this;
        uIForm.UnRegisterForm();
    }

    public void Open(UIManager uIManager)
    {
        this.uIManager = uIManager;
        IsOpen = true;
        OpenAnim();
    }
    public void Close()
    {
        IsOpen = false;
        CloseAnim();
    }

    private void OpenAnim()
    {
        switch (formAnimType)
        {
            case FormAnimType.None:
                gameObject.SetActive(true);
                break;
            case FormAnimType.Fade:
                UIAnimation.FadeIn(this);
                break;
            case FormAnimType.Zoom:
                UIAnimation.ZoomIn(this);
                break;
        }
    }
    private void CloseAnim()
    {
        switch (formAnimType)
        {
            case FormAnimType.None:
                gameObject.SetActive(false);
                break;
            case FormAnimType.Fade:
                UIAnimation.FadeOut(this);
                break;
            case FormAnimType.Zoom:
                UIAnimation.ZoomOut(this);
                break;
        }
    }
    public UIFormBase GetUIFormBase()
    {
        return this;
    }
}
