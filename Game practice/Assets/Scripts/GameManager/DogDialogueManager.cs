using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using TMPro;

public class DogDialogueManager : MonoBehaviour
{
    public static bool isDogDialogueActive = false;

    [Header("UI Elements")]
    public GameObject dialoguePanel;
    public TextMeshProUGUI speakerText;
    public TextMeshProUGUI dialogueText;
    public GameObject optionsPanel;
    public Button[] optionButtons;
    public TextMeshProUGUI[] optionButtonTexts;
    public GameObject continuePrompt;

    [Header("Player")]
    public PlayerController playerController;
    public Animator playerAnimator;
    public QuestStepTrigger questTrigger;

    [Header("Typing Settings")]
    public float typingSpeed = 0.05f;

    [Header("Success Threshold")]
    public int successThreshold = 40;

    [System.Serializable]
    public class DialogueQuestion
    {
        public string[] speakerNames;
        public string[] dialogueLines;
        public string[] optionTexts;
        public bool[] correctOptions;
    }

    [Header("Questions")]
    public DialogueQuestion[] questions;

    private int correctAnswersCount = 0;
    private bool dialogueCompleted = false;

    // Все строки диалога
    private List<DialogueLine> allLines = new List<DialogueLine>();
    private int currentLineIndex = 0;
    private int currentQuestionIndex = 0;

    // Состояния
    private bool isTyping = false;
    private bool waitingForNext = false;
    private string currentFullText = "";
    private string currentSpeaker = "";

    // Флаги
    private bool isInQuestions = false;
    private bool isShowingOptions = false;
    private int selectedQuestionIndex = -1;

    private class DialogueLine
    {
        public string speaker;
        public string text;
        public bool isQuestion;
        public string[] optionTexts;
        public bool[] correctOptions;
    }

