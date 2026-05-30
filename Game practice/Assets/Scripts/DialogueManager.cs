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
    public PlayerController playerController;  // Перетащите сюда PlayerController
    public Animator playerAnimator;            // Перетащите сюда Animator игрока

    private Queue<string> sentences;
    private Queue<string> speakers;
    private string currentSentence = "";
    private bool isTyping = false;
    private bool cancelTyping = false;

    void Start()
    {
        sentences = new Queue<string>();
        speakers = new Queue<string>();
        gameObject.SetActive(false);
    }

    public void StartDialogue(string[] speakerNames, string[] dialogueLines)
    {
        if (speakerNames.Length != dialogueLines.Length)
        {
            Debug.LogError("Количество имён и строк не совпадает!");
            return;
        }

        gameObject.SetActive(true);
        continuePrompt.SetActive(false);

        sentences.Clear();
        speakers.Clear();

        for (int i = 0; i < dialogueLines.Length; i++)
        {
            speakers.Enqueue(speakerNames[i]);
            sentences.Enqueue(dialogueLines[i]);
        }

        // 🔒 ПОЛНАЯ ОСТАНОВКА ИГРОКА
        LockPlayerCompletely();

        DisplayNextSentence();
    }

    public void DisplayNextSentence()
    {
        if (isTyping)
        {
            cancelTyping = true;
            return;
        }

        if (sentences.Count > 0)
        {
            string nextSpeaker = speakers.Dequeue();
            string nextSentence = sentences.Dequeue();

            nameText.text = nextSpeaker;
            StopAllCoroutines();
            StartCoroutine(TypeSentence(nextSentence));
        }
        else
        {
            EndDialogue();
        }
    }

    IEnumerator TypeSentence(string sentence)
    {
        isTyping = true;
        cancelTyping = false;
        currentSentence = sentence;
        dialogueText.text = "";

        foreach (char letter in sentence.ToCharArray())
        {
            if (cancelTyping)
            {
                dialogueText.text = sentence;
                break;
            }
            dialogueText.text += letter;
            yield return new WaitForSeconds(typingSpeed);
        }

        isTyping = false;
        if (continuePrompt != null)
            continuePrompt.SetActive(true);
    }

    void EndDialogue()
    {
        gameObject.SetActive(false);

        // 🔓 ВОЗВРАЩАЕМ УПРАВЛЕНИЕ ИГРОКУ
        UnlockPlayerCompletely();
    }

    void LockPlayerCompletely()
    {
        if (playerController != null)
        {
            // 1. Убираем стрелочку
            playerController.CancelMoveIndicator();

            // 2. Полностью останавливаем движение
            playerController.StopAllMovement();

            // 3. Отключаем возможность двигаться
            playerController.enabled = false;

            Debug.Log("🔒 Игрок ПОЛНОСТЬЮ заблокирован (движение, стрелочка, управление)");
        }

        // Ставим анимацию Idle
        if (playerAnimator != null)
        {
            playerAnimator.SetFloat("Speed", 0f);
            playerAnimator.Play("Idle");
        }
    }

    void UnlockPlayerCompletely()
    {
        if (playerController != null)
        {
            // Включаем управление обратно
            playerController.enabled = true;

            Debug.Log("🔓 Игрок разблокирован (управление восстановлено)");
        }
    }

    void Update()
    {
        if (gameObject.activeSelf && (Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(1)))
        {
            if (!isTyping && continuePrompt != null)
                continuePrompt.SetActive(false);

            DisplayNextSentence();
        }
    }
}