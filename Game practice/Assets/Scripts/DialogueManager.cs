using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using TMPro;  // если используешь TextMeshPro, иначе убери .TMP

public class DialogueManager : MonoBehaviour
{
    [Header("UI элементы")]
    public TextMeshProUGUI nameText;    // имя персонажа
    public TextMeshProUGUI dialogueText; // текст диалога
    public GameObject continuePrompt;   // иконка "нажмите чтобы продолжить"

    [Header("Настройки печати")]
    public float typingSpeed = 0.05f;   // задержка между буквами

    private Queue<string> sentences;     // очередь всех строк диалога
    private Queue<string> speakers;      // очередь имён для каждой строки

    private string currentSentence = "";
    private bool isTyping = false;       // идёт ли печать прямо сейчас
    private bool cancelTyping = false;   // нужно ли завершить печать досрочно

    void Start()
    {
        sentences = new Queue<string>();
        speakers = new Queue<string>();

        // На старте диалоговое окно скрыто
        gameObject.SetActive(false);
    }

    // Запуск диалога: передаём массивы (строки и кто их говорит)
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

        DisplayNextSentence();
    }

    // Переход к следующей строке (вызывается по пробелу/ПКМ)
    public void DisplayNextSentence()
    {
        // 1. Если текст ещё печатается — дорисовываем его мгновенно
        if (isTyping)
        {
            cancelTyping = true;
            return;
        }

        // 2. Если есть следующая строка — начинаем её печатать
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
            // 3. Диалог закончен — закрываем окно
            EndDialogue();
        }
    }

    IEnumerator TypeSentence(string sentence)
    {
        isTyping = true;
        cancelTyping = false;
        currentSentence = sentence;
        dialogueText.text = "";

        // Печатаем по буквам
        foreach (char letter in sentence.ToCharArray())
        {
            if (cancelTyping)
            {
                dialogueText.text = sentence; // выводим всю строку сразу
                break;
            }
            dialogueText.text += letter;
            yield return new WaitForSeconds(typingSpeed);
        }

        isTyping = false;
        // Показываем подсказку "нажмите для продолжения"
        if (continuePrompt != null)
            continuePrompt.SetActive(true);
    }

    void EndDialogue()
    {
        gameObject.SetActive(false);
        // Здесь можно разблокировать движение игрока и т.д.
    }

    void Update()
    {
        // Проверка нажатия пробел или правой кнопки мыши
        if (gameObject.activeSelf && (Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(1)))
        {
            // Перед тем как перейти к следующей строке — прячем подсказку
            if (!isTyping && continuePrompt != null)
                continuePrompt.SetActive(false);

            DisplayNextSentence();
        }
    }
}