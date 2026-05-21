using UnityEngine;
using UnityEngine.EventSystems;

public class MovePuzzle : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler
{
    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;
    private Vector2 startPosition;

    public int pieceIndex;
    private GameObject currentSlot;
    private bool isDragging = false;
    private bool isLocked = false; // Заблокирован ТОЛЬКО на правильном месте

    private GameManagerPuzzle gameManager;

    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();

        startPosition = rectTransform.anchoredPosition;

        gameManager = FindObjectOfType<GameManagerPuzzle>();

        Debug.Log($"Пазл {pieceIndex} готов");
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (isLocked) // Если на правильном месте - нельзя двигать
        {
            Debug.Log($"Пазл {pieceIndex} на своём месте! Его нельзя двигать.");
            return;
        }

        isDragging = true;
        canvasGroup.alpha = 0.6f;
        canvasGroup.blocksRaycasts = false;

        if (currentSlot != null)
        {
            PuzzleSlot slot = currentSlot.GetComponent<PuzzleSlot>();
            if (slot != null)
            {
                int slotIndex = slot.slotIndex;
                slot.RemovePiece();

                if (gameManager != null)
                    gameManager.CheckPieceRemoved(pieceIndex, slotIndex);
            }
            currentSlot = null;
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!isDragging || isLocked) return;
        rectTransform.position = eventData.position;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (isLocked) return;

        isDragging = false;
        canvasGroup.alpha = 1f;
        canvasGroup.blocksRaycasts = true;

        GameObject slotUnderMouse = GetSlotUnderMouse();

        if (slotUnderMouse != null)
        {
            PuzzleSlot slot = slotUnderMouse.GetComponent<PuzzleSlot>();

            if (slot != null && !slot.isOccupied)
            {
                rectTransform.position = slotUnderMouse.transform.position;
                currentSlot = slotUnderMouse;
                slot.PlacePiece(this);

                Debug.Log($"Пазл {pieceIndex} поставлен в слот {slot.slotIndex}");

                // ПРОВЕРКА: правильное ли это место?
                if (pieceIndex == slot.slotIndex)
                {
                    // НА ПРАВИЛЬНОМ МЕСТЕ - блокируем!
                    isLocked = true;
                    canvasGroup.blocksRaycasts = false; // Нельзя кликнуть
                    Debug.Log($"🔒 Пазл {pieceIndex} на СВОЁМ месте! Заблокирован, нельзя двигать.");
                }
                else
                {
                    // НЕ НА ПРАВИЛЬНОМ МЕСТЕ - можно двигать
                    isLocked = false;
                    canvasGroup.blocksRaycasts = true;
                    Debug.Log($"❌ Пазл {pieceIndex} НЕ на своём месте (слот {slot.slotIndex}). Можно двигать.");
                }

                if (gameManager != null)
                    gameManager.CheckPiecePlaced(pieceIndex, slot.slotIndex);
            }
            else
            {
                rectTransform.anchoredPosition = startPosition;
            }
        }
        else
        {
            rectTransform.anchoredPosition = startPosition;
        }
    }

    private GameObject GetSlotUnderMouse()
    {
        PointerEventData pointerData = new PointerEventData(EventSystem.current);
        pointerData.position = Input.mousePosition;

        var results = new System.Collections.Generic.List<RaycastResult>();
        EventSystem.current.RaycastAll(pointerData, results);

        foreach (var result in results)
        {
            PuzzleSlot slot = result.gameObject.GetComponent<PuzzleSlot>();
            if (slot != null)
                return result.gameObject;
        }
        return null;
    }

    public void ResetPiece()
    {
        if (currentSlot != null)
        {
            PuzzleSlot slot = currentSlot.GetComponent<PuzzleSlot>();
            if (slot != null)
                slot.RemovePiece();
            currentSlot = null;
        }

        rectTransform.anchoredPosition = startPosition;
        isLocked = false;
        canvasGroup.blocksRaycasts = true;
        canvasGroup.alpha = 1f;
    }

    public bool IsOnCorrectSlot()
    {
        return isLocked;
    }
}