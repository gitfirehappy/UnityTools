// UILayerSO.cs
using UnityEngine;

[CreateAssetMenu(fileName = "NewUILayer", menuName = "UI/UILayer", order = 1)]
public class UILayerSO : ScriptableObject
{
    [Header("显示名称")]
    public string displayName = "Default";

    [Header("大层级数字（主排序）")]
    public int majorOrder = 0;

    [Header("小层级数字（次排序）")]
    public int minorOrder = 0;
}