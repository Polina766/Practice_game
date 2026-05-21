using UnityEngine;
using UnityEngine.SceneManagement;

public class PuzzleTransition : MonoBehaviour
{
    public string puzzleSceneName = "CardPuzzleScene";
    public string playerTag = "Player";

    private static bool puzzleCompleted = false;

    void Start()
    {
        if (puzzleCompleted)
        {
            Destroy(gameObject);
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
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
    }
}