using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 挂在 UI Slider 上，自动绑定对应音量类型
/// </summary>
[RequireComponent(typeof(Slider))]
public class VolumeSliderBinder : MonoBehaviour
{
    [SerializeField] private VolumeType volumeType = VolumeType.Master;
    private Slider slider;

    private void Awake()
    {
        slider = GetComponent<Slider>();
        slider.onValueChanged.AddListener(OnSliderChanged);
    }

    private void Start()
    {
        slider.SetValueWithoutNotify(AudioVolumeService.GetVolume(volumeType));
    }

    private void OnSliderChanged(float value)
    {
        AudioVolumeService.SetVolume(volumeType, value);
    }
}
