using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    [Header("Game Elements")]
    [Range(2, 6)]
    [SerializeField] private int difficulty = 4;
    [SerializeField] private Transform gameHolder;
    [SerializeField] private GameObject piecePrefab;

    [Header("UI Elements")]
    [SerializeField] private List<Texture2D> imageTextures;
    [SerializeField] private Transform levelSelectPanel;
    [SerializeField] private Image levelSelectPrefab;

    private List<Transform> pieces;
    private Vector2Int dimensions;
    private float width;
    private float height;

    void Start()
    {
        // Create the UI
        foreach (Texture2D texture in imageTextures)
        {
            Image image = Instantiate(levelSelectPrefab, levelSelectPanel);
            image.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.zero);
            // Assign button action
            image.GetComponent<Button>().onClick.AddListener(delegate { StartGame(texture); });
        }
    }

    public void StartGame(Texture2D jigsawTexture)
    {
        // Hide the UI
        levelSelectPanel.gameObject.SetActive(false);

        // We store a list of the transform for each jigsaw piece so we can track them later.
        pieces = new List<Transform>();

        // Calculate the size of each jigsaw piece, based on a difficulty setting.
        dimensions = GetDimensions(jigsawTexture, difficulty);

        // Create the pieces of the correct size with the correct texture
        CreateJigsawPieces(jigsawTexture);
    }

    Vector2Int GetDimensions(Texture2D jigsawTexture, int difficulty)
    {
        Vector2Int dimensions = Vector2Int.zero;
        // Difficulty is the number of pieces on the smallest texture dimension.
        // This helps ensure the pieces are as square as possible.
        if (jigsawTexture.width < jigsawTexture.height)
        {
            dimensions.x = difficulty;
            dimensions.y = (difficulty * jigsawTexture.height) / jigsawTexture.width;
        }
        else
        {
            dimensions.x = (difficulty * jigsawTexture.width) / jigsawTexture.height;
            dimensions.y = difficulty;
        }
        return dimensions;
    }

    // Create all the jigsaw pieces
    void CreateJigsawPieces(Texture2D jigsawTexture)
    {
        // Calculate piece sizes based on the dimensions.
        height = 1f / dimensions.y;
        float aspect = (float)jigsawTexture.width / jigsawTexture.height;
        width = aspect / dimensions.x;

        for (int row = 0; row < dimensions.y; row++)
        {
            for (int col = 0; col < dimensions.x; col++)
            {
                // Create the piece in the right location of the right size.
                GameObject piece = Instantiate(piecePrefab, gameHolder);
                piece.transform.localPosition = new Vector3(
                  (-width * dimensions.x / 2) + (width * col) + (width / 2),
                  (-height * dimensions.y / 2) + (height * row) + (height / 2),
                  -1);
                piece.transform.localScale = new Vector3(width, height, 1f);

                // We don't have to name them, but always useful for debugging.
                piece.name = $"Piece {(row * dimensions.x) + col}";
                pieces.Add(piece.transform);

                float pieceWidth = (float)jigsawTexture.width / dimensions.x;
                float pieceHeight = (float)jigsawTexture.height / dimensions.y;

                Rect rect = new Rect(
                    col * pieceWidth,
                    row * pieceHeight,
                    pieceWidth,
                    pieceHeight
                );

                Sprite sprite = Sprite.Create(
                    jigsawTexture,
                    rect,
                    new Vector2(0.5f, 0.5f),
                    100f
                );

                piece.GetComponent<SpriteRenderer>().sprite = sprite;
            }
        }
    }
}
