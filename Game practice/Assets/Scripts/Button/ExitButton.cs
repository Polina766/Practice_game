using UnityEngine;

public class ExitButton : MonoBehaviour
{
    public void ExitGame()
    {
     #if UNITY_EDITOR
        // Если ты в редакторе Unity — останавливаем игру
        UnityEditor.EditorApplication.isPlaying = false;
#else
            // Если это собранная игра (exe) — закрываем приложение
            Application.Quit();
#endif
    }
}
