using UnityEngine;
using System.Collections.Generic;

public class BookPuzzleManager : MonoBehaviour
{
    public BookSlot[] slots;
    public Transform booksPanel;
    public GameObject closeButton;  // 👈 Кнопка крестика (перетащи сюда)

    private bool gameWon = false;

    void Start()
    {
        ShuffleBooks();

        // Скрываем кнопку в начале
        if (closeButton != null)
            closeButton.SetActive(false);
    }

    void ShuffleBooks()
    {
        List<DraggableBook> books = new List<DraggableBook>();

        foreach (Transform child in booksPanel)
        {
            DraggableBook book = child.GetComponent<DraggableBook>();
            if (book != null)
            {
                books.Add(book);
            }
        }

        // Перемешиваем
        for (int i = 0; i < books.Count; i++)
        {
            DraggableBook temp = books[i];
            int randomIndex = Random.Range(i, books.Count);
            books[i] = books[randomIndex];
            books[randomIndex] = temp;
        }

        // Расставляем
        for (int i = 0; i < books.Count; i++)
        {
            books[i].transform.SetParent(booksPanel);
            books[i].transform.SetSiblingIndex(i);
            books[i].transform.localPosition = Vector3.zero;
        }
    }

    public void CheckWin()
    {
        if (gameWon) return;

        // Проверяем все ли слоты заняты
        foreach (BookSlot slot in slots)
        {
            if (slot.GetCurrentBook() == null)
                return;
        }

        // Проверяем правильность
        bool allCorrect = true;
        for (int i = 0; i < slots.Length; i++)
        {
            if (!slots[i].IsCorrect())
            {
                allCorrect = false;
                break;
            }
        }

        if (allCorrect)
        {
            gameWon = true;
            Debug.Log("ПОБЕДА! Показываем кнопку крестика...");

            // ПОКАЗЫВАЕМ КНОПКУ КРЕСТИКА
            if (closeButton != null)
                closeButton.SetActive(true);
        }
    }

    // Этот метод повесишь на кнопку крестика
    public void ClosePuzzle()
    {
        // Скрываем всю головоломку
        transform.parent.gameObject.SetActive(false); // Если менеджер внутри PuzzleGame
        // Или если кнопка отдельно: gameObject.SetActive(false);
        Debug.Log("Головоломка закрыта");
    }
}