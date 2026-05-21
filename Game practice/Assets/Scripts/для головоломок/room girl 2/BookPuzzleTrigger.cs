using UnityEngine;

public class PuzzleTrigger : MonoBehaviour
{
    public GameObject puzzleUI;
    private bool triggered = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!triggered && other.CompareTag("Player"))
        {
            triggered = true;
            puzzleUI.SetActive(true);
        }
    }
}