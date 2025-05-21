using UnityEngine;

/// <summary>
/// 测试事件监听注册，用于验证 EventCenter 是否被 EventCenterViewer 扫描到
/// </summary>
public class TestEventListener : MonoBehaviour
{
    void Start()
    {
        /// 注册无参事件监听
        EventCenter.AddListener("OnTestEvent", OnTestEventReceived);

        /// 注册带参事件监听
        EventCenter.AddListener<int>("OnHealthChanged", OnHealthChanged);

        /// 手动触发一次测试事件
        EventCenter.Trigger("OnTestEvent");
        EventCenter.Trigger("OnHealthChanged", 100);
    }

    void OnDestroy()
    {
        /// 移除监听，良好习惯
        EventCenter.RemoveListener("OnTestEvent", OnTestEventReceived);
        EventCenter.RemoveListener<int>("OnHealthChanged", OnHealthChanged);
    }

    /// <summary>
    /// 接收到测试事件
    /// </summary>
    void OnTestEventReceived()
    {
        Debug.Log("收到 OnTestEvent");
    }

    /// <summary>
    /// 血量变更事件回调
    /// </summary>
    void OnHealthChanged(int newHp)
    {
        Debug.Log("血量变为: " + newHp);
    }
}
