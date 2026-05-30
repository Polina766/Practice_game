using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class CauldronPuzzle : MonoBehaviour
{
    [Header("Слоты (предметы)")]
    public Button[] slots;
    public Vector2 selectedScale = new Vector2(1.2f, 1.2f);
    public Vector2 normalScale = Vector2.one;

    [Header("Какие слоты правильные (индексы 0..8)")]
    public int[] correctSlots = { 0, 2, 5, 7 };

    [Header("Панель головоломки")]
    public GameObject puzzlePanel;

    [Header("Система паузы")]
    public GameObject player;
    public MonoBehaviour pauseManager;

    [Header("Настройки активации")]
    public bool activateOnApproach = true;
    public bool activateOnRightClick = true;
    public float activationRange = 3f;
    public KeyCode activationKey = KeyCode.Mouse1;

    [Header("Quest Trigger")]
    public QuestStepTrigger questTrigger;

    // Внутренние переменные
    private List<int> selectedIndices = new List<int>();
    private Dictionary<int, Vector3> originalScales = new Dictionary<int, Vector3>();
    private bool isPuzzleActive = false;
    private bool puzzleCompleted = false;
    private bool playerInRange = false;
    private Animator playerAnimator;
    private Rigidbody2D playerRigidbody;
    private MonoBehaviour playerController;
    private Collider2D triggerCollider;

    void Start()
    {
        for (int i = 0; i < slots.Length; i++)
        {
            int index = i;
            slots[i].onClick.AddListener(() => OnSlotClicked(index));
            originalScales[index] = slots[index].transform.localScale;
        }

        if (puzzlePanel != null)
            puzzlePanel.SetActive(false);

        if (player != null)
        {
            playerAnimator = player.GetComponent<Animator>();
            playerRigidbody = player.GetComponent<Rigidbody2D>();
            playerController = player.GetComponent<PlayerController>();
        }

        SetupTrigger();
    }

    void Update()
    {
        if (activateOnRightClick && !isPuzzleActive && playerInRange && !puzzleCompleted)
        {
            if (Input.GetMouseButtonDown(1))
            {
                OpenPuzzle();
            }
        }
    }

    void SetupTrigger()
    {
        if (GetComponent<Collider2D>() == null)
        {
            BoxCollider2D collider = gameObject.AddComponent<BoxCollider2D>();
            collider.isTrigger = true;
            collider.size = new Vector2(activationRange, activationRange);
            triggerCollider = collider;
        }
        else
        {
            triggerCollider = GetComponent<Collider2D>();
            triggerCollider.isTrigger = true;
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
            if (activateOnApproach && !puzzleCompleted)
            {
                OpenPuzzle();
            }
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            if (isPuzzleActive)
            {
                ClosePuzzle();
            }
        }
    }

    public void OpenPuzzle()
    {
        if (isPuzzleActive || puzzleCompleted) return;

        if (puzzlePanel != null)
            puzzlePanel.SetActive(true);
        PauseGame();
        isPuzzleActive = true;
    }

    public void ClosePuzzle()
    {
        if (!isPuzzleActive) return;

        ResumeGame();
        if (puzzlePanel != null)
            puzzlePanel.SetActive(false);
        isPuzzleActive = false;
        ResetPuzzle();
    }

    private void PauseGame()
    {
        Time.timeScale = 0f;
        if (playerAnimator != null) playerAnimator.speed = 0f;
        if (playerRigidbody != null)
        {
            playerRigidbody.simulated = false;
            playerRigidbody.linearVelocity = Vector2.zero;
        }
        if (pauseManager != null)
        {
            var method = pauseManager.GetType().GetMethod("PauseGame");
            if (method != null) method.Invoke(pauseManager, null);
        }
    }

    private void ResumeGame()
    {
        Time.timeScale = 1f;
        if (playerAnimator != null) playerAnimator.speed = 1f;
        if (playerRigidbody != null)
        {
            playerRigidbody.simulated = true;
            playerRigidbody.WakeUp();
        }
        if (pauseManager != null)
        {
            var method = pauseManager.GetType().GetMethod("ResumeGame");
            if (method != null) method.Invoke(pauseManager, null);
        }
    }

    void OnSlotClicked(int slotIndex)
    {
        if (!isPuzzleActive || puzzleCompleted) return;
        if (selectedIndices.Contains(slotIndex)) return;

        selectedIndices.Add(slotIndex);
        slots[slotIndex].transform.localScale = selectedScale;

        if (selectedIndices.Count == 4)
        {
            // Запускаем корутину через GameManager, чтобы она не умерла при отключении объекта
            if (GameManager.Instance != null)
            {
                GameManager.Instance.StartCoroutine(CheckSolutionCoroutine());
            }
            else
            {
                StartCoroutine(CheckSolutionCoroutine());
            }
        }
    }

    // Корутина, запускаемая через GameManager (он всегда активен)
    public IEnumerator CheckSolutionCoroutine()
    {
        bool allCorrect = true;
        foreach (int idx in selectedIndices)
        {
            if (!System.Array.Exists(correctSlots, element => element == idx))
            {
                allCorrect = false;
                break;
            }
        }

        if (allCorrect && selectedIndices.Count == correctSlots.Length)
        {
            Debug.Log("Зелье готово! 🧪");
            puzzleCompleted = true;

            // Уведомляем GameManager
            if (questTrigger != null)
            {
                questTrigger.NotifyManually();
            }

            // Ждём 1.5 секунды
            yield return new WaitForSecondsRealtime(1.5f);

            // Закрываем панель (если объект ещё существует)
            if (this != null && gameObject != null)
            {
                ClosePuzzle();
            }
        }
        else
        {
            Debug.Log("Ошибка! Сброс...");
            yield return new WaitForSecondsRealtime(0.3f);

            // Возвращаем размеры (если объект ещё существует)
            if (this != null && gameObject != null)
            {
                foreach (int idx in selectedIndices)
                {
                    if (slots[idx] != null)
                        slots[idx].transform.localScale = originalScales[idx];
                }
                selectedIndices.Clear();
            }
        }
    }

    public void ResetPuzzle()
    {
        foreach (int idx in selectedIndices)
        {
            slots[idx].transform.localScale = originalScales[idx];
        }
        selectedIndices.Clear();
    }

    void OnDestroy()
    {
        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i] != null)
                slots[i].onClick.RemoveAllListeners();
        }
    }
}