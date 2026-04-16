using UnityEngine;
using UnityEngine.SceneManagement; // Обязательно подключите эту библиотеку для работы со сценами

public class SceneLoader : MonoBehaviour
{
    // Эта функция будет вызвана при нажатии на кнопку
    public void LoadTargetScene(string sceneName)
    {
        // Загружаем сцену по её имени [citation:1][citation:3]
        SceneManager.LoadScene(sceneName);
    }
}
