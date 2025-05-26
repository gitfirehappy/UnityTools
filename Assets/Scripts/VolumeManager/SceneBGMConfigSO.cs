using UnityEngine;

[CreateAssetMenu(fileName = "SceneBGMConfigSO", menuName = "Audio/Scene BGM Config SO")]
public class SceneBGMConfigSO : ScriptableObject
{
    [System.Serializable]
    public struct SceneBGM
    {
        public int sceneBuildIndex; // 使用 Build Index
        public AudioClip bgmClip;
    }

    public SceneBGM[] sceneBGMs;

    public AudioClip GetBGMForScene(int buildIndex)
    {
        foreach (var bgm in sceneBGMs)
        {
            if (bgm.sceneBuildIndex == buildIndex)
                return bgm.bgmClip;
        }
        return null;
    }
}
