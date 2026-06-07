using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using TMPro;

public class GameEnding : MonoBehaviour
{
    [Header("Финальная картинка")]
    public GameObject finalImage;           // Картинка с персонажами
    public float finalImageDuration = 2f;   // Сколько секунд показывать картинку

    [Header("Черный экран")]
    public GameObject blackScreen;           // Черный экран (Image с черным цветом)
    public float blackScreenFadeTime = 1f;   // Время появления черного экрана
    public float blackScreenDisplayTime = 2f; // Сколько секунд показывать черный экран с текстом

    [Header("Текст The End")]
    public TextMeshProUGUI theEndText;       // Текст "The End"

    private CanvasGroup blackCanvasGroup;
    private CanvasGroup imageCanvasGroup;

    void Start()
    {
        // Скрываем всё в начале
        if (finalImage != null)
            finalImage.SetActive(false);

        if (blackScreen != null)
        {
            blackScreen.SetActive(false);
            blackCanvasGroup = blackScreen.GetComponent<CanvasGroup>();
            if (blackCanvasGroup == null)
                blackCanvasGroup = blackScreen.AddComponent<CanvasGroup>();
            blackCanvasGroup.alpha = 0f;
        }

        if (theEndText != null)
            theEndText.gameObject.SetActive(false);
    }

    public void StartEnding()
    {
        StartCoroutine(PlayEndingSequence());
    }

    IEnumerator PlayEndingSequence()
    {
        // 1. Показываем финальную картинку
        if (finalImage != null)
        {
            finalImage.SetActive(true);
            Debug.Log("📸 Финальная картинка показана");
        }

        // Ждём пару секунд
        yield return new WaitForSeconds(finalImageDuration);

        // 2. Скрываем картинку
        if (finalImage != null)
        {
            finalImage.SetActive(false);
        }

        // 3. Показываем черный экран
        if (blackScreen != null)
        {
            blackScreen.SetActive(true);

            // Плавно появляем черный экран
            float elapsed = 0f;
            while (elapsed < blackScreenFadeTime)
            {
                elapsed += Time.deltaTime;
                blackCanvasGroup.alpha = Mathf.Lerp(0f, 1f, elapsed / blackScreenFadeTime);
                yield return null;
            }
            blackCanvasGroup.alpha = 1f;
        }

        // 4. Показываем текст "The End"
        if (theEndText != null)
        {
            theEndText.gameObject.SetActive(true);
        }

        // Ждём
        yield return new WaitForSeconds(blackScreenDisplayTime);

        // 5. Возвращаемся в главное меню или перезапускаем игру
        SceneManager.LoadScene("Menu");
    }
}
