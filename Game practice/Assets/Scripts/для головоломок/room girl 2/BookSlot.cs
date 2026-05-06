using UnityEngine;
using UnityEngine.EventSystems;

public class BookSlot : MonoBehaviour, IDropHandler
{
    public int requiredBookIndex;

    private DraggableBook currentBook;
    private BookPuzzleManager manager;

    void Start()
    {
        manager = FindAnyObjectByType<BookPuzzleManager>();
    }

    public void OnDrop(PointerEventData eventData)
    {
        DraggableBook draggedBook = eventData.pointerDrag.GetComponent<DraggableBook>();
        if (draggedBook == null) return;

        // Находим слот, откуда пришла книга (если книга была в слоте)
        BookSlot sourceSlot = draggedBook.GetComponentInParent<BookSlot>();

        // СЛУЧАЙ 1: Слот пустой
        if (currentBook == null)
        {
            // Если книга была в другом слоте - очищаем тот слот
            if (sourceSlot != null)
            {
                sourceSlot.currentBook = null;
            }

            // Ставим книгу в текущий слот
            currentBook = draggedBook;
            draggedBook.transform.SetParent(transform);
            draggedBook.transform.localPosition = Vector3.zero;
        }
        // СЛУЧАЙ 2: Слот занят
        else
        {
            // Запоминаем книгу, которая уже в слоте
            DraggableBook oldBook = currentBook;

            // Если книга пришла из другого слота
            if (sourceSlot != null)
            {
                // Ставим новую книгу в текущий слот
                currentBook = draggedBook;
                draggedBook.transform.SetParent(transform);
                draggedBook.transform.localPosition = Vector3.zero;

                // Старую книгу ставим в слот, откуда пришла новая
                sourceSlot.currentBook = oldBook;
                oldBook.transform.SetParent(sourceSlot.transform);
                oldBook.transform.localPosition = Vector3.zero;
            }
            else
            {
                // Книга пришла из панели - меняем местами
                currentBook = draggedBook;
                draggedBook.transform.SetParent(transform);
                draggedBook.transform.localPosition = Vector3.zero;

                // Старую книгу отправляем в панель
                oldBook.transform.SetParent(manager.booksPanel);
                oldBook.transform.localPosition = Vector3.zero;
                oldBook.GetComponent<DraggableBook>().ReturnToStart();
            }
        }

        // Проверяем победу
        if (manager != null)
            manager.CheckWin();
    }

    public void ClearSlot()
    {
        currentBook = null;
    }

    public DraggableBook GetCurrentBook()
    {
        return currentBook;
    }

    public bool IsCorrect()
    {
        if (currentBook == null) return false;
        return currentBook.bookIndex == requiredBookIndex;
    }
}