using UnityEngine;
using System.Collections;

public class PuzzleTrigger : MonoBehaviour
{
    public GameObject puzzleUI;

    [Header("GameManager Integration")]
    public GameManager.StoryStep requiredStep = GameManager.StoryStep.Puzzle3;

    private bool triggered = false;
    private Collider2D myCollider;
    private bool isCompleted = false;

    void Start()
    {
        myCollider = GetComponent<Collider2D>();

        // Проверяем сохранение
        if (PlayerPrefs.GetInt("BooksPuzzleCompleted", 0) == 1)
        {
            isCompleted = true;
            if (myCollider != null) myCollider.enabled = false;
            return;
        }

        CheckAvailability();
    }

    public void CheckAvailability()
    {
        if (GameManager.Instance == null) return;
        if (isCompleted || triggered) return;

        bool shouldBeActive = (GameManager.Instance.GetCurrentStep() == requiredStep);
        if (myCollider != null) myCollider.enabled = shouldBeActive;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Проверка через GameManager
        if (GameManager.Instance != null && GameManager.Instance.GetCurrentStep() != requiredStep)
        {
            Debug.Log("Головоломка с книгами недоступна сейчас");
            return;
        }

        if (!triggered && !isCompleted && other.CompareTag("Player"))
        {
            triggered = true;
            if (puzzleUI != null)
                puzzleUI.SetActive(true);

            // Подписываемся на закрытие UI
            StartCoroutine(WaitForPuzzleComplete());

            if (myCollider != null)
                myCollider.enabled = false;
        }
    }

    private IEnumerator WaitForPuzzleComplete()
    {
        // Ждём пока UI головоломки активен
        while (puzzleUI != null && puzzleUI.activeSelf)
        {
            yield return null;
        }

        // Головоломка закрыта - сообщаем GameManager
        isCompleted = true;
        PlayerPrefs.SetInt("BooksPuzzleCompleted", 1);
        PlayerPrefs.Save();

        if (GameManager.Instance != null)
        {
            GameManager.Instance.CompletePuzzle("BooksPuzzle");
        }
    }
}