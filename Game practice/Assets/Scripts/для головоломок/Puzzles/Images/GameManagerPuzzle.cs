using UnityEngine;

public class GameManagerPuzzle : MonoBehaviour
{
    [Header("UI")]
    public GameObject winButton; // Крестик (кнопка)
    public GameObject puzzleContainer; // Контейнер со всей головоломкой

    [Header("Игрок")]
    public PlayerController playerController; // Перетащите сюда PlayerController
    public Animator playerAnimator; // Перетащите сюда Animator игрока

    [Header("Quest Trigger (опционально)")]
    public QuestStepTrigger questTrigger; // Перетащите сюда QuestStepTrigger для уведомления

    [Header("Настройки")]
    public string playerTag = "Player";

    private int totalPieces = 25;
    private int piecesInPlace = 0;
    private bool isPuzzleActive = false;
    private MovePuzzle[] allPieces;
    private PuzzleSlot[] allSlots;
    private bool puzzleCompleted = false;

    void Start()
    {
        if (winButton != null)
            winButton.SetActive(false);

        if (puzzleContainer != null)
            puzzleContainer.SetActive(false);

        allPieces = FindObjectsOfType<MovePuzzle>();
        allSlots = FindObjectsOfType<PuzzleSlot>();

        
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag(playerTag) && !isPuzzleActive && !puzzleCompleted)
        {
            ActivatePuzzle();
        }
    }

    void ActivatePuzzle()
    {
        isPuzzleActive = true;
        

        // 🔒 БЛОКИРУЕМ ИГРОКА
        LockPlayer();

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

    // Блокировка игрока
    void LockPlayer()
    {
        if (playerController != null)
        {
            // Убираем стрелочку
            playerController.CancelMoveIndicator();

            // Останавливаем всё движение
            playerController.StopAllMovement();

            // Отключаем управление
            playerController.enabled = false;

            
        }

        if (playerAnimator != null)
        {
            playerAnimator.SetFloat("Speed", 0f);
            playerAnimator.Play("Idle");
        }
    }

    // Разблокировка игрока
    void UnlockPlayer()
    {
        if (playerController != null)
        {
            playerController.enabled = true;
            
        }
    }

    public void CheckPiecePlaced(int pieceIndex, int slotIndex)
    {
        if (!isPuzzleActive) return;

        if (pieceIndex == slotIndex)
        {
            piecesInPlace++;
            
        }

        if (piecesInPlace == totalPieces)
        {
            Win();
        }
    }

    public void CheckPieceRemoved(int pieceIndex, int slotIndex)
    {
        if (!isPuzzleActive) return;

        if (pieceIndex == slotIndex)
        {
            piecesInPlace--;
            
        }
    }

    void Win()
    {
        
        puzzleCompleted = true;

        // Уведомляем GameManager (если есть QuestTrigger)
        if (questTrigger != null)
        {
            questTrigger.NotifyManually();
            
        }

        if (winButton != null)
        {
            winButton.SetActive(true);
            
        }
    }

    // Вызывается при нажатии на крестик
    public void ClosePuzzle()
    {
        

        if (puzzleContainer != null)
        {
            puzzleContainer.SetActive(false);
        }

        if (winButton != null)
        {
            winButton.SetActive(false);
        }

        isPuzzleActive = false;

        // 🔓 РАЗБЛОКИРУЕМ ИГРОКА
        UnlockPlayer();
    }
}