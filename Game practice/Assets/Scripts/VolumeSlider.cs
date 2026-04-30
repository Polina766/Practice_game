using UnityEngine;
using UnityEngine.UI;

public class VolumeSliders : MonoBehaviour
{
    [SerializeField] private Slider musicSlider;
    [SerializeField] private Slider sfxSlider;

    private void Start()
    {
        if (AudioManager.Instance == null) return;

        // Устанавливаем значения ползунков из сохранённой громкости
        if (musicSlider != null)
        {
            musicSlider.value = AudioManager.Instance.GetMusicVolume();
            musicSlider.onValueChanged.AddListener(OnMusicSliderChanged);
        }

        if (sfxSlider != null)
        {
            sfxSlider.value = AudioManager.Instance.GetSFXVolume();
            sfxSlider.onValueChanged.AddListener(OnSFXSliderChanged);
        }

        // Подписываемся на обновления (если громкость изменилась из другого места)
        AudioManager.Instance.OnMusicVolumeChanged += UpdateMusicSlider;
        AudioManager.Instance.OnSFXVolumeChanged += UpdateSFXSlider;
    }

    private void OnDestroy()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.OnMusicVolumeChanged -= UpdateMusicSlider;
            AudioManager.Instance.OnSFXVolumeChanged -= UpdateSFXSlider;
        }
    }

    private void OnMusicSliderChanged(float value)
    {
        AudioManager.Instance.SetMusicVolume(value);
    }

    private void OnSFXSliderChanged(float value)
    {
        AudioManager.Instance.SetSFXVolume(value);
    }

    private void UpdateMusicSlider(float value)
    {
        if (musicSlider != null && musicSlider.value != value)
            musicSlider.value = value;
    }

    private void UpdateSFXSlider(float value)
    {
        if (sfxSlider != null && sfxSlider.value != value)
            sfxSlider.value = value;
    }
}