using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerSpawn : MonoBehaviour
{
    void Start()
    {
        // Получаем текущую сцену
        string currentScene = SceneManager.GetActiveScene().name;

        // Ищем сохранённую позицию для этой сцены
        string foundKey = null;
        Vector2 savedPosition = Vector2.zero;

        foreach (var key in SceneTransfer.doorPositions.Keys)
        {
            if (key.StartsWith(currentScene + "_"))
            {
                foundKey = key;
                savedPosition = SceneTransfer.doorPositions[key];
                break;
            }
        }

        if (foundKey != null)
        {
            // Телепортируем игрока
            transform.position = new Vector3(savedPosition.x, transform.position.y, transform.position.z);
            Debug.Log($"Игрок телепортирован к {foundKey}: {savedPosition}");

            // Удаляем использованную позицию
            SceneTransfer.doorPositions.Remove(foundKey);
        }
        else
        {
            Debug.Log($"Нет сохранённой позиции для сцены {currentScene}");
        }
    }
}