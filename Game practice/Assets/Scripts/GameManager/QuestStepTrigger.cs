using UnityEngine;

public class QuestStepTrigger : MonoBehaviour
{
    [Tooltip("Уникальный ID этого триггера, совпадает с requiredTriggerID в GameManager")]
    public string triggerID;

    // Опционально: задержка перед уведомлением (если нужно, чтобы диалог успел начаться)
    public float delayBeforeReport = 0f;

    private bool alreadyReported = false;

    void Start()
    {
        // Автоматически задаем ID из имени объекта, если не заполнено
        if (string.IsNullOrEmpty(triggerID))
            triggerID = gameObject.name;
    }

    // Этот метод вызывается из ваших существующих скриптов или из OnTriggerEnter
    public void NotifyQuestCompleted()
    {
        if (alreadyReported) return;

        if (GameManager.Instance != null)
        {
            if (delayBeforeReport > 0)
                Invoke(nameof(SendReport), delayBeforeReport);
            else
                SendReport();
        }
        else
        {
            Debug.LogWarning("GameManager не найден!");
        }
    }

    void SendReport()
    {
        GameManager.Instance.ReportTrigger(triggerID);
        alreadyReported = true;
    }

    // Стандартный вход в триггер (если у вас старый триггер на Collider2D)
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            NotifyQuestCompleted();
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            NotifyQuestCompleted();
        }
    }
}
