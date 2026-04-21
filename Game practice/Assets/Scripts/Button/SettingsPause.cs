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
    public Button[] uiButtons;

    public static bool isPaused = false;

    private Animator playerAnimator;
    private Rigidbody2D playerRigidbody;
    private PlayerController playerController; // ДОБАВЛЯЕМ

    void Start()
    {
        isPaused = false;
        Time.timeScale = 1f;

        if (player != null)
        {
            playerAnimator = player.GetComponent<Animator>();
            playerRigidbody = player.GetComponent<Rigidbody2D>();
            playerController = player.GetComponent<PlayerController>(); // ДОБАВЛЯЕМ
        }

        if (settingsPanel != null)
            settingsPanel.SetActive(false);
    }

    void Update()
    {
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

        if (playerAnimator != null)
            playerAnimator.speed = 0f;

        if (playerRigidbody != null)
        {
            playerRigidbody.simulated = false;
            playerRigidbody.linearVelocity = Vector2.zero;
        }

        // НЕ ОТКЛЮЧАЕМ PlayerController, просто останавливаем физику

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
        {
            playerRigidbody.simulated = true;
            // ПРИНУДИТЕЛЬНО ОБНОВЛЯЕМ
            playerRigidbody.WakeUp();
        }

        if (settingsPanel != null)
            settingsPanel.SetActive(false);

        Debug.Log("Пауза снята, физика должна работать");
    }
}