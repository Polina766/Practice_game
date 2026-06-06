using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using TMPro;

public class DogDialogueManager : MonoBehaviour
{
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
    public int successThreshold = 40; // Need 40% correct answers

    // Structure for each question
    [System.Serializable]
    public class DialogueQuestion
    {
        public string[] speakerNames;      // Who says each line (Personality A, Personality B, Player, Mag)
        public string[] dialogueLines;     // Dialogue text lines
        public string[] optionTexts;       // Answer options
        public bool[] correctOptions;      // Which answer is correct (true/false)
    }

    [Header("Questions")]
    public DialogueQuestion[] questions;

    private int currentQuestionIndex = 0;
    private int currentLineIndex = 0;
    private int correctAnswersCount = 0;
    private bool isInQuestion = false;
    private bool isTyping = false;
    private bool cancelTyping = false;
    private bool dialogueCompleted = false;

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

        // Lock player
        LockPlayer();

        dialogueCompleted = false;
        currentQuestionIndex = 0;
        correctAnswersCount = 0;

        dialoguePanel.SetActive(true);
        StartCoroutine(PlayDialogue());
    }

    IEnumerator PlayDialogue()
    {
        // Introduction lines (Mag and Player)
        yield return StartCoroutine(ShowDialogueLine("Mag", "— Once, this was supposed to be a simple spell."));
        yield return StartCoroutine(ShowDialogueLine("Mag", "— To create an assistant. Someone who understands without explanation."));
        yield return StartCoroutine(ShowDialogueLine("Player", "— So what went wrong?"));
        yield return StartCoroutine(ShowDialogueLine("Mag", "— He understood everything…"));
        yield return StartCoroutine(ShowDialogueLine("Mag", "— …but decided he had his own opinion."));
        yield return StartCoroutine(ShowDialogueLine("Mag", "— I tried to negotiate."));
        yield return StartCoroutine(ShowDialogueLine("Mag", "— To explain why he exists."));
        yield return StartCoroutine(ShowDialogueLine("Mag", "— Why he should obey."));
        yield return StartCoroutine(ShowDialogueLine("Mag", "— Turns out, sentient beings don't respond well to orders.", true));
        yield return StartCoroutine(ShowDialogueLine("Player", "— And you locked him up?"));
        yield return StartCoroutine(ShowDialogueLine("Mag", "— I called it a \"temporary solution.\""));
        yield return StartCoroutine(ShowDialogueLine("Mag", "— It's been going on for several years."));

        // Go through all questions
        for (int i = 0; i < questions.Length; i++)
        {
            yield return StartCoroutine(PlayQuestion(i));
        }

        // Check result
        int correctPercent = (correctAnswersCount * 100) / questions.Length;
        Debug.Log($"🐕 Result: {correctAnswersCount}/{questions.Length} correct answers ({correctPercent}%)");

        if (correctPercent >= successThreshold)
        {
            // SUCCESS
            yield return StartCoroutine(ShowDialogueLine("Personality A", " That's an honest answer."));
            yield return StartCoroutine(ShowDialogueLine("Personality B", " Honesty is a good start."));
            yield return StartCoroutine(ShowDialogueLine("Personality A", " Maybe I was wrong."));
            yield return StartCoroutine(ShowDialogueLine("Personality B", " Mistakes can be fixed."));
            yield return StartCoroutine(ShowDialogueLine("Mag", " Alright. I… might have been too harsh."));
            yield return StartCoroutine(ShowDialogueLine("Mag", " I don't like admitting mistakes.", true));
            yield return StartCoroutine(ShowDialogueLine("Mag", " But they happen. Even to me."));
            yield return StartCoroutine(ShowDialogueLine("Mag", " You did well. Not perfect. But better than me."));
            yield return StartCoroutine(ShowDialogueLine("Player", " He really eats flowers."));
            yield return StartCoroutine(ShowDialogueLine("Mag", " Therapy. Cheap and eco-friendly."));
            yield return StartCoroutine(ShowDialogueLine("Player", " Are you… happy?"));
            yield return StartCoroutine(ShowDialogueLine("Mag", " Don't start. It's temporary.", true));
            yield return StartCoroutine(ShowDialogueLine("Mag", " You did well. Not perfect. But you did well. You deserve to become my apprentice."));
            yield return StartCoroutine(ShowDialogueLine("Player", " You doubted?"));
            yield return StartCoroutine(ShowDialogueLine("Mag", " I won't admit it."));
            yield return StartCoroutine(ShowDialogueLine("Mag", " Let's go clean up the mess in the kitchen."));
            yield return StartCoroutine(ShowDialogueLine("Player", " Again…"));
            yield return StartCoroutine(ShowDialogueLine("Mag", " Don't think about slacking off. The mess isn't going anywhere."));

            // Success - notify GameManager
            if (questTrigger != null)
            {
                questTrigger.NotifyManually();
            }
        }
        else
        {
            // FAIL - restart
            yield return StartCoroutine(ShowDialogueLine("Personality A", " No."));
            yield return StartCoroutine(ShowDialogueLine("Personality B", " I don't believe you."));
            yield return StartCoroutine(ShowDialogueLine("Player", " Alright, let's try again."));
            yield return StartCoroutine(ShowDialogueLine("Mag", " I told you they're stubborn."));

            // Don't notify GameManager, game continues
        }

        EndDialogue();
    }

    IEnumerator PlayQuestion(int questionIndex)
    {
        DialogueQuestion q = questions[questionIndex];
        isInQuestion = true;

        // Show dialogue lines before the question
        for (int i = 0; i < q.speakerNames.Length; i++)
        {
            yield return StartCoroutine(ShowDialogueLine(q.speakerNames[i], q.dialogueLines[i]));
        }

        // Show answer options
        yield return StartCoroutine(ShowOptions(q.optionTexts, q.correctOptions));

        isInQuestion = false;
    }

    IEnumerator ShowDialogueLine(string speaker, string line, bool isQuiet = false)
    {
        speakerText.text = speaker;
        dialogueText.text = "";

        isTyping = true;
        cancelTyping = false;

        foreach (char letter in line.ToCharArray())
        {
            if (cancelTyping)
            {
                dialogueText.text = line;
                break;
            }
            dialogueText.text += letter;
            float speed = isQuiet ? typingSpeed * 1.5f : typingSpeed;
            yield return new WaitForSeconds(speed);
        }

        isTyping = false;

        // Show continue prompt
        if (continuePrompt != null)
            continuePrompt.SetActive(true);

        // Wait for input
        while (!Input.GetKeyDown(KeyCode.Space) && !Input.GetMouseButtonDown(1))
        {
            yield return null;
        }

        if (continuePrompt != null)
            continuePrompt.SetActive(false);

        cancelTyping = true;
    }

    IEnumerator ShowOptions(string[] options, bool[] correctOptions)
    {
        optionsPanel.SetActive(true);

        // Setup buttons
        for (int i = 0; i < optionButtons.Length; i++)
        {
            if (i < options.Length)
            {
                optionButtons[i].gameObject.SetActive(true);
                optionButtonTexts[i].text = options[i];
                int index = i; // Closure
                optionButtons[i].onClick.RemoveAllListeners();
                optionButtons[i].onClick.AddListener(() => OnOptionSelected(index, correctOptions[index]));
            }
            else
            {
                optionButtons[i].gameObject.SetActive(false);
            }
        }

        // Wait for selection
        bool selected = false;
        while (!selected)
        {
            yield return null;
        }

        optionsPanel.SetActive(false);
    }

    void OnOptionSelected(int optionIndex, bool isCorrect)
    {
        Debug.Log($"📝 Selected option {optionIndex + 1}, correct: {isCorrect}");

        if (isCorrect)
        {
            correctAnswersCount++;
            Debug.Log($"✅ Correct! Total correct: {correctAnswersCount}");
        }

        // Show dog's reaction
        StartCoroutine(ShowReaction(isCorrect));
    }

    IEnumerator ShowReaction(bool isCorrect)
    {
        if (isCorrect)
        {
            yield return StartCoroutine(ShowDialogueLine("Personality A", " Hmm... that sounds reasonable.", true));
            yield return StartCoroutine(ShowDialogueLine("Personality B", " Alright, one point for you.", true));
        }
        else
        {
            yield return StartCoroutine(ShowDialogueLine("Personality A", " No! That's not right!", true));
            yield return StartCoroutine(ShowDialogueLine("Personality B", " You don't understand us.", true));
        }
    }

    void EndDialogue()
    {
        dialoguePanel.SetActive(false);
        optionsPanel.SetActive(false);

        // Unlock player
        UnlockPlayer();

        Debug.Log("🐕 Dog dialogue finished");
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
            playerAnimator.Play("Idle");
        }
    }

    void UnlockPlayer()
    {
        if (playerController != null)
        {
            playerController.enabled = true;
        }
    }

    // Start dialogue from trigger
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && !dialogueCompleted)
        {
            StartDialogue();
            dialogueCompleted = true;

            // Disable collider after activation
            GetComponent<Collider2D>().enabled = false;
        }
    }
}
