using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// UI 管理器（单例）
/// 负责动态加载、注册、显示、隐藏、回收 UI 面板
/// </summary>
public class UIManager : Singleton<UIManager>
{
    #region 字段

    /// <summary> 路径映射模板（Key = 面板名，Value = Resources 路径） </summary>
    public Dictionary<string, GameObject> formPrefabs = new();

    /// <summary> 当前已注册的实例面板 </summary>
    public Dictionary<string, UIFormBase> forms = new();

    /// <summary> 当前正在显示的面板 </summary>
    public List<UIFormBase> showForms = new();

    /// <summary> 面板显示堆栈（用于顺序关闭） </summary>
    public Stack<UIFormBase> showFormStack = new();

    /// <summary> UI 根节点（场景中应存在名为 UIRoot 的对象） </summary>
    public Transform uiRoot => GameObject.Find("UIRoot").transform;

    #endregion

    #region 注册接口

    public void RegisterForm(IUIForm uIForm)
    {
        var form = uIForm.GetUIFormBase();
        string key = form.GetType().Name;

        if (!forms.ContainsKey(key))
        {
            forms.Add(key, form);
            form.Close(); // 默认关闭
        }
    }

    public void UnRegisterForm(IUIForm uIForm)
    {
        var form = uIForm.GetUIFormBase();
        string key = form.GetType().Name;

        if (forms.ContainsKey(key))
        {
            showForms.Remove(form);

            var tmpStack = new Stack<UIFormBase>();
            while (showFormStack.Count > 0)
            {
                var top = showFormStack.Pop();
                if (top != form) tmpStack.Push(top);
            }
            showFormStack = new Stack<UIFormBase>(tmpStack);

            forms.Remove(key);
        }
    }

    /// <summary>
    /// 【新增】手动注册已有面板实例（用于场景中已有面板）
    /// </summary>
    public void RegisterFormInstance(UIFormBase formInstance)
    {
        string key = formInstance.GetType().Name;
        if (!forms.ContainsKey(key))
        {
            forms.Add(key, formInstance);
            formInstance.Close(); // 默认关闭
        }
    }

    #endregion

    #region 显示与隐藏

    public void ShowUIForm(string name)
    {
        if (!forms.ContainsKey(name))
        {
            CreateForm(name);
            if (!forms.ContainsKey(name)) return;
        }

        var form = forms[name];
        if (form != null && !showForms.Contains(form))
        {
            form.Open(this);
            showForms.Add(form);
            showFormStack.Push(form);
        }
    }

    public void ShowUIForm<T>() where T : UIFormBase => ShowUIForm(typeof(T).Name);

    public void HideUIForm(string name)
    {
        var form = GetForm(name);
        if (form != null && showForms.Contains(form))
        {
            showForms.Remove(form);
            form.Close();
        }
    }

    public void HideUIForm<T>() where T : UIFormBase => HideUIForm(typeof(T).Name);

    public void HideUIFormTurn()
    {
        if (showFormStack.Count > 0)
        {
            var form = showFormStack.Pop();
            HideUIForm(form.name);
        }
    }

    public void HideAllUIForm()
    {
        foreach (var form in showForms)
            form.Close();

        showForms.Clear();
        showFormStack.Clear();
    }

    public bool HasActiveForm() => showForms.Count > 0;

    #endregion

    #region 资源预加载（推荐使用路径）

    public void PreLoadForm(string name, string path)
    {
        if (!formPrefabs.ContainsKey(name))
        {
            var prefab = Resources.Load<GameObject>(path);
            if (prefab != null)
            {
                formPrefabs.Add(name, prefab);
            }
            else
            {
                Debug.LogError($"[UIManager] Load failed: {path}");
            }
        }
    }

    public void PreLoadForms(Dictionary<string, string> namePathPairs)
    {
        foreach (var kv in namePathPairs)
        {
            PreLoadForm(kv.Key, kv.Value);
        }
    }

    private void CreateForm(string name)
    {
        if (formPrefabs.ContainsKey(name))
        {
            var formObj = GameObject.Instantiate(formPrefabs[name], uiRoot);
            formObj.name = name;
        }
        else
        {
            Debug.LogError($"[UIManager] CreateForm Failed: no prefab named {name}");
        }
    }

    public void PreLoadForm(GameObject prefab)
    {
        if (prefab == null) return;
        if (!formPrefabs.ContainsKey(prefab.name))
        {
            formPrefabs.Add(prefab.name, prefab);
        }
    }

    public void PreLoadForms(GameObject[] prefabs)
    {
        foreach (var prefab in prefabs)
        {
            PreLoadForm(prefab);
        }
    }

    #endregion

    #region 快捷访问

    public UIFormBase GetForm(string name) => forms.TryGetValue(name, out var f) ? f : null;

    public T GetForm<T>() where T : UIFormBase => GetForm(typeof(T).Name) as T;

    public bool IsShown(string name) => GetForm(name)?.IsOpen ?? false;

    public UIFormBase TryShowForm(string name)
    {
        ShowUIForm(name);
        return GetForm(name);
    }

    #endregion
}


public interface IUIForm
{
    void RegisterForm() => UIManager.Instance.RegisterForm(this);
    void UnRegisterForm() => UIManager.Instance.UnRegisterForm(this);
    UIFormBase GetUIFormBase();
}

public enum FormAnimType
{
    None,
    Fade,
    Zoom,
    Pop,
    SlideLeft,
    SlideRight,
    SlideUp,
    SlideDown,
    FadeSlide,

}


