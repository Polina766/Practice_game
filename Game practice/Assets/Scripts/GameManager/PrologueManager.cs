using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;

public class PrologueManager : MonoBehaviour
{
    [Header("UI элементы")]
    public TextMeshProUGUI prologueText;      // Текст пролога
    public Button continueButton;              // Кнопка "Продолжить"
    public GameObject continueButtonObject;    // Объект кнопки (если нужно показывать/скрывать)

    [Header("Настройки печати")]
    [TextArea(5, 10)]
    public string fullPrologueText = "Здесь будет текст вашего пролога...\n\nМожно писать в несколько строк.\n\nИстория персонажа...";
    public float typingSpeed = 0.05f;          // Скорость печати

    [Header("Автоматическое завершение")]
    public bool autoCompleteOnClick = true;    // Можно ли пропустить кликом
    public bool enableContinueAfterTyping = true; // Показывать кнопку только после печати

    private bool isTyping = false;
    private bool isComplete = false;
    private Coroutine typingCoroutine;

    void Start()
    {
        // Скрываем кнопку в начале
        if (continueButton != null)
            continueButton.gameObject.SetActive(false);
        else if (continueButtonObject != null)
            continueButtonObject.SetActive(false);

        // Начинаем печать текста
        StartTyping();

        // Добавляем слушатель на кнопку
        if (continueButton != null)
            continueButton.onClick.AddListener(OnContinueClicked);
    }

    void StartTyping()
    {
        if (typingCoroutine != null)
            StopCoroutine(typingCoroutine);

        typingCoroutine = StartCoroutine(TypeText());
    }

    IEnumerator TypeText()
    {
        isTyping = true;
        prologueText.text = "";

        foreach (char letter in fullPrologueText.ToCharArray())
        {
            prologueText.text += letter;
            yield return new WaitForSeconds(typingSpeed);
        }

        isTyping = false;
        isComplete = true;

        // Показываем кнопку "Продолжить" после окончания печати
        if (enableContinueAfterTyping)
        {
            ShowContinueButton();
        }

        Debug.Log("📜 Пролог закончен, кнопка показана");
    }

    void ShowContinueButton()
    {
        if (continueButton != null)
            continueButton.gameObject.SetActive(true);
        else if (continueButtonObject != null)
            continueButtonObject.SetActive(true);
    }

    void OnContinueClicked()
    {
        Debug.Log("🔘 Нажата кнопка 'Продолжить'");

        // Запускаем следующую сцену
        if (GameManager.Instance != null)
        {
            GameManager.Instance.PrologueFinished();
        }
        else
        {
            // Если GameManager не найден (на случай тестирования)
            UnityEngine.SceneManagement.SceneManager.LoadScene("Hall");
        }
    }

    void Update()
    {
        // Пропуск анимации печати по клику (если включено)
        if (autoCompleteOnClick && isTyping && (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space)))
        {
            SkipTyping();
        }
    }

    void SkipTyping()
    {
        if (typingCoroutine != null)
            StopCoroutine(typingCoroutine);

        prologueText.text = fullPrologueText;
        isTyping = false;
        isComplete = true;

        if (enableContinueAfterTyping)
        {
            ShowContinueButton();
        }
    }
}