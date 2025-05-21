using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 通用事件中心，支持字符串事件名注册与触发。
/// </summary>
public static class EventCenter
{
    #region 字段

    private static Dictionary<string, Delegate> eventTable = new();

    // 运行时追踪堆栈，用于编辑器查看
    private static Dictionary<string, List<string>> _listenerStackTraces = new();
    private static Dictionary<string, List<string>> _removerStackTraces = new();

    #endregion

    #region 添加监听器

    public static void AddListener(string eventName, Action callback)
    {
        if (!eventTable.ContainsKey(eventName))
            eventTable[eventName] = null;
        eventTable[eventName] = (Action)eventTable[eventName] + callback;

        if (!_listenerStackTraces.ContainsKey(eventName))
            _listenerStackTraces[eventName] = new List<string>();
        _listenerStackTraces[eventName].Add(Environment.StackTrace);
    }

    public static void AddListener<T>(string eventName, Action<T> callback)
    {
        if (!eventTable.ContainsKey(eventName))
            eventTable[eventName] = null;
        eventTable[eventName] = (Action<T>)eventTable[eventName] + callback;

        if (!_listenerStackTraces.ContainsKey(eventName))
            _listenerStackTraces[eventName] = new List<string>();
        _listenerStackTraces[eventName].Add(Environment.StackTrace);
    }

    #endregion

    #region 移除监听器

    public static void RemoveListener(string eventName, Action callback)
    {
        if (eventTable.ContainsKey(eventName))
        {
            eventTable[eventName] = (Action)eventTable[eventName] - callback;

            if (!_removerStackTraces.ContainsKey(eventName))
                _removerStackTraces[eventName] = new List<string>();
            _removerStackTraces[eventName].Add(Environment.StackTrace);
        }
    }

    public static void RemoveListener<T>(string eventName, Action<T> callback)
    {
        if (eventTable.ContainsKey(eventName))
        {
            eventTable[eventName] = (Action<T>)eventTable[eventName] - callback;

            if (!_removerStackTraces.ContainsKey(eventName))
                _removerStackTraces[eventName] = new List<string>();
            _removerStackTraces[eventName].Add(Environment.StackTrace);
        }
    }

    #endregion

    #region 触发事件

    public static void Trigger(string eventName)
    {
        if (eventTable.TryGetValue(eventName, out var d))
        {
            (d as Action)?.Invoke();
        }
    }

    public static void Trigger<T>(string eventName, T arg)
    {
        if (eventTable.TryGetValue(eventName, out var d))
        {
            (d as Action<T>)?.Invoke(arg);
        }
    }

    #endregion

    #region 编辑器辅助接口

    public static Dictionary<string, Delegate> GetAllEvents() => eventTable;
    public static Dictionary<string, List<string>> GetListenerTraces() => _listenerStackTraces;
    public static Dictionary<string, List<string>> GetRemoverTraces() => _removerStackTraces;

    #endregion
}