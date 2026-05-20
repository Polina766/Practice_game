using UnityEngine;
using UnityEngine.SceneManagement;

public class PuzzleTransition : MonoBehaviour
{
    public string puzzleSceneName = "CardPuzzleScene";
    public string playerTag = "Player";

    [Header("GameManager Integration")]
    public GameManager.StoryStep requiredStep = GameManager.StoryStep.Puzzle1;

    private static bool puzzleCompleted = false;
    private Collider2D myCollider;

    void Start()
    {
        myCollider = GetComponent<Collider2D>();

        // Проверяем сохранение
        if (PlayerPrefs.HasKey("CardPuzzleCompleted"))
            puzzleCompleted = PlayerPrefs.GetInt("CardPuzzleCompleted", 0) == 1;
        else
            puzzleCompleted = false;

        if (puzzleCompleted)
        {
            if (myCollider != null) myCollider.enabled = false;
            return;
        }

        // Проверяем доступность по шагу
        CheckAvailability();
    }

    public void CheckAvailability()
    {
        if (GameManager.Instance == null) return;
        if (puzzleCompleted) return;

        bool shouldBeActive = (GameManager.Instance.GetCurrentStep() == requiredStep);
        if (myCollider != null) myCollider.enabled = shouldBeActive;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // Проверка через GameManager
        if (GameManager.Instance != null && GameManager.Instance.GetCurrentStep() != requiredStep)
        {
            Debug.Log($"Карточная головоломка недоступна на шаге {GameManager.Instance.GetCurrentStep()}");
            return;
        }

        if (other.CompareTag(playerTag) && !puzzleCompleted)
        {
            SceneManager.LoadScene(puzzleSceneName);
        }
    }

    public static void CompletePuzzle()
    {
        puzzleCompleted = true;
        PlayerPrefs.SetInt("CardPuzzleCompleted", 1);
        PlayerPrefs.Save();

        if (GameManager.Instance != null)
        {
            GameManager.Instance.CompletePuzzle("CardPuzzle");
        }
    }
}