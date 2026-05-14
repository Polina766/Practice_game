using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Collections;

public class CauldronPuzzle : MonoBehaviour
{
    [Header("Слоты (предметы)")]
    public Button[] slots;  // перетащи 9 кнопок сюда
    public Vector2 selectedScale = new Vector2(1.2f, 1.2f);
    public Vector2 normalScale = Vector2.one;

    [Header("Какие слоты правильные (индексы 0..8)")]
    public int[] correctSlots = { 0, 2, 5, 7 };  // пример: 4 правильных слота

    [Header("Анимация и UI")]
    public Animator cauldronAnimator;  // твой котел с анимацией
    public Button closeButton;         // кнопка крестика
    public string winTriggerName = "Brew";  // имя триггера анимации

    [Header("Панель головоломки")]
    public GameObject puzzlePanel;  // ВСЯ панель головоломки (родитель всех слотов)

    [Header("Система паузы")]
    public GameObject player;  // игрок для остановки
    public MonoBehaviour pauseManager;  // можно перетащить PauseManager сюда, если он есть

    [Header("Настройки активации")]
    public bool activateOnApproach = true;  // активировать при приближении
    public bool activateOnRightClick = true; // активировать по правой кнопке мыши
    public float activationRange = 3f;  // дистанция активации
    public KeyCode activationKey = KeyCode.Mouse1;  // правая кнопка мыши

    // Внутренние переменные
    private List<int> selectedIndices = new List<int>();
    private Dictionary<int, Vector3> originalScales = new Dictionary<int, Vector3>();
    private bool isPuzzleActive = false;  // активна ли головоломка
    private bool playerInRange = false;   // игрок в зоне триггера
    private Animator playerAnimator;
    private Rigidbody2D playerRigidbody;
    private MonoBehaviour playerController;
    private Collider2D triggerCollider;    // коллайдер котла/триггера

    void Start()
    {
        // Сохраняем исходный размер каждого слота
        for (int i = 0; i < slots.Length; i++)
        {
            int index = i; // локальная копия для замыкания
            slots[i].onClick.AddListener(() => OnSlotClicked(index));
            originalScales[index] = slots[index].transform.localScale;
        }

        // Прячем крестик в начале
        if (closeButton != null)
            closeButton.gameObject.SetActive(false);

        // Прячем панель головоломки в начале
        if (puzzlePanel != null)
            puzzlePanel.SetActive(false);

        // Добавляем слушатель на кнопку крестика
        if (closeButton != null)
            closeButton.onClick.AddListener(ClosePuzzle);

        // Сохраняем компоненты игрока
        if (player != null)
        {
            playerAnimator = player.GetComponent<Animator>();
            playerRigidbody = player.GetComponent<Rigidbody2D>();
            playerController = player.GetComponent<PlayerController>();
        }

        // Настраиваем триггер
        SetupTrigger();
    }

    void Update()
    {
        // Проверяем активацию по правой кнопке мыши
        if (activateOnRightClick && !isPuzzleActive && playerInRange)
        {
            if (Input.GetMouseButtonDown(1)) // 1 - правая кнопка
            {
                OpenPuzzle();
            }
        }
    }

    void SetupTrigger()
    {
        // Добавляем коллайдер на объект котла, если его нет
        if (GetComponent<Collider2D>() == null)
        {
            // Создаём коллайдер-триггер
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
            Debug.Log("Игрок подошел к котлу. Можно открыть головоломку!");

            // Если активация при приближении включена
            if (activateOnApproach)
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
            Debug.Log("Игрок отошел от котла");

            // Если головоломка открыта и игрок ушел - закрываем
            if (isPuzzleActive)
            {
                ClosePuzzle();
            }
        }
    }

    // Метод для открытия головоломки
    public void OpenPuzzle()
    {
        if (isPuzzleActive) return;

        // Показываем панель головоломки
        if (puzzlePanel != null)
            puzzlePanel.SetActive(true);

        // Ставим игру на паузу
        PauseGame();

        isPuzzleActive = true;
        Debug.Log("Головоломка открыта, игра на паузе");
    }

