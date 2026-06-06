using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using TMPro;

public class DialogueManager : MonoBehaviour
{
    [Header("UI элементы")]
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI dialogueText;
    public GameObject continuePrompt;

    [Header("Настройки печати")]
    public float typingSpeed = 0.05f;

    [Header("Игрок")]
    public PlayerController playerController;
    public Animator playerAnimator;

    private Queue<string> sentences;
    private Queue<string> speakers;
    private bool isDialogueActive = false;
    private bool isTyping = false;
    private bool waitingForNext = false;
    private string fullText = "";

    void Start()
    {
        sentences = new Queue<string>();
        speakers = new Queue<string>();
        gameObject.SetActive(false);
    }

    public void StartDialogue(string[] speakerNames, string[] dialogueLines)
    {
        if (DogDialogueManager.isDogDialogueActive) return;

        if (speakerNames.Length != dialogueLines.Length)
        {
            Debug.LogError("Количество имён и строк не совпадает!");
            return;
        }

        isDialogueActive = true;
        waitingForNext = false;
        gameObject.SetActive(true);

        if (continuePrompt != null)
            continuePrompt.SetActive(false);

        sentences.Clear();
        speakers.Clear();

        for (int i = 0; i < dialogueLines.Length; i++)
        {
            speakers.Enqueue(speakerNames[i]);
            sentences.Enqueue(dialogueLines[i]);
        }

        LockPlayer();
        ShowCurrentLine();
    }

    void ShowCurrentLine()
    {
        if (sentences.Count > 0)
        {
            nameText.text = speakers.Dequeue();
            fullText = sentences.Dequeue();
            StartCoroutine(TypeText());
        }
        else
        {
            EndDialogue();
        }
    }

    IEnumerator TypeText()
    {
        isTyping = true;
        waitingForNext = false;
        dialogueText.text = "";

        if (continuePrompt != null)
            continuePrompt.SetActive(false);

        foreach (char letter in fullText.ToCharArray())
        {
            dialogueText.text += letter;
            yield return new WaitForSeconds(typingSpeed);
        }

        isTyping = false;
        waitingForNext = true;

        if (continuePrompt != null)
            continuePrompt.SetActive(true);
    }

    void EndDialogue()
    {
        isDialogueActive = false;
        waitingForNext = false;
        gameObject.SetActive(false);
        UnlockPlayer();
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
        if (DogDialogueManager.isDogDialogueActive) return;
        if (!isDialogueActive) return;

        // Проверка нажатия пробел или правой кнопки мыши
        if (Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(1))
        {
            // Если текст печатается - показываем весь сразу
            if (isTyping)
            {
                StopAllCoroutines();
                dialogueText.text = fullText;
                isTyping = false;
                waitingForNext = true;
                if (continuePrompt != null)
                    continuePrompt.SetActive(true);
            }
            // Если текст напечатан - переходим к следующей строке
            else if (waitingForNext)
            {
                waitingForNext = false;
                if (continuePrompt != null)
                    continuePrompt.SetActive(false);
                ShowCurrentLine();
            }
        }
    }
}