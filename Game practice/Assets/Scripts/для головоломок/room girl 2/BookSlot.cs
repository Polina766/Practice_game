using UnityEngine;

public class BookSlot : MonoBehaviour
{
    public int requiredBookIndex;
    public RectTransform slotRect;

    void Start()
    {
        slotRect = GetComponent<RectTransform>();
    }
}