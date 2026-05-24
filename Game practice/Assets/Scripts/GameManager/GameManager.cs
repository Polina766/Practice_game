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
            "DialogueTrigger11",
            "portal"
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

        // Загружаем первую сцену
        SceneManager.LoadScene("Hall");
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
        Debug.Log($"📱 Загружена сцена: {scene.name}");

        // ФИКС: перезапускаем коллайдеры на дверях
        Invoke(nameof(FixAllDoorColliders), 0.05f);

        if (isLoadingGame)
        {
            RestoreTriggersState();
            RestorePlayerPosition();
            isLoadingGame = false;
        }
        else
        {
            DisableAllTriggersExcept(GetCurrentExpectedTrigger());
        }
    }

    void FixAllDoorColliders()
    {
        GameObject[] doors = GameObject.FindGameObjectsWithTag("Door");
        foreach (GameObject door in doors)
        {
            Collider2D col = door.GetComponent<Collider2D>();
            if (col != null)
            {
                col.enabled = false;
                col.enabled = true;
                Debug.Log($"🔧 Починен коллайдер: {door.name}");
            }
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

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            data.playerPosX = player.transform.position.x;
            data.playerPosY = player.transform.position.y;
        }

        SaveSystem.SaveGame(data);
    }

    // Сохранение при выходе в меню
    public void SaveAndGoToMenu()
    {
        AutoSave();
        SceneManager.LoadScene("Menu");
    }

    // Восстановление состояния триггеров
    void RestoreTriggersState()
    {
        // Отключаем ВСЕ триггеры, КРОМЕ ДВЕРЕЙ
        QuestStepTrigger[] allTriggers = FindObjectsOfType<QuestStepTrigger>(true);
        foreach (var trigger in allTriggers)
        {
            // Пропускаем двери (не отключаем их)
            if (trigger.CompareTag("Door"))
            {
                Debug.Log($"🚪 Дверь '{trigger.triggerID}' пропущена (не отключаем)");
                continue;
            }
            trigger.gameObject.SetActive(false);
        }

        // Включаем следующий квестовый триггер
        if (currentStepIndex < storySequence.Count)
        {
            string nextTriggerID = storySequence[currentStepIndex];
            EnableTrigger(nextTriggerID);
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
        int enabledCount = 0;

        foreach (var trigger in allTriggers)
        {
            // ПРОВЕРКА НА ДВЕРЬ - не отключаем двери!
            if (trigger.CompareTag("Door"))
            {
                Debug.Log($"🚪 Дверь '{trigger.triggerID}' пропущена, остаётся активной");
                continue; // пропускаем, ничего не делаем с дверью
            }

            // Обычные квестовые триггеры
            if (trigger.triggerID == activeTriggerID)
            {
                trigger.gameObject.SetActive(true);
                enabledCount++;
                Debug.Log($"✅ Активирован квестовый триггер: {trigger.triggerID}");
            }
            else
            {
                trigger.gameObject.SetActive(false);
            }
        }

        if (enabledCount == 0 && activeTriggerID != "GameComplete")
        {
            Debug.LogWarning($"⚠️ Триггер с ID '{activeTriggerID}' не найден в этой сцене!");
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
                Debug.Log($"🔓 Включён квестовый триггер: {triggerID}");
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