using UnityEngine;
using System;

/// <summary>
/// 所有 UI 面板应继承此类，内置动画、生命周期控制
/// </summary>
public class UIFormBase : MonoBehaviour, IUIForm
{
    protected UIManager uIManager;

    public bool Cached = false;
    public bool IsOpen = false;
    public bool IsInited = false;

    public FormType formType = FormType.None;
    public FormAnimType formAnimType = FormAnimType.None;

    private void Awake()
    {
        if (!gameObject.scene.IsValid()) return;

        IUIForm ui = this;
        ui.RegisterForm();

        // 场景面板默认关闭，不闪烁
        if (gameObject.activeSelf)
        {
            CloseImmediate();
        }

        // Fade面板初始化CanvasGroup alpha为0，保证第一次打开时能淡入
        if (formAnimType == FormAnimType.Fade)
        {
            var cg = gameObject.GetComponent<CanvasGroup>();
            if (cg == null)
            {
                cg = gameObject.AddComponent<CanvasGroup>();
            }
            cg.alpha = 0f;
        }
    }

    private void OnDestroy()
    {
        IUIForm ui = this;
        ui.UnRegisterForm();
    }

    public void Open(UIManager uIManager)
    {
        this.uIManager = uIManager;
        if (!IsInited)
        {
            IsInited = true;
            Init(); // 首次打开时初始化
        }

        OpenAnim();
    }

    public void Close()
    {
        if (!IsOpen) return;

        CloseAnim();
    }

    /// <summary>
    /// 直接关闭且不播放动画，内部调用（场景面板默认关闭用）
    /// </summary>
    private void CloseImmediate()
    {
        // 若是Fade动画，关闭时alpha也要重置为0，确保下次打开能淡入
        if (formAnimType == FormAnimType.Fade)
        {
            var cg = gameObject.GetComponent<CanvasGroup>();
            if (cg != null)
            {
                cg.alpha = 0f;
            }
        }

        gameObject.SetActive(false);
        IsOpen = false;
    }

    /// <summary>
    /// 初始化方法，面板逻辑初始化应重写此方法
    /// </summary>
    protected virtual void Init() { }

    private void OpenAnim()
    {
        switch (formAnimType)
        {
            case FormAnimType.None:
                gameObject.SetActive(true);
                IsOpen = true;
                break;
            case FormAnimType.Fade:
                UIAnimation.FadeIn(this, () => IsOpen = true);
                break;
            case FormAnimType.Zoom:
                UIAnimation.ZoomIn(this, () => IsOpen = true);
                break;
        }
    }

    private void CloseAnim()
    {
        Action onCloseComplete = () =>
        {
            IsOpen = false;

            if (!Cached)
            {
                UIManager.Instance.UnRegisterForm(this);
                Destroy(gameObject);
            }
        };

        switch (formAnimType)
        {
            case FormAnimType.None:
                gameObject.SetActive(false);
                onCloseComplete();
                break;
            case FormAnimType.Fade:
                UIAnimation.FadeOut(this, onCloseComplete);
                break;
            case FormAnimType.Zoom:
                UIAnimation.ZoomOut(this, onCloseComplete);
                break;
        }
    }

    public UIFormBase GetUIFormBase() => this;
}
