using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("Audio Sources")]
    public AudioSource musicSource;
    public AudioSource sfxSource;

    private float musicVolume = 0.75f;
    private float sfxVolume = 0.75f;

    private const string MUSIC_KEY = "MusicVolume";
    private const string SFX_KEY = "SFXVolume";

    

    private void Awake()
    {
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

        musicVolume = PlayerPrefs.GetFloat(MUSIC_KEY, 0.75f);
        sfxVolume = PlayerPrefs.GetFloat(SFX_KEY, 0.75f);
    }

    private void Start()
    {
        if (musicSource != null) musicSource.volume = musicVolume;
        if (sfxSource != null) sfxSource.volume = sfxVolume;
    }

    public void SetMusicVolume(float volume)
    {
        musicVolume = volume;
        PlayerPrefs.SetFloat(MUSIC_KEY, musicVolume);
        if (musicSource != null) musicSource.volume = musicVolume;
    }

    public void SetSFXVolume(float volume)
    {
        sfxVolume = volume;
        PlayerPrefs.SetFloat(SFX_KEY, sfxVolume);
        if (sfxSource != null) sfxSource.volume = sfxVolume;
    }

    public float GetMusicVolume() => musicVolume;
    public float GetSFXVolume() => sfxVolume;

    // НОВЫЙ МЕТОД для управления музыкой
    public void PlayMusic(AudioClip clip, bool restartIfSame = false)
    {
        if (clip == null || musicSource == null) return;

        // Если эта же музыка уже играет - ничего не делаем (не перезапускаем)
        if (musicSource.clip == clip && musicSource.isPlaying && !restartIfSame)
        {
            Debug.Log($"Музыка '{clip.name}' уже играет, не перезапускаем");
            return;
        }

        // Меняем музыку
        musicSource.clip = clip;
        musicSource.loop = true;
        musicSource.Play();
        Debug.Log($"Запущена музыка: {clip.name}");
    }

    public void PlaySFX(AudioClip clip, float volumeScale = 1f)
    {
        if (clip != null && sfxSource != null)
        {
            sfxSource.PlayOneShot(clip, volumeScale);
        }
    }

    public float GetMusicTime() => musicSource.time;
    public void SetMusicTime(float time) => musicSource.time = time;


    private float savedMusicTime = 0f;

    public void PauseMusic()
    {
        if (musicSource.isPlaying)
        {
            savedMusicTime = musicSource.time;
            musicSource.Pause();
        }
    }

    public void ResumeMusic()
    {
        musicSource.time = savedMusicTime;
        musicSource.Play();
    }

    public void PlayMusicWithSave(AudioClip clip)
    {
        if (musicSource.clip == clip && musicSource.isPlaying)
            return;

        musicSource.clip = clip;
        musicSource.time = savedMusicTime;
        musicSource.Play();
    }
}