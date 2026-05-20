using UnityEngine;
using System.Collections;

public class DialogueTrigger : MonoBehaviour
{
    [Header("Диалог")]
    [Tooltip("Имена персонажей в том же порядке, что и реплики")]
    public string[] speakerNames;

    [Tooltip("Текст реплик в том же порядке")]
    public string[] dialogueLines;

    public DialogueManager dialogueManager;

    [Header("Настройки повтора")]
    [Tooltip("Можно ли проиграть диалог только один раз?")]
    public bool playOnlyOnce = true;

    [Tooltip("Автоматически уничтожить объект триггера после диалога?")]
    public bool destroyAfterDialogue = true;

    [Header("GameManager Integration")]
    [Tooltip("Какой шаг сюжета нужен для активации этого диалога")]
    public GameManager.StoryStep requiredStep = GameManager.StoryStep.Dialogue1;

    [Tooltip("Автоматически отключать триггер если не подходит по шагу")]
    public bool disableIfWrongStep = true;

    private bool hasBeenPlayed = false;
    private Collider2D myCollider;
    private bool isAvailable = true;

    void Start()
    {
        if (dialogueManager == null)
            dialogueManager = FindAnyObjectByType<DialogueManager>();

        myCollider = GetComponent<Collider2D>();
        CheckAvailability();
    }

    public void CheckAvailability()
    {
        if (GameManager.Instance == null) return;

        bool shouldBeAvailable = (GameManager.Instance.GetCurrentStep() == requiredStep && !hasBeenPlayed);

        if (myCollider != null && disableIfWrongStep)
        {
            myCollider.enabled = shouldBeAvailable;
        }

        // Можно также скрывать визуальный индикатор
        if (GetComponent<SpriteRenderer>() != null)
            GetComponent<SpriteRenderer>().enabled = shouldBeAvailable;

        isAvailable = shouldBeAvailable;
    }

    public void TriggerDialogue()
    {
        // Проверка через GameManager
        if (GameManager.Instance != null && GameManager.Instance.GetCurrentStep() != requiredStep)
        {
            Debug.Log($"Диалог {gameObject.name} недоступен на текущем шаге {GameManager.Instance.GetCurrentStep()}");
            return;
        }

        // Если диалог уже был проигран и нужно только один раз - выходим
        if (playOnlyOnce && hasBeenPlayed)
        {
            Debug.Log("Диалог уже был проигран и не повторится");
            return;
        }

        if (dialogueManager != null)
        {
            hasBeenPlayed = true;
            dialogueManager.StartDialogue(speakerNames, dialogueLines);

            // Сообщаем Game Manager'у что диалог начался и ждём его завершения
            StartCoroutine(NotifyGameManagerWhenDialogueEnds());

            // После завершения диалога удаляем объект (если нужно)
            if (destroyAfterDialogue)
            {
                StartCoroutine(WaitAndDestroy());
            }
            else
            {
                // Просто отключаем триггер, но объект остаётся
                if (myCollider != null) myCollider.enabled = false;
            }
        }
    }

    private IEnumerator NotifyGameManagerWhenDialogueEnds()
    {
        // Ждём пока диалог активен
        while (dialogueManager != null && dialogueManager.isActiveAndEnabled && dialogueManager.gameObject.activeSelf)
        {
            yield return null;
        }

        // Небольшая задержка для плавности
        yield return new WaitForSeconds(0.1f);

        // Сообщаем GameManager'у что диалог завершён
        if (GameManager.Instance != null)
        {
            GameManager.Instance.CompleteDialogue(gameObject.name);
        }
    }

    private IEnumerator WaitAndDestroy()
    {
        // Ждём, пока диалог закончится
        while (dialogueManager != null && dialogueManager.isActiveAndEnabled && dialogueManager.gameObject.activeSelf)
        {
            yield return null;
        }

        // Немного задержки для красоты
        yield return new WaitForSeconds(0.1f);

        // Уничтожаем объект с триггером (NPC, зона диалога)
        Destroy(gameObject);
    }

    // Запуск диалога при нажатии на NPC мышкой
    void OnMouseDown()
    {
        TriggerDialogue();
    }

    // Запуск диалога при входе в триггер-зону
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            TriggerDialogue();

            // Отключаем коллайдер, чтобы диалог не запускался снова (если не уничтожаем объект)
            if (!destroyAfterDialogue && playOnlyOnce && myCollider != null)
            {
                myCollider.enabled = false;
            }
        }
    }
}