    void Start()
    {
        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);
        if (optionsPanel != null)
            optionsPanel.SetActive(false);
    }

    public void StartDialogue()
    {
        Debug.Log("🐕 Dog dialogue started");
        isDogDialogueActive = true;

        LockPlayer();

        dialoguePanel.SetActive(true);
        correctAnswersCount = 0;
        currentQuestionIndex = 0;
        currentLineIndex = 0;
        isInQuestions = false;
        isShowingOptions = false;

        BuildAllLines();
        ShowCurrentLine();
    }

    void BuildAllLines()
    {
        allLines.Clear();

        // Вступительные строки
        allLines.Add(new DialogueLine { speaker = "Mag", text = "Once, this was supposed to be a simple spell.", isQuestion = false });
        allLines.Add(new DialogueLine { speaker = "Mag", text = "To create an assistant. Someone who understands without explanation.", isQuestion = false });
        allLines.Add(new DialogueLine { speaker = "Girl", text = "So what went wrong?", isQuestion = false });
        allLines.Add(new DialogueLine { speaker = "Mag", text = "He understood everything…", isQuestion = false });
        allLines.Add(new DialogueLine { speaker = "Mag", text = "…but decided he had his own opinion.", isQuestion = false });
        allLines.Add(new DialogueLine { speaker = "Mag", text = "I tried to negotiate.", isQuestion = false });
        allLines.Add(new DialogueLine { speaker = "Mag", text = "To explain why he exists.", isQuestion = false });
        allLines.Add(new DialogueLine { speaker = "Mag", text = "Why he should obey.", isQuestion = false });
        allLines.Add(new DialogueLine { speaker = "Mag", text = "Turns out, sentient beings don't respond well to orders.", isQuestion = false });
        allLines.Add(new DialogueLine { speaker = "Girl", text = "And you locked him up?", isQuestion = false });
        allLines.Add(new DialogueLine { speaker = "Mag", text = "I called it a \"temporary solution.\"", isQuestion = false });
        allLines.Add(new DialogueLine { speaker = "Mag", text = "It's been going on for several years.", isQuestion = false });

        // Вопросы
        for (int i = 0; i < questions.Length; i++)
        {
            DialogueQuestion q = questions[i];

            for (int j = 0; j < q.speakerNames.Length; j++)
            {
                allLines.Add(new DialogueLine
                {
                    speaker = q.speakerNames[j],
                    text = q.dialogueLines[j],
                    isQuestion = false
                });
            }

            allLines.Add(new DialogueLine
            {
                speaker = "",
                text = "",
                isQuestion = true,
                optionTexts = q.optionTexts,
                correctOptions = q.correctOptions
            });
        }

        // Финальные строки (успех)
        allLines.Add(new DialogueLine { speaker = "Personality A", text = "That's an honest answer.", isQuestion = false });
        allLines.Add(new DialogueLine { speaker = "Personality B", text = "Honesty is a good start.", isQuestion = false });
        allLines.Add(new DialogueLine { speaker = "Personality A", text = "Maybe I was wrong.", isQuestion = false });
        allLines.Add(new DialogueLine { speaker = "Personality B", text = "Mistakes can be fixed.", isQuestion = false });
        allLines.Add(new DialogueLine { speaker = "Mag", text = "Alright. I… might have been too harsh.", isQuestion = false });
        allLines.Add(new DialogueLine { speaker = "Mag", text = "I don't like admitting mistakes.", isQuestion = false });
        allLines.Add(new DialogueLine { speaker = "Mag", text = "But they happen. Even to me.", isQuestion = false });
        allLines.Add(new DialogueLine { speaker = "Mag", text = "You did well. Not perfect. But better than me.", isQuestion = false });
        allLines.Add(new DialogueLine { speaker = "Girl", text = "He really eats flowers.", isQuestion = false });
        allLines.Add(new DialogueLine { speaker = "Mag", text = "Therapy. Cheap and eco-friendly.", isQuestion = false });
        allLines.Add(new DialogueLine { speaker = "Girl", text = "Are you… happy?", isQuestion = false });
        allLines.Add(new DialogueLine { speaker = "Mag", text = "Don't start. It's temporary.", isQuestion = false });
        allLines.Add(new DialogueLine { speaker = "Mag", text = "You deserve to become my apprentice.", isQuestion = false });
        allLines.Add(new DialogueLine { speaker = "Girl", text = "You doubted?", isQuestion = false });
        allLines.Add(new DialogueLine { speaker = "Mag", text = "I won't admit it.", isQuestion = false });
        allLines.Add(new DialogueLine { speaker = "Mag", text = "Let's go clean up the mess in the kitchen.", isQuestion = false });
        allLines.Add(new DialogueLine { speaker = "Girl", text = "Again…", isQuestion = false });
        allLines.Add(new DialogueLine { speaker = "Mag", text = "Don't think about slacking off.", isQuestion = false });
    }

    void ShowCurrentLine()
    {
        if (currentLineIndex >= allLines.Count)
        {
            FinishDogDialogue();
            return;
        }

        DialogueLine line = allLines[currentLineIndex];

        if (line.isQuestion)
        {
            ShowOptions(line.optionTexts, line.correctOptions);
            return;
        }

        currentSpeaker = line.speaker;
        currentFullText = line.text;
        speakerText.text = currentSpeaker;
        dialogueText.text = "";

        isTyping = true;
        waitingForNext = false;

        if (continuePrompt != null)
            continuePrompt.SetActive(false);

        StartCoroutine(TypeText());
    }

    IEnumerator TypeText()
    {
        dialogueText.text = "";

        foreach (char letter in currentFullText.ToCharArray())
        {
            dialogueText.text += letter;
            yield return new WaitForSeconds(typingSpeed);
        }

        isTyping = false;
        waitingForNext = true;

        if (continuePrompt != null)
            continuePrompt.SetActive(true);
    }

    void ShowOptions(string[] options, bool[] correctOptions)
    {
        isShowingOptions = true;

        speakerText.gameObject.SetActive(false);
        dialogueText.gameObject.SetActive(false);
        if (continuePrompt != null)
            continuePrompt.SetActive(false);

        optionsPanel.SetActive(true);

        for (int i = 0; i < optionButtons.Length; i++)
        {
            if (i < options.Length)
            {
                optionButtons[i].gameObject.SetActive(true);
                optionButtonTexts[i].text = options[i];
                int idx = i;
                optionButtons[i].onClick.RemoveAllListeners();
                optionButtons[i].onClick.AddListener(() => OnOptionSelected(idx, correctOptions[idx]));
            }
            else
            {
                optionButtons[i].gameObject.SetActive(false);
            }
        }
    }

    void OnOptionSelected(int optionIndex, bool isCorrect)
    {
        Debug.Log($"Option {optionIndex + 1}, correct: {isCorrect}");

        if (isCorrect)
            correctAnswersCount++;

        optionsPanel.SetActive(false);

        speakerText.gameObject.SetActive(true);
        dialogueText.gameObject.SetActive(true);

        isShowingOptions = false;

        currentLineIndex++;
        ShowCurrentLine();
    }

    void NextLine()
    {
        if (!waitingForNext) return;

        waitingForNext = false;
        currentLineIndex++;
        ShowCurrentLine();
    }

    void SkipTyping()
    {
        if (!isTyping) return;

        StopAllCoroutines();
        dialogueText.text = currentFullText;
        isTyping = false;
        waitingForNext = true;

        if (continuePrompt != null)
            continuePrompt.SetActive(true);
    }

    void LockPlayer()
    {
        if (playerController != null)
        {
            playerController.CancelMoveIndicator();
            playerController.StopAllMovement();
            playerController.enabled = false;
        }
        if (playerAnimator != null)
        {
            playerAnimator.SetFloat("Speed", 0f);
        }
    }

    void UnlockPlayer()
    {
        if (playerController != null)
        {
            playerController.enabled = true;
        }
    }

    void Update()
    {
        if (!isDogDialogueActive) return;
        if (isShowingOptions) return;

        if (Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(1))
        {
            if (isTyping)
            {
                SkipTyping();
            }
            else if (waitingForNext)
            {
                NextLine();
            }
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && !dialogueCompleted && !isDogDialogueActive)
        {
            StartDialogue();
            dialogueCompleted = true;
            GetComponent<Collider2D>().enabled = false;
        }
    }

    // 🔥 ЕДИНСТВЕННЫЙ МЕТОД ЗАВЕРШЕНИЯ ДИАЛОГА
    void FinishDogDialogue()
    {
        isDogDialogueActive = false;
        dialoguePanel.SetActive(false);
        optionsPanel.SetActive(false);
        speakerText.gameObject.SetActive(true);
        dialogueText.gameObject.SetActive(true);
        UnlockPlayer();

        if (questTrigger != null)
        {
            questTrigger.NotifyManually();
        }

        GameEnding gameEnding = FindObjectOfType<GameEnding>();
        if (gameEnding != null)
        {
            gameEnding.StartEnding();
        }
        else
        {
            Debug.LogWarning("GameEnding не найден на сцене!");
        }

        Debug.Log("🐕 Dog dialogue finished");
    }
}