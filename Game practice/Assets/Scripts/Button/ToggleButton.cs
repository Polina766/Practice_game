using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ToggleButton : MonoBehaviour, IPointerClickHandler
{
    public bool isMouseButton;
    public GameObject checkmark;

    void Start()
    {
        UpdateCheckmark();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (isMouseButton)
            ControlManager.Instance.SelectMouse();
        else
            ControlManager.Instance.SelectKeyboard();

        UpdateCheckmark();

        // НАХОДИМ ПЕРСОНАЖА И ОСТАНАВЛИВАЕМ ЕГО
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            PlayerController controller = player.GetComponent<PlayerController>();
            if (controller != null)
            {
                controller.StopAllMovement();
            }
        }
    }

    void UpdateCheckmark()
    {
        bool isActive = (isMouseButton && ControlManager.Instance.useMouseMovement) ||
                       (!isMouseButton && !ControlManager.Instance.useMouseMovement);

        if (checkmark != null)
            checkmark.SetActive(isActive);
    }
}