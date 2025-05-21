using UnityEditor;
using UnityEngine;
using System.IO;
using System.Collections.Generic;

/// <summary>
/// 查看运行时事件监听者信息（调试使用）
/// </summary>
public class EventCenterViewer : EditorWindow
{
    #region 字段

    private Vector2 scrollPos;
    private Dictionary<string, List<string>> _listenerCache;
    private Dictionary<string, List<string>> _removerCache;

    #endregion

    #region 初始化

    [MenuItem("Tools/Event Center Viewer (Runtime)")]
    public static void Open()
    {
        GetWindow<EventCenterViewer>("事件监听查看器");
    }

    private void OnEnable()
    {
        RefreshData();
    }

    private void RefreshData()
    {
        _listenerCache = EventCenter.GetListenerTraces();
        _removerCache = EventCenter.GetRemoverTraces();
    }

    #endregion

    #region GUI

    private void OnGUI()
    {
        if (GUILayout.Button("🔄 刷新监听信息"))
        {
            RefreshData();
        }

        scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

        foreach (var kv in _listenerCache)
        {
            GUILayout.Space(10);
            GUILayout.Label($"🎯 事件: {kv.Key}", EditorStyles.boldLabel);

            GUILayout.Label("注册位置:");
            foreach (string trace in kv.Value)
            {
                if (TryExtractFileInfo(trace, out string filePath, out int line))
                {
                    EditorGUILayout.BeginHorizontal();
                    GUI.color = Color.green;
                    if (GUILayout.Button($"{Path.GetFileName(filePath)}:{line}", GUILayout.Width(150)))
                    {
                        var script = AssetDatabase.LoadAssetAtPath<MonoScript>(filePath);
                        if (script != null)
                            AssetDatabase.OpenAsset(script, line);
                    }
                    GUI.color = Color.white;
                    GUILayout.Label(ExtractFirstMethodLine(trace));
                    EditorGUILayout.EndHorizontal();
                }
                else
                {
                    GUILayout.TextArea(trace, GUILayout.Height(60));
                }
            }

            if (_removerCache != null && _removerCache.TryGetValue(kv.Key, out var removes))
            {
                GUILayout.Label("移除位置:");
                foreach (string trace in removes)
                {
                    if (TryExtractFileInfo(trace, out string filePath, out int line))
                    {
                        EditorGUILayout.BeginHorizontal();
                        GUI.color = Color.red;
                        if (GUILayout.Button($"{Path.GetFileName(filePath)}:{line}", GUILayout.Width(150)))
                        {
                            var script = AssetDatabase.LoadAssetAtPath<MonoScript>(filePath);
                            if (script != null)
                                AssetDatabase.OpenAsset(script, line);
                        }
                        GUI.color = Color.white;
                        GUILayout.Label(ExtractFirstMethodLine(trace));
                        EditorGUILayout.EndHorizontal();
                    }
                    else
                    {
                        GUILayout.TextArea(trace, GUILayout.Height(60));
                    }
                }
            }
        }

        EditorGUILayout.EndScrollView();
    }

    #endregion

    #region 堆栈信息处理

    private bool TryExtractFileInfo(string stackTrace, out string path, out int lineNumber)
    {
        path = null;
        lineNumber = 0;

        var lines = stackTrace.Split('\n');
        foreach (var line in lines)
        {
            if (line.Contains(".cs:line "))
            {
                int startIndex = line.IndexOf("Assets/");
                int endIndex = line.IndexOf(":line ");
                if (startIndex >= 0 && endIndex > startIndex)
                {
                    path = line.Substring(startIndex, endIndex - startIndex);
                    string lineStr = line[(endIndex + 6)..];
                    int.TryParse(lineStr, out lineNumber);
                    return true;
                }
            }
        }
        return false;
    }

    private string ExtractFirstMethodLine(string trace)
    {
        var lines = trace.Split('\n');
        foreach (var line in lines)
        {
            if (line.Trim().StartsWith("at "))
                return line.Trim();
        }
        return lines.Length > 0 ? lines[0].Trim() : trace;
    }

    #endregion
}