    // Метод для закрытия головоломки (по крестику)
    public void ClosePuzzle()
    {
        if (!isPuzzleActive) return;

        // Снимаем паузу
        ResumeGame();

        // Прячем панель головоломки
        if (puzzlePanel != null)
            puzzlePanel.SetActive(false);

        isPuzzleActive = false;

        // Сбрасываем головоломку для следующего раза
        ResetPuzzle();

        Debug.Log("Головоломка закрыта, игра продолжается");
    }

    // Поставить игру на паузу
    private void PauseGame()
    {
        // Останавливаем время
        Time.timeScale = 0f;

        // Останавливаем анимацию игрока
        if (playerAnimator != null)
            playerAnimator.speed = 0f;

        // Останавливаем физику игрока
        if (playerRigidbody != null)
        {
            playerRigidbody.simulated = false;
            playerRigidbody.linearVelocity = Vector2.zero;
        }

        // Если есть PauseManager - используем его
        if (pauseManager != null)
        {
            var method = pauseManager.GetType().GetMethod("PauseGame");
            if (method != null)
                method.Invoke(pauseManager, null);
        }
    }

    // Снять игру с паузы
    private void ResumeGame()
    {
        // Возвращаем время
        Time.timeScale = 1f;

        // Возвращаем анимацию игрока
        if (playerAnimator != null)
            playerAnimator.speed = 1f;

        // Возвращаем физику игрока
        if (playerRigidbody != null)
        {
            playerRigidbody.simulated = true;
            playerRigidbody.WakeUp();
        }

        // Если есть PauseManager - используем его
        if (pauseManager != null)
        {
            var method = pauseManager.GetType().GetMethod("ResumeGame");
            if (method != null)
                method.Invoke(pauseManager, null);
        }

        Debug.Log("Пауза снята");
    }

    void OnSlotClicked(int slotIndex)
    {
        // Если головоломка не активна или уже победили — запрещаем
        if (!isPuzzleActive || (closeButton != null && closeButton.gameObject.activeSelf))
            return;

        // Если этот слот уже выбран — игнорируем
        if (selectedIndices.Contains(slotIndex))
            return;

        // Добавляем в список выбранных
        selectedIndices.Add(slotIndex);

        // Увеличиваем предмет
        slots[slotIndex].transform.localScale = selectedScale;

        // Если выбрали 4 предмета — проверяем
        if (selectedIndices.Count == 4)
        {
            StartCoroutine(CheckSolution());
        }
    }

    IEnumerator CheckSolution()
    {
        // Проверяем, все ли выбранные индексы входят в correctSlots
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
            // ПОБЕДА!
            Debug.Log("Зелье готово!");

            // Запускаем анимацию котла (используем реальное время)
            if (cauldronAnimator != null)
                cauldronAnimator.SetTrigger(winTriggerName);

            // Показываем крестик
            if (closeButton != null)
                closeButton.gameObject.SetActive(true);
        }
        else
        {
            // НЕПРАВИЛЬНО — сброс
            Debug.Log("Ошибка! Сброс...");

            // Ждем 0.3 секунды в реальном времени (не зависит от Time.timeScale)
            yield return new WaitForSecondsRealtime(0.3f);

            // Возвращаем все слоты в исходный размер
            foreach (int idx in selectedIndices)
            {
                slots[idx].transform.localScale = originalScales[idx];
            }

            // Очищаем список выбранных
            selectedIndices.Clear();
        }
    }

    // Сброс головоломки
    public void ResetPuzzle()
    {
        // Возвращаем всё как в начале
        foreach (int idx in selectedIndices)
        {
            slots[idx].transform.localScale = originalScales[idx];
        }
        selectedIndices.Clear();

        if (closeButton != null)
            closeButton.gameObject.SetActive(false);

        // Останавливаем анимацию котла
        if (cauldronAnimator != null)
            cauldronAnimator.Rebind();
    }

    void OnDestroy()
    {
        // Убираем слушатели при уничтожении
        if (closeButton != null)
            closeButton.onClick.RemoveListener(ClosePuzzle);

        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i] != null)
                slots[i].onClick.RemoveAllListeners();
        }
    }
}