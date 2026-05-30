using UnityEngine;
using System.Collections;

public class DialogueTrigger : MonoBehaviour
{
    [Header("Диалог")]
    public string[] speakerNames;
    public string[] dialogueLines;

    [Header("Ссылки")]
    public DialogueManager dialogueManager;
    public QuestStepTrigger questTrigger;  // ← ДОБАВИТЬ! Перетащите сюда QuestStepTrigger

    [Header("Настройки повтора")]
    public bool playOnlyOnce = true;
    public bool destroyAfterDialogue = true;

    private bool hasBeenPlayed = false;

    void Start()
    {
        if (dialogueManager == null)
            dialogueManager = FindAnyObjectByType<DialogueManager>();

        // Если questTrigger не назначен - пробуем найти
        if (questTrigger == null)
            questTrigger = GetComponent<QuestStepTrigger>();
    }

    public void TriggerDialogue()
    {
        if (playOnlyOnce && hasBeenPlayed)
        {
            Debug.Log("Диалог уже был проигран и не повторится");
            return;
        }

        if (dialogueManager != null)
        {
            hasBeenPlayed = true;
            dialogueManager.StartDialogue(speakerNames, dialogueLines);

            // Запускаем корутину для ожидания окончания диалога
            StartCoroutine(WaitForDialogueEnd());
        }
    }

    // Корутина, которая ждёт окончания диалога и отправляет уведомление
    IEnumerator WaitForDialogueEnd()
    {
        // Ждём, пока диалог активен
        while (dialogueManager != null && dialogueManager.gameObject.activeSelf)
        {
            yield return null;
        }

        // Небольшая задержка после закрытия диалога
        yield return new WaitForSeconds(0.1f);

        // 🔥 ОТПРАВЛЯЕМ УВЕДОМЛЕНИЕ В GAMEMANAGER
        if (questTrigger != null)
        {
            questTrigger.NotifyManually();
            Debug.Log($"✅ Диалог завершён, уведомлён GameManager: {questTrigger.triggerID}");
        }
        else
        {
            Debug.LogWarning("⚠️ QuestStepTrigger не найден для отправки уведомления");
        }

        // Уничтожаем объект после диалога (если нужно)
        if (destroyAfterDialogue)
        {
            yield return new WaitForSeconds(0.1f);
            Destroy(gameObject);
        }
        else if (playOnlyOnce)
        {
            Collider2D col = GetComponent<Collider2D>();
            if (col != null) col.enabled = false;
        }
    }

    void OnMouseDown()
    {
        TriggerDialogue();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            TriggerDialogue();
        }
    }
}