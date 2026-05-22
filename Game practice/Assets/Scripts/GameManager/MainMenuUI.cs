using UnityEngine;
using UnityEngine.UI;

public class MainMenuUI : MonoBehaviour
{
    [Header("Кнопки")]
    public Button newGameButton;
    public Button continueButton;
    public Button exitButton;

    [Header("Настройки")]
    public string firstSceneName = "Scene1"; // Название вашей первой сцены

    void Start()
    {
        // Настраиваем кнопки
        if (newGameButton != null)
            newGameButton.onClick.AddListener(StartNewGame);

        if (continueButton != null)
            continueButton.onClick.AddListener(ContinueGame);

        if (exitButton != null)
            exitButton.onClick.AddListener(ExitGame);

        // Обновляем состояние кнопки "Продолжить"
        UpdateContinueButtonState();
    }

    void StartNewGame()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.NewGame();
        }
        else
        {
            // Если GameManager ещё не создан (например, в меню нет GameManager)
            // Создаём его или просто загружаем сцену
            Debug.LogWarning("GameManager не найден, создаём новый...");
            GameObject gm = new GameObject("GameManager");
            gm.AddComponent<GameManager>();
            GameManager.Instance.NewGame();
        }
    }

    void ContinueGame()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.ContinueGame();
        }
        else
        {
            // Создаём GameManager и загружаем сохранение
            GameObject gm = new GameObject("GameManager");
            gm.AddComponent<GameManager>();
            GameManager.Instance.ContinueGame();
        }
    }

    void UpdateContinueButtonState()
    {
        if (continueButton != null)
        {
            // Кнопка "Продолжить" активна ТОЛЬКО если есть сохранение
            bool hasSave = SaveSystem.HasSavedGame();
            continueButton.interactable = hasSave;

            // Можно добавить визуальный эффект (полупрозрачность)
            ColorBlock colors = continueButton.colors;
            colors.disabledColor = new Color(0.5f, 0.5f, 0.5f, 0.5f);
            continueButton.colors = colors;
        }
    }

    void ExitGame()
    {
        Debug.Log("Выход из игры");
        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    // Обновляем состояние кнопки при каждом появлении меню
    void OnEnable()
    {
        UpdateContinueButtonState();
    }
}