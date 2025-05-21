using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIManager : Singleton<UIManager>
{
    public Dictionary<string,UIFormBase> forms = new Dictionary<string,UIFormBase>();

    public List<UIFormBase> showForms = new List<UIFormBase>();

    public Transform uiRoot
    {
        get { return GameObject.Find("UIRoot").transform; }
    }

    public void RegisterForm(IUIForm uIForm)
    {
        var form = uIForm.GetUIFormBase();
        if (!forms.ContainsKey(form.name))
        {
            forms.Add(form.name, form);
            form.Close();//默认关闭状态
        }
        else
        {
            forms[form.name] = form;
        }
    }
    public void UnRegisterForm(IUIForm uIForm)
    {
        var form = uIForm.GetUIFormBase();
        if (!forms.ContainsKey(form.name))
        {
            forms.Add(form.name, form);
        }
        else
        {
            forms.Remove(form.name);
        }
    }

    public void ShowUIForm(string name)
    {
        if (!forms.ContainsKey(name))
        {
            return;
        }
        var form = forms[name];
        form.Open(this);
        showForms.Add(form);
    } 

    public void HideUIForm(string name)
    {
        var form = forms[name];
        if (showForms.Contains(form))
        {
            showForms.Remove(form);
            form.Close();
        }
    }
}

public interface IUIForm
{
    void RegisterForm() => UIManager.Instance.RegisterForm(this);
    void UnRegisterForm() => UIManager.Instance.RegisterForm(this);

    UIFormBase GetUIFormBase();
}
