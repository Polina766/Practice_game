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

    [Header("Цвета для кнопки Continue")]
    public Color normalColor = Color.white;                      // Когда есть сохранение
    public Color disabledColor = new Color(0.3f, 0.3f, 0.3f, 1f); // Тёмно-серая (НЕ прозрачная!)

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
            bool canContinue = false;

            if (GameManager.Instance != null)
            {
                canContinue = GameManager.Instance.CanContinue();
            }
            else
            {
                // Если GameManager ещё нет, проверяем только наличие сохранения
                canContinue = SaveSystem.HasSavedGame();
            }

            continueButton.interactable = canContinue;

            ColorBlock colors = continueButton.colors;
            if (canContinue)
            {
                colors.normalColor = normalColor;
                colors.highlightedColor = normalColor * 1.1f;
                colors.pressedColor = normalColor * 0.9f;
                colors.selectedColor = normalColor;
                colors.disabledColor = disabledColor;
            }
            else
            {
                colors.normalColor = disabledColor;
                colors.highlightedColor = disabledColor;
                colors.pressedColor = disabledColor;
                colors.selectedColor = disabledColor;
                colors.disabledColor = disabledColor;
            }
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