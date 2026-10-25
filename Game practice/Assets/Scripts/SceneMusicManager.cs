using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneMusicManager : MonoBehaviour
{
    [Header("Музыка для разных сцен")]
    public AudioClip mainMenuMusic;     // Музыка для главного меню
    public AudioClip gameplayMusic;     // Музыка для игровых сцен (Холл, Кухня)

    [Header("Список игровых сцен")]
    public string[] gameplayScenes = { "Hall", "Kitchen" };

    private string currentSceneName;
    private bool isPlayingGameplayMusic = false;

    void Start()
    {
        // Подписываемся на событие загрузки сцены
        SceneManager.sceneLoaded += OnSceneLoaded;

        // Загружаем музыку для текущей сцены
        LoadMusicForCurrentScene();
    }

    void OnDestroy()
    {
        // Отписываемся, чтобы избежать ошибок
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        LoadMusicForCurrentScene();
    }

    void LoadMusicForCurrentScene()
    {
        if (AudioManager.Instance == null) return;

        currentSceneName = SceneManager.GetActiveScene().name;

        // Проверяем, является ли сцена игровой (Hall, Kitchen)
        bool isGameplayScene = IsGameplayScene(currentSceneName);

        if (isGameplayScene)
        {
            // Игровая сцена
            if (!isPlayingGameplayMusic)
            {
                // Музыка ещё не играет - включаем
                if (gameplayMusic != null)
                {
                    AudioManager.Instance.PlayMusic(gameplayMusic);
                    isPlayingGameplayMusic = true;
                    
                }
            }
            else
            {
                // Музыка уже играет - НЕ перезапускаем!
                
            }
        }
        else
        {
            // Главное меню или другие сцены
            if (mainMenuMusic != null)
            {
                AudioManager.Instance.PlayMusic(mainMenuMusic);
                isPlayingGameplayMusic = false;
                
            }
        }
    }

    bool IsGameplayScene(string sceneName)
    {
        foreach (string scene in gameplayScenes)
        {
            if (scene == sceneName)
                return true;
        }
        return false;
    }
}
