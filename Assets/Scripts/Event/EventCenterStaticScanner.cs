using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

/// <summary>
/// 静态扫描项目中所有注册/移除事件的代码位置
/// </summary>
public class EventCenterStaticScanner : EditorWindow
{
    #region 内部结构

    private class ListenerInfo
    {
        public string eventName;
        public string methodLine;
        public int lineNumber;
        public string assetPath;
        public string type; // 注册 or 移除
    }

    #endregion

    #region 字段

    private List<ListenerInfo> listeners = new();
    private Vector2 scroll;

    #endregion

    #region 菜单入口

    [MenuItem("Tools/Event Center Static Scanner")]
    public static void Open()
    {
        GetWindow<EventCenterStaticScanner>("事件注册扫描器");
    }

    #endregion

    #region UI

    private void OnGUI()
    {
        if (GUILayout.Button("扫描所有事件注册/移除"))
        {
            ScanProject();
        }

        scroll = EditorGUILayout.BeginScrollView(scroll);

        foreach (var info in listeners)
        {
            EditorGUILayout.BeginHorizontal();

            GUI.color = info.type == "注册" ? Color.green : Color.red;

            if (GUILayout.Button($"[{info.type}] {info.eventName}", GUILayout.Width(250)))
            {
                var obj = AssetDatabase.LoadAssetAtPath<MonoScript>(info.assetPath);
                AssetDatabase.OpenAsset(obj, info.lineNumber);
            }

            GUI.color = Color.white;
            GUILayout.Label($"{Path.GetFileName(info.assetPath)}:{info.lineNumber}", GUILayout.Width(150));
            GUILayout.Label(info.methodLine);

            EditorGUILayout.EndHorizontal();
        }

        EditorGUILayout.EndScrollView();
    }

    #endregion

    #region 扫描逻辑

    private void ScanProject()
    {
        listeners.Clear();
        string[] guids = AssetDatabase.FindAssets("t:Script");

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            string[] lines = File.ReadAllLines(path);

            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i];

                if (line.Contains("EventCenter.AddListener"))
                {
                    listeners.Add(new ListenerInfo
                    {
                        eventName = ExtractEventName(line),
                        methodLine = line.Trim(),
                        lineNumber = i + 1,
                        assetPath = path,
                        type = "注册"
                    });
                }
                else if (line.Contains("EventCenter.RemoveListener"))
                {
                    listeners.Add(new ListenerInfo
                    {
                        eventName = ExtractEventName(line),
                        methodLine = line.Trim(),
                        lineNumber = i + 1,
                        assetPath = path,
                        type = "移除"
                    });
                }
            }
        }
    }

    private string ExtractEventName(string line)
    {
        int start = line.IndexOf('"');
        int end = line.LastIndexOf('"');
        if (start != -1 && end > start)
        {
            return line.Substring(start + 1, end - start - 1);
        }
        return "未知事件";
    }

    #endregion
}