using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using TMPro;

public class GameEnding : MonoBehaviour
{
    [Header("Финальная картинка")]
    public GameObject finalImage;
    public float finalImageDuration = 4f;

    [Header("Черный экран (поверх картинки)")]
    public GameObject blackScreen;
    public float blackScreenFadeTime = 1f;

    [Header("Текст The End")]
    public TextMeshProUGUI theEndText;

    private CanvasGroup blackCanvasGroup;
    private bool isEndingStarted = false;

    void Start()
    {
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
        if (isEndingStarted) return;
        isEndingStarted = true;
        StartCoroutine(PlayEndingSequence());
    }

    IEnumerator PlayEndingSequence()
    {
        // 1. Показываем финальную картинку
        if (finalImage != null)
        {
            finalImage.SetActive(true);
            Debug.Log("📸 Финальная картинка показана на 4 секунды");
        }

        // Ждём 4 секунды
        yield return new WaitForSeconds(finalImageDuration);

        // 2. Показываем черный экран ПОВЕРХ картинки
        if (blackScreen != null)
        {
            blackScreen.SetActive(true);

            float elapsed = 0f;
            while (elapsed < blackScreenFadeTime)
            {
                elapsed += Time.deltaTime;
                blackCanvasGroup.alpha = Mathf.Lerp(0f, 1f, elapsed / blackScreenFadeTime);
                yield return null;
            }
            blackCanvasGroup.alpha = 1f;
        }

        // 🔥 КОГДА ЧЁРНЫЙ ЭКРАН ПОЯВИЛСЯ - ИГРА ЗАВЕРШЕНА!
        if (GameManager.Instance != null)
        {
            GameManager.Instance.MarkGameAsCompleted();
            Debug.Log("🏆 Игра помечена как завершённая (чёрный экран)");
        }

        // 3. Показываем текст "The End"
        if (theEndText != null)
        {
            theEndText.gameObject.SetActive(true);
        }

        // Ждём 2 секунды перед возвратом в меню
        yield return new WaitForSeconds(2f);

        // 4. Возвращаемся в главное меню
        SceneManager.LoadScene("Menu");
    }
}