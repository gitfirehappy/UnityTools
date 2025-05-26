using UnityEngine;
using UnityEngine.Audio;

/// <summary>
/// 不依赖 UI，纯逻辑音量控制工具类
/// </summary>
public static class AudioVolumeService
{
    private const string MASTER_KEY = "MasterVolume";
    private const string MUSIC_KEY = "MusicVolume";
    private const string SFX_KEY = "SFXVolume";

    private static AudioMixer _mixer;

    public static void Init(AudioMixer mixer)
    {
        _mixer = mixer;
        ApplySavedVolumes();
    }

    public static void SetVolume(VolumeType type, float value01)
    {
        if (_mixer == null) return;

        string exposed = GetKey(type);
        float db = Mathf.Log10(Mathf.Clamp(value01, 0.0001f, 1f)) * 20f;
        _mixer.SetFloat(exposed, db);
        PlayerPrefs.SetFloat(exposed, value01);
    }

    public static float GetVolume(VolumeType type)
    {
        return PlayerPrefs.GetFloat(GetKey(type), 1f);
    }

    public static void ApplySavedVolumes()
    {
        foreach (VolumeType type in System.Enum.GetValues(typeof(VolumeType)))
        {
            SetVolume(type, GetVolume(type));
        }
    }

    private static string GetKey(VolumeType type) => type switch
    {
        VolumeType.Master => MASTER_KEY,
        VolumeType.Music => MUSIC_KEY,
        VolumeType.SFX => SFX_KEY,
        _ => MASTER_KEY
    };
}


public enum VolumeType
{
    Master, // 总音量
    Music,  // 可选
    SFX     // 可选
}
