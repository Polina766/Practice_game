using UnityEngine;

public class QuestStepTrigger : MonoBehaviour
{
    [Tooltip("Уникальный ID этого триггера")]
    public string triggerID;

    public float delayBeforeReport = 0f;

    private bool alreadyReported = false;
    private bool hasNotified = false;

    void Start()
    {
        if (string.IsNullOrEmpty(triggerID))
            triggerID = gameObject.name;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && !hasNotified)
        {
            NotifyQuestCompleted();
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !hasNotified)
        {
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

    void SendReport()
    {
        GameManager.Instance.ReportTrigger(triggerID);
        hasNotified = true;
    }

    // Для отладки
    void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(transform.position, Vector3.one);
    }
}