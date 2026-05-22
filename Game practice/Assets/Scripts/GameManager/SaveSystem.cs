using UnityEngine;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

[System.Serializable]
public class GameData
{
    public int currentStepIndex;      // Какой шаг пройден
    public string currentSceneName;    // На какой сцене вышли
    public bool hasSavedGame;          // Есть ли сохранение
    public float playerPosX;           // Позиция игрока (опционально)
    public float playerPosY;
}

public static class SaveSystem
{
    private static string savePath = Application.persistentDataPath + "/gamesave.save";

    public static void SaveGame(GameData data)
    {
        try
        {
            BinaryFormatter formatter = new BinaryFormatter();
            FileStream stream = new FileStream(savePath, FileMode.Create);
            formatter.Serialize(stream, data);
            stream.Close();
            Debug.Log("✅ Игра сохранена: шаг " + data.currentStepIndex + " на сцене " + data.currentSceneName);
        }
        catch (System.Exception e)
        {
            Debug.LogError("❌ Ошибка сохранения: " + e.Message);
        }
    }

    public static GameData LoadGame()
    {
        if (File.Exists(savePath))
        {
            try
            {
                BinaryFormatter formatter = new BinaryFormatter();
                FileStream stream = new FileStream(savePath, FileMode.Open);
                GameData data = formatter.Deserialize(stream) as GameData;
                stream.Close();
                Debug.Log("✅ Игра загружена: шаг " + data.currentStepIndex + " на сцене " + data.currentSceneName);
                return data;
            }
            catch (System.Exception e)
            {
                Debug.LogError("❌ Ошибка загрузки: " + e.Message);
                return null;
            }
        }
        else
        {
            Debug.Log("ℹ️ Файл сохранения не найден");
            return null;
        }
    }

    public static bool HasSavedGame()
    {
        return File.Exists(savePath);
    }

    public static void DeleteSave()
    {
        if (File.Exists(savePath))
        {
            File.Delete(savePath);
            Debug.Log("🗑️ Сохранение удалено");
        }
    }
}