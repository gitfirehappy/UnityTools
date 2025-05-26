using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;

public class AudioManager : SingletonMono<AudioManager>
{
    [Header("配置")]
    [SerializeField] private AudioMixer mixer;
    [SerializeField] private SceneBGMConfigSO bgmConfig;

    [Header("音源")]
    [SerializeField] private AudioSource bgmSource;
    [SerializeField] private AudioSource sfxSource;

    protected override void Init()
    {
        // 自动创建 AudioSource
        bgmSource = gameObject.AddComponent<AudioSource>();
        sfxSource = gameObject.AddComponent<AudioSource>();

        bgmSource.loop = true;
        bgmSource.playOnAwake = false;
        sfxSource.playOnAwake = false;

        // 自动绑定 AudioMixerGroup
        var musicGroup = mixer.FindMatchingGroups("Music");
        if (musicGroup.Length > 0)
            bgmSource.outputAudioMixerGroup = musicGroup[0];

        var sfxGroup = mixer.FindMatchingGroups("SFX");
        if (sfxGroup.Length > 0)
            sfxSource.outputAudioMixerGroup = sfxGroup[0];

        // 初始化音量控制（静态工具类）
        AudioVolumeService.Init(mixer);

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        var bgm = bgmConfig?.GetBGMForScene(scene.buildIndex);
        if (bgm != null)
        {
            bgmSource.clip = bgm;
            bgmSource.Play();
        }
        else
        {
            bgmSource.Stop();
        }
    }


    public void PlaySFX(AudioClip clip)
    {
        if (clip != null)
            sfxSource.PlayOneShot(clip);
    }
}
