using UnityEngine;
using UnityEngine.SceneManagement;
using System;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public enum StoryStep
    {
        Dialogue1,      // 0
        Dialogue2,      // 1
        Puzzle1,        // 2
        Dialogue3,      // 3
        Dialogue4,      // 4
        Puzzle2,        // 5
        Dialogue5,      // 6
        Dialogue6,      // 7
        Puzzle3,        // 8
        Dialogue7,      // 9
        Dialogue8,      // 10
        Dialogue9,      // 11
        Dialogue10,     // 12
        Puzzle4,        // 13
        Dialogue11,     // 14
        GameComplete    // 15
    }

    [SerializeField] private StoryStep currentStep = StoryStep.Dialogue1;

    public Action<StoryStep> OnStepChanged;

    private void Awake()
    {
        // Правильная реализация синглтона
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadProgress();
            Debug.Log($"GameManager создан. Текущий шаг: {currentStep}");
        }
        else if (Instance != this)
        {
            Debug.Log("Удаляем лишний GameManager");
            Destroy(gameObject);
            return;
        }
    }

    private void Start()
    {
        UpdateAllTriggers();
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log($"Загружена сцена: {scene.name}, текущий шаг: {currentStep}");
        // Даём время на инициализацию всех объектов в сцене
        Invoke(nameof(UpdateAllTriggers), 0.1f);
    }

    public StoryStep GetCurrentStep()
    {
        return currentStep;
    }

    public void SetStep(StoryStep newStep)
    {
        if (currentStep == newStep) return;

        currentStep = newStep;
        Debug.Log($"[GameManager] Сюжетный шаг изменён на: {currentStep}");

        OnStepChanged?.Invoke(currentStep);
        SaveProgress();
        UpdateAllTriggers();
    }

    public void CompleteDialogue(string dialogueName)
    {
        Debug.Log($"Диалог завершён: {dialogueName}, текущий шаг: {currentStep}");

        switch (currentStep)
        {
            case StoryStep.Dialogue1:
                SetStep(StoryStep.Dialogue2);
                break;
            case StoryStep.Dialogue2:
                SetStep(StoryStep.Puzzle1);
                break;
            case StoryStep.Dialogue3:
                SetStep(StoryStep.Dialogue4);
                break;
            case StoryStep.Dialogue4:
                SetStep(StoryStep.Puzzle2);
                break;
            case StoryStep.Dialogue5:
                SetStep(StoryStep.Dialogue6);
                break;
            case StoryStep.Dialogue6:
                SetStep(StoryStep.Puzzle3);
                break;
            case StoryStep.Dialogue7:
                SetStep(StoryStep.Dialogue8);
                break;
            case StoryStep.Dialogue8:
                SetStep(StoryStep.Dialogue9);
                break;
            case StoryStep.Dialogue9:
                SetStep(StoryStep.Dialogue10);
                break;
            case StoryStep.Dialogue10:
                SetStep(StoryStep.Puzzle4);
                break;
            case StoryStep.Dialogue11:
                SetStep(StoryStep.GameComplete);
                Debug.Log("🎉 Игра пройдена! 🎉");
                break;
            default:
                Debug.LogWarning($"Неизвестный шаг для диалога: {currentStep}");
                break;
        }
    }

    public void CompletePuzzle(string puzzleName)
    {
        Debug.Log($"Головоломка завершена: {puzzleName}, текущий шаг: {currentStep}");

        switch (currentStep)
        {
            case StoryStep.Puzzle1:
                SetStep(StoryStep.Dialogue3);
                break;
            case StoryStep.Puzzle2:
                SetStep(StoryStep.Dialogue5);
                break;
            case StoryStep.Puzzle3:
                SetStep(StoryStep.Dialogue7);
                break;
            case StoryStep.Puzzle4:
                SetStep(StoryStep.Dialogue11);
                break;
            default:
                Debug.LogWarning($"Головоломка {puzzleName} не соответствует текущему шагу {currentStep}");
                break;
        }
    }

    private void UpdateAllTriggers()
    {
        // Находим все триггеры в сцене
        DialogueTrigger[] dialogueTriggers = FindObjectsByType<DialogueTrigger>(FindObjectsSortMode.None);
        Debug.Log($"Найдено диалоговых триггеров: {dialogueTriggers.Length}");
        foreach (var trigger in dialogueTriggers)
        {
            if (trigger != null)
                trigger.CheckAvailability();
        }

        PuzzleTransition[] puzzleTransitions = FindObjectsByType<PuzzleTransition>(FindObjectsSortMode.None);
        foreach (var trigger in puzzleTransitions)
        {
            if (trigger != null)
                trigger.CheckAvailability();
        }

        PuzzleTrigger[] bookTriggers = FindObjectsByType<PuzzleTrigger>(FindObjectsSortMode.None);
        foreach (var trigger in bookTriggers)
        {
            if (trigger != null)
                trigger.CheckAvailability();
        }

        CauldronPuzzle[] cauldronPuzzles = FindObjectsByType<CauldronPuzzle>(FindObjectsSortMode.None);
        foreach (var puzzle in cauldronPuzzles)
        {
            if (puzzle != null)
                puzzle.CheckAvailability();
        }
    }

    private void SaveProgress()
    {
        PlayerPrefs.SetInt("GameProgress_Step", (int)currentStep);
        PlayerPrefs.Save();
        Debug.Log($"Прогресс сохранён: шаг {currentStep}");
    }

    private void LoadProgress()
    {
        if (PlayerPrefs.HasKey("GameProgress_Step"))
        {
            currentStep = (StoryStep)PlayerPrefs.GetInt("GameProgress_Step");
            Debug.Log($"Прогресс загружен: {currentStep}");
        }
        else
        {
            Debug.Log("Нет сохранённого прогресса, начинаем с начала");
        }
    }

    public void ResetGame()
    {
        currentStep = StoryStep.Dialogue1;
        PlayerPrefs.DeleteKey("GameProgress_Step");

        PlayerPrefs.DeleteKey("CardPuzzleCompleted");
        PlayerPrefs.DeleteKey("CauldronPuzzleCompleted");
        PlayerPrefs.DeleteKey("BooksPuzzleCompleted");
        PlayerPrefs.DeleteKey("ImagePuzzleCompleted");

        PlayerPrefs.Save();
        UpdateAllTriggers();
        Debug.Log("Игра сброшена до начала");
    }
}