using UnityEngine;
using UnityEngine.EventSystems;

public class DraggableBook : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public int bookIndex;

    private CanvasGroup canvasGroup;
    private Vector2 startPosition;
    private Canvas canvas;
    private RectTransform rectTransform;

    void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        canvas = GetComponentInParent<Canvas>();
        rectTransform = GetComponent<RectTransform>();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        startPosition = rectTransform.anchoredPosition;
        canvasGroup.blocksRaycasts = false;
        canvasGroup.alpha = 0.7f;
    }

    public void OnDrag(PointerEventData eventData)
    {
        rectTransform.anchoredPosition += eventData.delta / canvas.scaleFactor;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        canvasGroup.blocksRaycasts = true;
        canvasGroup.alpha = 1f;

        var results = new System.Collections.Generic.List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, results);

        foreach (var hit in results)
        {
            BookSlot slot = hit.gameObject.GetComponent<BookSlot>();
            if (slot != null)
            {
                BookPuzzleManager.SwapBooks(this, slot);
                return;
            }
        }

        rectTransform.anchoredPosition = startPosition;
    }

    public void SetPosition(Vector2 position)
    {
        rectTransform.anchoredPosition = position;
    }
}