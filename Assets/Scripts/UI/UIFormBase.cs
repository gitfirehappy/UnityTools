using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIFormBase : MonoBehaviour, IUIForm
{
    private UIManager uIManager;

    public bool IsOpen;
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
        gameObject.SetActive(true);
    }
    public void Close()
    {
        IsOpen = false;
        gameObject.SetActive(false);
    }
    public UIFormBase GetUIFormBase()
    {
        return this;
    }
}
