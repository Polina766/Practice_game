using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Настройки сюжета")]
    public List<string> storySequence = new List<string>();

    [Header("Состояние игры")]
    public int currentStepIndex = 0;
    public string currentSceneName = "";

    [Header("Компоненты")]
    public GameObject playerPrefab; // Префаб игрока (если нужно спавнить)

    private bool isLoadingGame = false;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Подписываемся на событие загрузки сцены
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void Start()
    {
        if (storySequence.Count == 0)
            SetupDefaultSequence();

        // Не запускаем инициализацию автоматически, ждём команды от UI
    }

    void SetupDefaultSequence()
    {
        storySequence = new List<string>()
        {
            "DialogueTrigger1",
            "DialogueTrigger2",
            "тригер для головоломки",
            "DialogueTrigger3",
            "DialogueTrigger4",
            "DialogueTrigger5",
            "PuzzleManager",
            "DialogueTrigger6",
            "DialogueTrigger7",
            "TriggerBook",
            "DialogueTrigger8",
            "DialogueTrigger9",
            "DialogueTrigger10",
            "ImageTrigger",
            "DialogueTrigger11"
        };
    }

    // ========== КНОПКИ UI ==========

    public void NewGame()
    {
        Debug.Log("🎮 Начинаем новую игру");

        // Удаляем старое сохранение
        SaveSystem.DeleteSave();

        // Сбрасываем прогресс
        currentStepIndex = 0;
        isLoadingGame = false;

        // Загружаем первую сцену (укажите название вашей первой сцены)
        SceneManager.LoadScene("Hall"); // Замените на имя вашей первой сцены
    }

    public void ContinueGame()
    {
        if (!SaveSystem.HasSavedGame())
        {
            Debug.LogWarning("⚠️ Нет сохранённой игры!");
            return;
        }

        Debug.Log("📀 Загружаем сохранённую игру");

        GameData data = SaveSystem.LoadGame();
        if (data != null)
        {
            currentStepIndex = data.currentStepIndex;
            isLoadingGame = true;

            // Загружаем сохранённую сцену
            SceneManager.LoadScene(data.currentSceneName);
        }
    }

    // Вызывается после загрузки сцены
    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        currentSceneName = scene.name;

        if (isLoadingGame)
        {
            // Восстанавливаем состояние триггеров после загрузки
            RestoreTriggersState();

            // Восстанавливаем позицию игрока (если сохраняли)
            RestorePlayerPosition();

            isLoadingGame = false;
        }
        else
        {
            // Новая игра - активируем только первый триггер
            DisableAllTriggersExcept(GetCurrentExpectedTrigger());
        }
    }

    // Вызывается триггерами
    public void ReportTrigger(string triggerID)
    {
        if (currentStepIndex < storySequence.Count)
        {
            string expectedID = storySequence[currentStepIndex];

            if (expectedID == triggerID)
            {
                Debug.Log($"✅ Шаг {currentStepIndex + 1}/{storySequence.Count} выполнен: {triggerID}");
                currentStepIndex++;

                // Автоматически сохраняем прогресс
                AutoSave();

                // Активируем следующий триггер
                if (currentStepIndex < storySequence.Count)
                {
                    DisableAllTriggersExcept(storySequence[currentStepIndex]);
                }
                else
                {
                    Debug.Log("🎉 Игра пройдена! Поздравляю!");
                }
            }
            else
            {
                Debug.Log($"⚠️ Сейчас ожидается '{expectedID}', а вы активировали '{triggerID}'");
            }
        }
    }

    // Автосохранение
    public void AutoSave()
    {
        GameData data = new GameData();
        data.currentStepIndex = currentStepIndex;
        data.currentSceneName = SceneManager.GetActiveScene().name;
        data.hasSavedGame = true;

        // Сохраняем позицию игрока (опционально)
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            data.playerPosX = player.transform.position.x;
            data.playerPosY = player.transform.position.y;
        }

        SaveSystem.SaveGame(data);
    }

    // Сохранение при выходе в меню (вызывать из кнопки "Выход в меню")
    public void SaveAndGoToMenu()
    {
        AutoSave();
        SceneManager.LoadScene("Menu"); // Название вашей сцены с меню
    }

    // Восстановление состояния триггеров
    void RestoreTriggersState()
    {
        // Отключаем ВСЕ триггеры
        QuestStepTrigger[] allTriggers = FindObjectsOfType<QuestStepTrigger>(true);
        foreach (var trigger in allTriggers)
        {
            trigger.gameObject.SetActive(false);
        }

        // Включаем триггер, который должен быть следующим
        if (currentStepIndex < storySequence.Count)
        {
            string nextTriggerID = storySequence[currentStepIndex];
            EnableTrigger(nextTriggerID);
        }

        // Также включаем ВСЕ уже пройденные триггеры, если они нужны для декораций?
        // (обычно их лучше оставить выключенными, чтобы диалоги не повторялись)
        for (int i = 0; i < currentStepIndex; i++)
        {
            DisableTrigger(storySequence[i]); // Убеждаемся, что старые выключены
        }
    }

    // Восстановление позиции игрока
    void RestorePlayerPosition()
    {
        GameData data = SaveSystem.LoadGame();
        if (data != null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null && (data.playerPosX != 0 || data.playerPosY != 0))
            {
                player.transform.position = new Vector3(data.playerPosX, data.playerPosY, 0);
                Debug.Log($"📍 Восстановлена позиция игрока: ({data.playerPosX}, {data.playerPosY})");
            }
        }
    }

    void DisableAllTriggersExcept(string activeTriggerID)
    {
        QuestStepTrigger[] allTriggers = FindObjectsOfType<QuestStepTrigger>(true);
        foreach (var trigger in allTriggers)
        {
            if (trigger.triggerID == activeTriggerID)
                trigger.gameObject.SetActive(true);
            else
                trigger.gameObject.SetActive(false);
        }
    }

    void EnableTrigger(string triggerID)
    {
        QuestStepTrigger[] allTriggers = FindObjectsOfType<QuestStepTrigger>(true);
        foreach (var trigger in allTriggers)
        {
            if (trigger.triggerID == triggerID)
            {
                trigger.gameObject.SetActive(true);
                break;
            }
        }
    }

    void DisableTrigger(string triggerID)
    {
        GameObject obj = GameObject.Find(triggerID);
        if (obj != null)
            obj.SetActive(false);
    }

    string GetCurrentExpectedTrigger()
    {
        if (currentStepIndex < storySequence.Count)
            return storySequence[currentStepIndex];
        return "GameComplete";
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
}