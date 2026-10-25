using UnityEngine;
using System.Collections.Generic;

public class BookPuzzleManager : MonoBehaviour
{
    public BookSlot[] slots;
    public GameObject closeButton;
    public DraggableBook[] books;

    // ВРУЧНУЮ НАЗНАЧАЕМ, какая книга в каком слоте изначально
    public int[] initialSlotMapping; // Например: [2, 0, 3, 1] значит книга 2 в слоте 0, книга 0 в слоте 1 и т.д.

    private Dictionary<DraggableBook, Vector2> bookPositions;
    private Dictionary<BookSlot, DraggableBook> slotContents;
    private bool won = false;

    void Start()
    {
        if (closeButton != null)
            closeButton.SetActive(false);

        bookPositions = new Dictionary<DraggableBook, Vector2>();
        slotContents = new Dictionary<BookSlot, DraggableBook>();

        // Запоминаем позиции всех книг
        foreach (DraggableBook book in books)
        {
            bookPositions[book] = book.GetComponent<RectTransform>().anchoredPosition;
        }

        // Очищаем слоты
        foreach (BookSlot slot in slots)
        {
            slotContents[slot] = null;
        }

        // НАЗНАЧАЕМ КНИГИ В СЛОТЫ ВРУЧНУЮ
        for (int i = 0; i < slots.Length && i < initialSlotMapping.Length; i++)
        {
            int bookIndex = initialSlotMapping[i];
            if (bookIndex >= 0 && bookIndex < books.Length)
            {
                slotContents[slots[i]] = books[bookIndex];
                // Ставим книгу на позицию слота
                books[bookIndex].SetPosition(slots[i].slotRect.anchoredPosition);
                bookPositions[books[bookIndex]] = slots[i].slotRect.anchoredPosition;
                
            }
        }
    }

    public static void SwapBooks(DraggableBook draggedBook, BookSlot targetSlot)
    {
        BookPuzzleManager manager = FindFirstObjectByType<BookPuzzleManager>();
        manager.SwapBooksInternal(draggedBook, targetSlot);
    }

    private void SwapBooksInternal(DraggableBook draggedBook, BookSlot targetSlot)
    {
        // Находим слот, в котором сейчас стоит перетаскиваемая книга
        BookSlot sourceSlot = null;
        foreach (var kvp in slotContents)
        {
            if (kvp.Value == draggedBook)
            {
                sourceSlot = kvp.Key;
                break;
            }
        }

        // Книга, которая стоит в целевом слоте
        DraggableBook targetBook = slotContents[targetSlot];

        // Позиция целевого слота
        Vector2 targetSlotPos = targetSlot.slotRect.anchoredPosition;

        

        if (sourceSlot != null)
        {
            // Книга была в слоте
            if (targetBook != null)
            {
                // МЕНЯЕМ КНИГИ МЕСТАМИ
                targetBook.SetPosition(bookPositions[draggedBook]);
                bookPositions[targetBook] = bookPositions[draggedBook];
                slotContents[sourceSlot] = targetBook;
                
            }
            else
            {
                // Целевой слот пуст
                slotContents[sourceSlot] = null;
            }
        }

        // Перетаскиваемая книга летит в целевой слот
        draggedBook.SetPosition(targetSlotPos);
        bookPositions[draggedBook] = targetSlotPos;
        slotContents[targetSlot] = draggedBook;

        

        CheckWin();
    }

    private void CheckWin()
    {
        if (won) return;

        foreach (BookSlot slot in slots)
        {
            DraggableBook book = slotContents[slot];
            if (book == null) return;
            if (book.bookIndex != slot.requiredBookIndex) return;
        }

        won = true;
        
        if (closeButton != null)
            closeButton.SetActive(true);
    }

    public void ClosePuzzle()
    {
        transform.parent.gameObject.SetActive(false);
    }
}