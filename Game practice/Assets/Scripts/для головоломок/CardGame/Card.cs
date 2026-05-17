using UnityEngine;
using UnityEngine.UI;

public class Card : MonoBehaviour
{
    public int cardID; // Unique ID for this card
    public GameManagerCard gameManager; // Reference to the game manager
    private bool isFLipped;
    private Image cardImage;

    void Start()
    {
        cardImage = GetComponent<Image>();
        isFLipped = false;
        cardImage.sprite = GameManagerCard.Instance.cardBack;
    }
    public void FlipCard()
    {
        if (!isFLipped && (gameManager.firstCard == null || gameManager.secondCard == null))
        {
            isFLipped=true;
            cardImage.sprite = gameManager.cardFaces[cardID];
            gameManager.CardFlipped(this);
        }
    }

    public void HideCard()
    {
        isFLipped = false;
        cardImage.sprite = gameManager.cardBack;
    }
}
