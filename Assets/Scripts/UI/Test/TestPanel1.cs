using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestPanel1 : UIFormBase
{
    private void Start()
    {
        // 手动注册场景中已有面板实例，加入 UIManager 管理
        UIManager.Instance.RegisterFormInstance(this);

        // 也可以自动注册 Awake 里调用
    }
}
