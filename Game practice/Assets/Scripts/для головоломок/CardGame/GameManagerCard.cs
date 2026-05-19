using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameManagerCard : MonoBehaviour
{
    public static GameManagerCard Instance;

    public Card cardPrefab;
    public Sprite cardBack;
    public Sprite[] cardFaces;

    private List<Card> cards;
    private List<int> cardIDs;
    public Card firstCard, secondCard;

    public Transform cardHolder;
    public TextMeshProUGUI timerText;

    private int pairsMatched;
    private int totalPairs;
    private float timer;
    private bool isGameOver;
    private bool isLevelFinished;

    public float maxTime = 60f;

    public Button closeButton;
    public GameObject losePanel;
    public string sceneToReturnTo = "KitchenScene";

    private void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        cards = new List<Card>();
        cardIDs = new List<int>();
        pairsMatched = 0;
        totalPairs = cardFaces.Length / 2;

        timer = maxTime;
        isGameOver = false;
        isLevelFinished = false;

        CreateCards();
        ShuffleCards();

        if (timerText != null)
            timerText.text = "Time Left: " + Mathf.Round(timer) + "s";

        if (losePanel != null)
            losePanel.SetActive(false);

        if (closeButton != null)
        {
            closeButton.onClick.AddListener(ClosePuzzle);
            closeButton.gameObject.SetActive(false);
        }

        if (losePanel != null)
        {
            Button restartButton = losePanel.GetComponentInChildren<Button>();
            if (restartButton != null)
                restartButton.onClick.AddListener(RestartGame);
        }
    }

    void Update()
    {
        if (!isGameOver && !isLevelFinished)
        {
            if (timer > 0)
            {
                timer -= Time.deltaTime;
                if (timerText != null)
                    timerText.text = "Time Left: " + Mathf.Round(timer) + "s";
            }
            else
            {
                GameOver();
            }
        }
    }

    void CreateCards()
    {
        for (int i = 0; i < cardFaces.Length / 2; i++)
        {
            cardIDs.Add(i);
            cardIDs.Add(i);
        }

        foreach (int id in cardIDs)
        {
            Card newCard = Instantiate(cardPrefab, cardHolder);
            newCard.gameManager = this;
            newCard.cardID = id;
            cards.Add(newCard);
        }
    }

    void ShuffleCards()
    {
        for (int i = 0; i < cardIDs.Count; i++)
        {
            int randomIndex = Random.Range(i, cardIDs.Count);
            int temp = cardIDs[i];
            cardIDs[i] = cardIDs[randomIndex];
            cardIDs[randomIndex] = temp;
        }

        for (int i = 0; i < cards.Count; i++)
        {
            cards[i].cardID = cardIDs[i];
        }
    }

    public void CardFlipped(Card flippedCard)
    {
        if (firstCard == null)
        {
            firstCard = flippedCard;
        }
        else if (secondCard == null)
        {
            secondCard = flippedCard;
            CheckMatch();
        }
    }

    void CheckMatch()
    {
        if (firstCard.cardID == secondCard.cardID)
        {
            pairsMatched++;

            if (pairsMatched == totalPairs)
            {
                LevelFinished();
            }

            firstCard = null;
            secondCard = null;
        }
        else
        {
            StartCoroutine(FlipBackCards());
        }
    }

    IEnumerator FlipBackCards()
    {
        yield return new WaitForSeconds(1f);
        firstCard.HideCard();
        secondCard.HideCard();
        firstCard = null;
        secondCard = null;
    }

    void GameOver()
    {
        isGameOver = true;
        if (losePanel != null)
            losePanel.SetActive(true);
    }

    public void LevelFinished()
    {
        isLevelFinished = true;

        // ОТМЕЧАЕМ, ЧТО ГОЛОВОЛОМКА ПРОЙДЕНА
        PuzzleTransition.CompletePuzzle();

        if (closeButton != null)
            closeButton.gameObject.SetActive(true);
    }

    public void RestartGame()
    {
        pairsMatched = 0;
        timer = maxTime;
        isGameOver = false;
        isLevelFinished = false;

        if (losePanel != null)
            losePanel.SetActive(false);
        if (closeButton != null)
            closeButton.gameObject.SetActive(false);

        if (timerText != null)
            timerText.text = "Time Left: " + Mathf.Round(timer) + "s";

        foreach (var card in cards)
        {
            Destroy(card.gameObject);
        }
        cards.Clear();
        cardIDs.Clear();

        CreateCards();
        ShuffleCards();
    }

    public void ClosePuzzle()
    {
        SceneManager.LoadScene(sceneToReturnTo);
    }
}