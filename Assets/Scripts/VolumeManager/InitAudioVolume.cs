using UnityEngine;
using UnityEngine.Audio;

/// <summary>
/// 挂在一个场景启动物体上，初始化 AudioMixer
/// </summary>
public class InitAudioVolume : MonoBehaviour
{
    [SerializeField] private AudioMixer mixer;

    private void Awake()
    {
        AudioVolumeService.Init(mixer);
    }
}
