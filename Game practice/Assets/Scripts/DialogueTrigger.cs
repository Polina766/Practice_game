using UnityEngine;

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
    public bool playOnlyOnce = true;  // Ставим true по умолчанию

    [Tooltip("Автоматически уничтожить объект триггера после диалога?")]
    public bool destroyAfterDialogue = true;  // Уничтожит объект с NPC/триггером

    private bool hasBeenPlayed = false;  // Был ли диалог уже проигран

    void Start()
    {
        if (dialogueManager == null)
            dialogueManager = FindAnyObjectByType<DialogueManager>();
    }

    public void TriggerDialogue()
    {
        // Если диалог уже был проигран и нужно только один раз - выходим
        if (playOnlyOnce && hasBeenPlayed)
        {
            Debug.Log("Диалог уже был проигран и не повторится");
            return;
        }

        if (dialogueManager != null)
        {
            hasBeenPlayed = true;  // Отмечаем как проигранный
            dialogueManager.StartDialogue(speakerNames, dialogueLines);

            // После завершения диалога удаляем объект (если нужно)
            if (destroyAfterDialogue)
            {
                // Подписываемся на событие окончания диалога
                StartCoroutine(WaitAndDestroy());
            }
            else
            {
                // Просто отключаем триггер, но объект остаётся
                Collider2D col = GetComponent<Collider2D>();
                if (col != null) col.enabled = false;
            }
        }
    }

    private System.Collections.IEnumerator WaitAndDestroy()
    {
        // Ждём, пока диалог закончится
        while (dialogueManager.isActiveAndEnabled && dialogueManager.gameObject.activeSelf)
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
            if (!destroyAfterDialogue && playOnlyOnce)
            {
                GetComponent<Collider2D>().enabled = false;
            }
        }
    }
}