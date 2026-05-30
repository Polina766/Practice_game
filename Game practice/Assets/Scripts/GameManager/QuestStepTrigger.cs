using UnityEngine;

public class QuestStepTrigger : MonoBehaviour
{
    [Tooltip("Уникальный ID этого триггера")]
    public string triggerID;

    [Tooltip("Если true - триггер срабатывает при ВЫХОДЕ из триггера, а не при входе")]
    public bool triggerOnExit = false;

    [Tooltip("Если true - триггер НЕ срабатывает автоматически, только через NotifyManually()")]
    public bool manualOnly = false;

    public float delayBeforeReport = 0f;

    private bool hasNotified = false;

    void Start()
    {
        if (string.IsNullOrEmpty(triggerID))
            triggerID = gameObject.name;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (manualOnly) return;
        if (triggerOnExit) return;

        if (other.CompareTag("Player") && !hasNotified)
        {
            NotifyQuestCompleted();
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (manualOnly) return;

        if (triggerOnExit && other.CompareTag("Player") && !hasNotified)
        {
            Debug.Log($"🚪 Игрок вышел из триггера '{triggerID}', отправляем уведомление!");
            NotifyQuestCompleted();
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (manualOnly) return;
        if (triggerOnExit) return;

        if (other.CompareTag("Player") && !hasNotified)
        {
            NotifyQuestCompleted();
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (manualOnly) return;

        if (triggerOnExit && other.CompareTag("Player") && !hasNotified)
        {
            Debug.Log($"🚪 Игрок вышел из триггера '{triggerID}', отправляем уведомление!");
            NotifyQuestCompleted();
        }
    }

    public void NotifyQuestCompleted()
    {
        if (hasNotified) return;

        if (GameManager.Instance != null)
        {
            if (delayBeforeReport > 0)
                Invoke(nameof(SendReport), delayBeforeReport);
            else
                SendReport();
        }
    }

    // ✅ ДОБАВЬТЕ ЭТОТ МЕТОД
    public void NotifyManually()
    {
        Debug.Log($"🎯 QuestStepTrigger '{triggerID}' вызван РУЧНО!");
        NotifyQuestCompleted();
    }

    void SendReport()
    {
        GameManager.Instance.ReportTrigger(triggerID);
        hasNotified = true;
    }

    void OnDrawGizmos()
    {
        Color color = Color.yellow;
        if (manualOnly) color = Color.green;
        if (triggerOnExit) color = Color.blue;

        Gizmos.color = color;
        Gizmos.DrawWireCube(transform.position, Vector3.one);
    }
}