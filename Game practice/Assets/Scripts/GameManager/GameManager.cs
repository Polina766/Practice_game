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
    private bool isPrologueActive = false;
    private bool isGameCompleted = false;

    [Header("Компоненты")]
    public GameObject playerPrefab;

    public string prologueSceneName = "Prologue";
    public string firstGameSceneName = "Hall";

    private bool isLoadingGame = false;

    [Header("Настройки мага")]
    public string currentMagScene = "";
    public GameObject currentMagObject;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void Start()
    {
        if (storySequence.Count == 0)
            SetupDefaultSequence();
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

    public void NewGame()
    {
        Debug.Log("🎮 Начинаем новую игру");
        SaveSystem.DeleteSave();
        currentStepIndex = 0;
        isLoadingGame = false;
        isPrologueActive = true;
        isGameCompleted = false;

        SceneManager.LoadScene(prologueSceneName);
    }

    public void ContinueGame()
    {
        if (!SaveSystem.HasSavedGame())
        {
            Debug.LogWarning("⚠️ Нет сохранённой игры!");
            return;
        }

        if (isGameCompleted)
        {
            Debug.LogWarning("⚠️ Игра уже пройдена! Начните новую игру.");
            return;
        }

        Debug.Log("📀 Загружаем сохранённую игру");
        GameData data = SaveSystem.LoadGame();
        if (data != null)
        {
            currentStepIndex = data.currentStepIndex;
            isLoadingGame = true;
            SceneManager.LoadScene(data.currentSceneName);
        }
    }

    public void MarkGameAsCompleted()
    {
        if (isGameCompleted) return;

        isGameCompleted = true;
        SaveSystem.DeleteSave();
        Debug.Log("🏆 Игра официально завершена! Сохранение удалено.");
    }

    public bool CanContinue()
    {
        if (isGameCompleted) return false;
        return SaveSystem.HasSavedGame();
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        currentSceneName = scene.name;
        Debug.Log($"📱 Загружена сцена: {scene.name}");

        if (isPrologueActive && scene.name == prologueSceneName)
        {
            Debug.Log("🎬 Сцена пролога, игровые системы отключены");
            return;
        }

        EnableAllDoors();
        Invoke(nameof(FixAllDoorColliders), 0.05f);

        if (currentStepIndex < storySequence.Count)
        {
            ShowMagInScene(scene.name);
        }
        else
        {
            HideMagInCurrentScene();
        }

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

        Invoke(nameof(EnableAllDoors), 0.1f);
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

    public void ReportTrigger(string triggerID)
    {
        if (currentStepIndex < storySequence.Count)
        {
            string expectedID = storySequence[currentStepIndex];

            if (expectedID == triggerID)
            {
                Debug.Log($"✅ Шаг {currentStepIndex + 1}/{storySequence.Count} выполнен: {triggerID}");
                currentStepIndex++;
                AutoSave();

                if (currentStepIndex < storySequence.Count)
                {
                    DisableAllTriggersExcept(storySequence[currentStepIndex]);
                }
                else
                {
                    Debug.Log("🎉 Все квесты пройдены! Ожидаем завершающий диалог...");
                    HideMagInCurrentScene();
                    // НЕ УДАЛЯЕМ СОХРАНЕНИЕ ЗДЕСЬ!
                    // Игра завершится только при появлении чёрного экрана
                }
            }
            else
            {
                Debug.Log($"⚠️ Сейчас ожидается '{expectedID}', а вы активировали '{triggerID}'");
            }
        }
    }

    public void AutoSave()
    {
        if (isGameCompleted) return;

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

    public void SaveAndGoToMenu()
    {
        if (isGameCompleted) return;

        AutoSave();
        SceneManager.LoadScene("Menu");
    }

    void RestoreTriggersState()
    {
        EnableAllDoors();

        QuestStepTrigger[] allTriggers = FindObjectsOfType<QuestStepTrigger>(true);
        foreach (var trigger in allTriggers)
        {
            if (trigger.CompareTag("Door"))
            {
                continue;
            }
            trigger.gameObject.SetActive(false);
        }

        if (currentStepIndex < storySequence.Count)
        {
            string nextTriggerID = storySequence[currentStepIndex];
            EnableTrigger(nextTriggerID);
        }
    }

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
            if (trigger.CompareTag("Door"))
            {
                trigger.gameObject.SetActive(true);
                Collider2D col = trigger.GetComponent<Collider2D>();
                if (col != null) col.enabled = true;
                continue;
            }

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

        EnableAllDoors();
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

    // ========== УПРАВЛЕНИЕ МАГОМ ==========

    public void ShowMagInScene(string sceneName)
    {
        HideMagInAllScenes();

        GameObject[] allObjects = Resources.FindObjectsOfTypeAll<GameObject>();
        GameObject mag = null;

        foreach (GameObject obj in allObjects)
        {
            if (obj.CompareTag("Mag") && obj.scene.name == sceneName)
            {
                mag = obj;
                break;
            }
        }

        if (mag == null)
        {
            mag = GameObject.Find("Mage");
        }

        if (mag != null)
        {
            mag.SetActive(true);
            currentMagObject = mag;
            currentMagScene = sceneName;
            Debug.Log($"✨ Маг '{mag.name}' появился в сцене: {sceneName}");
        }
        else
        {
            Debug.LogError($"❌ Маг с тегом 'Mag' не найден в сцене {sceneName}!");
        }
    }

    public void HideMagInCurrentScene()
    {
        if (currentMagObject != null)
        {
            currentMagObject.SetActive(false);
            Debug.Log($"😴 Маг скрыт в сцене: {currentMagScene}");
            currentMagObject = null;
        }
    }

    void HideMagInAllScenes()
    {
        GameObject[] allObjects = FindObjectsOfType<GameObject>(true);
        foreach (GameObject obj in allObjects)
        {
            if (obj.CompareTag("Mag"))
            {
                obj.SetActive(false);
            }
        }
    }

    void EnableAllDoors()
    {
        GameObject[] doors = Resources.FindObjectsOfTypeAll<GameObject>();
        List<GameObject> foundDoors = new List<GameObject>();

        foreach (GameObject obj in doors)
        {
            if (obj.CompareTag("Door") && obj.scene.name == currentSceneName)
            {
                foundDoors.Add(obj);
            }
        }

        Debug.Log($"🚪 Найдено дверей в сцене '{currentSceneName}': {foundDoors.Count}");

        foreach (GameObject door in foundDoors)
        {
            door.SetActive(true);

            Collider2D col = door.GetComponent<Collider2D>();
            if (col != null)
            {
                col.enabled = true;
            }

            Door doorScript = door.GetComponent<Door>();
            if (doorScript != null)
            {
                doorScript.enabled = true;
            }

            Debug.Log($"🚪 Дверь '{door.name}' ПРИНУДИТЕЛЬНО включена");
        }
    }

    public void PrologueFinished()
    {
        if (!isPrologueActive) return;

        Debug.Log("🎬 Пролог завершён, начинаем основную игру");
        isPrologueActive = false;

        currentStepIndex = 0;
        isLoadingGame = false;

        SceneManager.LoadScene(firstGameSceneName);
    }
}