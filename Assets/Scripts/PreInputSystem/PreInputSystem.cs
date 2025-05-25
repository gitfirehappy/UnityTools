using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PreInputSystem : MonoBehaviour
{
    private InputCommand currentCommand;

    private void FixedUpdate()
    {
        if (currentCommand == null) return;

        //如果过期就清除
        if (currentCommand.IsExpired)
        {
            currentCommand = null;
            return;
        }

        //条件满足就执行
        if(currentCommand.Condition != null && currentCommand.Condition())
        {
            currentCommand.Execute?.Invoke();
            currentCommand = null;
        }
    }
    /// <summary>
    /// 尝试注册一个新操作，后者会覆盖前者
    /// </summary>
    public void RegisterCommand(InputCommand newCommand)
    {
        currentCommand = newCommand;
    }

}

public enum InputCommandType
{
    Jump,
    Dash,
    Attack
}

public class InputCommand
{
    public InputCommandType Type;
    public float Timestamp;
    public float BufferTime;
    public Func<bool> Condition; // 条件满足才能触发
    public Action Execute;       // 真正的操作

    public bool IsExpired => Time.time > Timestamp + BufferTime;
}
