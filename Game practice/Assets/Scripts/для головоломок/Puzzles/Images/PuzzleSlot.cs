using UnityEngine;

public class PuzzleSlot : MonoBehaviour
{
    public int slotIndex; // Индекс слота (0-24)
    public bool isOccupied = false;
    private MovePuzzle currentPiece;

    void Start()
    {
        // Добавляем коллайдер если нет
        if (GetComponent<Collider2D>() == null)
        {
            BoxCollider2D col = gameObject.AddComponent<BoxCollider2D>();
            col.isTrigger = false;
        }

        Debug.Log($"Слот {slotIndex} готов");
    }

    public void PlacePiece(MovePuzzle piece)
    {
        isOccupied = true;
        currentPiece = piece;
    }

    public void RemovePiece()
    {
        isOccupied = false;
        currentPiece = null;
    }
}