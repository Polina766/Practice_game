using UnityEngine;

public class GameManagerPuzzle : MonoBehaviour
{
    public GameObject winButton; // Крестик (кнопка)
    public GameObject puzzleContainer; // Контейнер со всей головоломкой
    public string playerTag = "Player"; // Тег игрока

    private int totalPieces = 25;
    private int piecesInPlace = 0;
    private bool isPuzzleActive = false; // Активна ли головоломка
    private MovePuzzle[] allPieces;
    private PuzzleSlot[] allSlots;

    void Start()
    {
        // Крестик скрыт в начале
        if (winButton != null)
            winButton.SetActive(false);

        // Головоломка скрыта до входа в триггер
        if (puzzleContainer != null)
            puzzleContainer.SetActive(false);

        // Находим все пазлы и слоты (они есть, но скрыты)
        allPieces = FindObjectsOfType<MovePuzzle>();
        allSlots = FindObjectsOfType<PuzzleSlot>();

        Debug.Log("Головоломка готова. Ожидаем входа в триггер...");
    }

    // Вход в триггер
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag(playerTag) && !isPuzzleActive)
        {
            ActivatePuzzle();
        }
    }

    void ActivatePuzzle()
    {
        isPuzzleActive = true;
        Debug.Log("🎮 Игрок вошёл в триггер! Включаем головоломку!");

        // Показываем головоломку
        if (puzzleContainer != null)
        {
            puzzleContainer.SetActive(true);
        }

        // Сбрасываем прогресс
        piecesInPlace = 0;

        // Сбрасываем все пазлы
        foreach (MovePuzzle piece in allPieces)
        {
            piece.ResetPiece();
        }

        // Очищаем все слоты
        foreach (PuzzleSlot slot in allSlots)
        {
            slot.RemovePiece();
        }

        // Прячем крестик
        if (winButton != null)
            winButton.SetActive(false);
    }

    // Этот метод вызывает каждый пазл, когда его поставили в слот
    public void CheckPiecePlaced(int pieceIndex, int slotIndex)
    {
        if (!isPuzzleActive) return;

        if (pieceIndex == slotIndex)
        {
            piecesInPlace++;
            Debug.Log($"✅ Пазл {pieceIndex} на своём месте! Всего на месте: {piecesInPlace}/{totalPieces}");
        }

        if (piecesInPlace == totalPieces)
        {
            Win();
        }
    }

    // Этот метод вызывает каждый пазл, когда его убрали со слота
    public void CheckPieceRemoved(int pieceIndex, int slotIndex)
    {
        if (!isPuzzleActive) return;

        if (pieceIndex == slotIndex)
        {
            piecesInPlace--;
            Debug.Log($"❌ Пазл {pieceIndex} убрали с места. Всего на месте: {piecesInPlace}/{totalPieces}");
        }
    }

    void Win()
    {
        Debug.Log("🏆 ПОБЕДА! Все 25 пазлов на своих местах! 🏆");

        if (winButton != null)
        {
            winButton.SetActive(true);
            Debug.Log("✨ Крестик появился! ✨");
        }
    }

    // Вызывается при нажатии на крестик
    public void ClosePuzzle()
    {
        Debug.Log("❌ Закрываем головоломку по нажатию на крестик");

        if (puzzleContainer != null)
        {
            puzzleContainer.SetActive(false);
        }

        if (winButton != null)
        {
            winButton.SetActive(false);
        }

        isPuzzleActive = false;
    }
}