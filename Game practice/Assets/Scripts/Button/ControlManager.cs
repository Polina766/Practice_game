using UnityEngine;

public class ControlManager : MonoBehaviour
{
    public static ControlManager Instance;

    public bool useMouseMovement = false; // false - клавиатура, true - мышь

    void Awake()
    {
        Instance = this;
    }

    public void SelectKeyboard()
    {
        useMouseMovement = false;
    }

    public void SelectMouse()
    {
        useMouseMovement = true;
    }
}