using UnityEngine;

public class OpenSettingsOnClick : MonoBehaviour
{
    public GameObject settingsPanelPrefab;  // Префаб панели настроек
    private GameObject currentSettingsPanel;

    public void OpenSettings()
    {
        // Скрываем шестерёнку
        gameObject.SetActive(false);

        // Скрываем главное меню
        GameObject mainMenu = GameObject.FindGameObjectWithTag("MainMenu");
        if (mainMenu != null) mainMenu.SetActive(false);

        // Создаём панель настроек
        if (settingsPanelPrefab != null)
        {
            currentSettingsPanel = Instantiate(settingsPanelPrefab);
            currentSettingsPanel.transform.SetParent(GetComponentInParent<Canvas>().transform, false);
        }
    }
}