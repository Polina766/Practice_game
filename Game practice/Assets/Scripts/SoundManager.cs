using UnityEngine;
using UnityEngine.UI;

public class VolumeSettings : MonoBehaviour
{
    [Header("Слайдеры")]
    public Slider soundSlider;      // ползунок для звуков
    public Slider musicSlider;      // ползунок для музыки

    [Header("Картинки")]
    public Image soundKey;          // ключ для звуков
    public Image musicKey;          // ключ для музыки

    [Header("Спрайты для ЗВУКА (sound)")]
    public Sprite soundKeyBrown;    // коричневый ключ для звука
    public Sprite soundKeyPurple;   // фиолетовый ключ для звука

    [Header("Спрайты для МУЗЫКИ (music)")]
    public Sprite musicKeyBrown;    // коричневый ключ для музыки
    public Sprite musicKeyPurple;   // фиолетовый ключ для музыки

    void Start()
    {
        // Подписываемся на изменение слайдеров
        soundSlider.onValueChanged.AddListener(OnSoundVolumeChanged);
        musicSlider.onValueChanged.AddListener(OnMusicVolumeChanged);

        // Запускаем проверку при старте
        OnSoundVolumeChanged(soundSlider.value);
        OnMusicVolumeChanged(musicSlider.value);
    }

    void OnSoundVolumeChanged(float value)
    {
        // Меняем цвет ключа для ЗВУКОВ
        if (value > 0.01f)
            soundKey.sprite = soundKeyPurple;
        else
            soundKey.sprite = soundKeyBrown;

        
    }

    void OnMusicVolumeChanged(float value)
    {
        // Меняем цвет ключа для МУЗЫКИ
        if (value > 0.01f)
            musicKey.sprite = musicKeyPurple;
        else
            musicKey.sprite = musicKeyBrown;

        
    }
}