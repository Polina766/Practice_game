using UnityEngine;
using UnityEngine.UI;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    // Текущие источники звука (будут меняться при загрузке сцены)
    private AudioSource currentMusicSource;
    private AudioSource currentSFXSource;

    // Хранимые значения громкости
    private float musicVolume = 0.75f;
    private float sfxVolume = 0.75f;

    // Ключи для сохранения
    private const string MUSIC_VOL_KEY = "MusicVolume";
    private const string SFX_VOL_KEY = "SFXVolume";

    // События для уведомления ползунков об изменении громкости
    public System.Action<float> OnMusicVolumeChanged;
    public System.Action<float> OnSFXVolumeChanged;

    private void Awake()
    {
        // Синглтон — объект живёт один между всеми сценами
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // Загружаем сохранённую громкость
        musicVolume = PlayerPrefs.GetFloat(MUSIC_VOL_KEY, 0.75f);
        sfxVolume = PlayerPrefs.GetFloat(SFX_VOL_KEY, 0.75f);
    }

    // ЭТОТ МЕТОД ВЫЗЫВАЕТСЯ В КАЖДОЙ СЦЕНЕ ДЛЯ ПОДКЛЮЧЕНИЯ МУЗЫКИ И ЗВУКОВ
    public void RegisterAudioSources(AudioSource musicSource, AudioSource sfxSource)
    {
        currentMusicSource = musicSource;
        currentSFXSource = sfxSource;

        // Применяем сохранённую громкость к новым источникам
        if (currentMusicSource != null)
            currentMusicSource.volume = musicVolume;

        if (currentSFXSource != null)
            currentSFXSource.volume = sfxVolume;
    }

    // Установка громкости музыки (вызывается из ползунков)
    public void SetMusicVolume(float volume)
    {
        musicVolume = Mathf.Clamp01(volume);
        PlayerPrefs.SetFloat(MUSIC_VOL_KEY, musicVolume);

        if (currentMusicSource != null)
            currentMusicSource.volume = musicVolume;

        OnMusicVolumeChanged?.Invoke(musicVolume);
    }

    // Установка громкости звуков
    public void SetSFXVolume(float volume)
    {
        sfxVolume = Mathf.Clamp01(volume);
        PlayerPrefs.SetFloat(SFX_VOL_KEY, sfxVolume);

        if (currentSFXSource != null)
            currentSFXSource.volume = sfxVolume;

        OnSFXVolumeChanged?.Invoke(sfxVolume);
    }

    // Получить текущую громкость (для инициализации ползунков)
    public float GetMusicVolume() => musicVolume;
    public float GetSFXVolume() => sfxVolume;

    // Проигрывание звука (можно вызывать из любой сцены)
    public void PlaySFX(AudioClip clip, float volumeScale = 1f)
    {
        if (clip != null && currentSFXSource != null)
        {
            currentSFXSource.PlayOneShot(clip, volumeScale);
        }
    }

    // Проигрывание звука в точке в мире
    public void PlaySFXAtPoint(AudioClip clip, Vector3 position, float volumeScale = 1f)
    {
        if (clip != null)
        {
            AudioSource.PlayClipAtPoint(clip, position, volumeScale * sfxVolume);
        }
    }
}