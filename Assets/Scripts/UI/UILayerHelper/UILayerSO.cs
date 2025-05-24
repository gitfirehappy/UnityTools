// UILayerSO.cs
using UnityEngine;

[CreateAssetMenu(fileName = "NewUILayer", menuName = "UI/UILayer", order = 1)]
public class UILayerSO : ScriptableObject
{
    [Header("显示名称")]
    public string displayName = "Default";

    [Header("排序值（越大越靠前）")]
    public int order = 0;
}