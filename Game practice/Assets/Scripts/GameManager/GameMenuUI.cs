using UnityEngine;
using UnityEngine.SceneManagement;

public class GameMenuUI : MonoBehaviour
{
    public void SaveAndExitToMenu()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.SaveAndGoToMenu();
        }
        else
        {
            SceneManager.LoadScene("Menu");
        }
    }

    public void ExitToMenuWithoutSave()
    {
        SceneManager.LoadScene("Menu");
    }
}