using UnityEngine;
using UnityEngine.UI;

public class VolumeSliders : MonoBehaviour
{
    public Slider musicSlider;
    public Slider sfxSlider;

    private void Start()
    {
        if (AudioManager.Instance == null)
        {
            Debug.LogError("AudioManager не найден!");
            return;
        }

        if (musicSlider != null)
        {
            musicSlider.value = AudioManager.Instance.GetMusicVolume();
            musicSlider.onValueChanged.AddListener(OnMusicChanged);
        }

        if (sfxSlider != null)
        {
            sfxSlider.value = AudioManager.Instance.GetSFXVolume();
            sfxSlider.onValueChanged.AddListener(OnSFXChanged);
        }
    }

    private void OnMusicChanged(float value)
    {
        AudioManager.Instance.SetMusicVolume(value);
    }

    private void OnSFXChanged(float value)
    {
        AudioManager.Instance.SetSFXVolume(value);
    }
}