using UnityEngine;

public class CloseSettingsButton : MonoBehaviour
{
    public void CloseSettings()
    {
        // Закрываем панель настроек
        transform.parent.gameObject.SetActive(false);

        // Показываем шестерёнку
        GameObject gearButton = GameObject.FindGameObjectWithTag("GearButton");
        if (gearButton != null) gearButton.SetActive(true);

        // Показываем главное меню
        GameObject mainMenu = GameObject.FindGameObjectWithTag("MainMenu");
        if (mainMenu != null) mainMenu.SetActive(true);
    }
}