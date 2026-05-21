using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    // Текущий шаг сюжета (0 - начало)
    private int currentStep = 0;

    // Шаги, которые надо выполнить по порядку
    // Ключ - номер шага, значение - ID триггера, который должен выполниться на этом шаге
    [System.Serializable]
    public class StepRequirement
    {
        public int stepIndex;
        public string requiredTriggerID; // например "DialogueTrigger1"
        public bool isComplete = false;
    }

    public List<StepRequirement> storySteps = new List<StepRequirement>();

    // Для сохранения прогресса между сценами
    private Dictionary<string, bool> completedTriggers = new Dictionary<string, bool>();

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        // Настраиваем шаги по вашему описанию (можно задать в инспекторе)
        if (storySteps.Count == 0)
            SetupDefaultSteps();
    }

    void SetupDefaultSteps()
    {
        // Пример вашей последовательности
        storySteps = new List<StepRequirement>()
        {
            new StepRequirement() { stepIndex = 0, requiredTriggerID = "DialogueTrigger1" },
            new StepRequirement() { stepIndex = 1, requiredTriggerID = "DialogueTrigger2" },
            new StepRequirement() { stepIndex = 2, requiredTriggerID = "DialogueTrigger3" },
            new StepRequirement() { stepIndex = 3, requiredTriggerID = "тригер для головоломки" }, // головоломка 1
            new StepRequirement() { stepIndex = 4, requiredTriggerID = "DialogueTrigger4" },
            new StepRequirement() { stepIndex = 5, requiredTriggerID = "DialogueTrigger5" },
            new StepRequirement() { stepIndex = 6, requiredTriggerID = "PuzzleManager" }, // головоломка 2
            new StepRequirement() { stepIndex = 7, requiredTriggerID = "DialogueTrigger6" },
            new StepRequirement() { stepIndex = 8, requiredTriggerID = "DialogueTrigger7" },
            new StepRequirement() { stepIndex = 9, requiredTriggerID = "TriggerBook" }, // головоломка 3
            new StepRequirement() { stepIndex = 10, requiredTriggerID = "DialogueTrigger8" },
            new StepRequirement() { stepIndex = 11, requiredTriggerID = "DialogueTrigger9" },
            new StepRequirement() { stepIndex = 12, requiredTriggerID = "DialogueTrigger10" },
            new StepRequirement() { stepIndex = 13, requiredTriggerID = "ImageTrigger" }, // головоломка 4
            new StepRequirement() { stepIndex = 14, requiredTriggerID = "DialogueTrigger11" }
        };
    }

    // Вызывается любым триггером, на который повешен QuestStepTrigger
    public void ReportTrigger(string triggerID)
    {
        // Проверяем, нужен ли этот триггер сейчас
        StepRequirement currentReq = GetCurrentStepRequirement();

        if (currentReq != null && currentReq.requiredTriggerID == triggerID && !currentReq.isComplete)
        {
            Debug.Log($"✅ GameManager: шаг {currentStep} выполнен триггером '{triggerID}'");
            currentReq.isComplete = true;
            currentStep++;

            // Доп. логика: отключаем старый триггер, чтобы не сработал снова (опционально)
            DisableTriggerGameObject(triggerID);
        }
        else
        {
            // Если триггер не по очереди - игнорируем (или можно вывести предупреждение)
            Debug.Log($"⚠️ Триггер '{triggerID}' проигнорирован, сейчас ожидается: " +
                      (currentReq != null ? currentReq.requiredTriggerID : "шаг завершен"));
        }
    }

    StepRequirement GetCurrentStepRequirement()
    {
        if (currentStep < storySteps.Count)
            return storySteps[currentStep];
        else
        {
            Debug.Log("🎉 Игра пройдена!");
            return null;
        }
    }

    // Опционально: отключаем GameObject самого триггера после выполнения, чтобы он не мешал
    void DisableTriggerGameObject(string triggerID)
    {
        GameObject obj = GameObject.Find(triggerID);
        if (obj != null)
            obj.SetActive(false);
    }

    // Для проверки в инспекторе
    public int GetCurrentStep()
    {
        return currentStep;
    }
}