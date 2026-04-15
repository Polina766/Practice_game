using UnityEngine;
using UnityEngine.UI;

public class PauseManager : MonoBehaviour
{
    [Header("Объекты для паузы")]
    public GameObject settingsPanel;
    public GameObject player;

    [Header("Аниматоры которые должны работать на паузе")]
    public Animator[] uiAnimators;

    [Header("Кнопки которые должны работать на паузе")]
    public Button[] uiButtons; // Сюда перетащи кнопку Back Button

    public static bool isPaused = false;

    private Animator playerAnimator;
    private Rigidbody2D playerRigidbody;

    void Start()
    {
        isPaused = false;
        Time.timeScale = 1f;

        if (player != null)
        {
            playerAnimator = player.GetComponent<Animator>();
            playerRigidbody = player.GetComponent<Rigidbody2D>();
        }

        if (settingsPanel != null)
            settingsPanel.SetActive(false);
    }

    void Update()
    {
        // Если игра на паузе - принудительно обновляем UI аниматоры
        if (isPaused && uiAnimators != null)
        {
            foreach (Animator anim in uiAnimators)
            {
                if (anim != null)
                {
                    anim.Update(Time.unscaledDeltaTime);
                }
            }
        }
    }

    public void PauseGame()
    {
        if (isPaused) return;

        isPaused = true;
        Time.timeScale = 0f;

        // Останавливаем персонажа
        if (playerAnimator != null)
            playerAnimator.speed = 0f;

        if (playerRigidbody != null)
        {
            playerRigidbody.simulated = false;
            playerRigidbody.linearVelocity = Vector2.zero;
        }

        // ВСЕ КНОПКИ продолжают работать на паузе
        // Ничего с ними не делаем

        if (settingsPanel != null)
            settingsPanel.SetActive(true);
    }

    public void ResumeGame()
    {
        if (!isPaused) return;

        isPaused = false;
        Time.timeScale = 1f;

        if (playerAnimator != null)
            playerAnimator.speed = 1f;

        if (playerRigidbody != null)
            playerRigidbody.simulated = true;

        if (settingsPanel != null)
            settingsPanel.SetActive(false);
    }
}