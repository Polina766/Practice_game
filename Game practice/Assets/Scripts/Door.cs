using UnityEngine;
using UnityEngine.SceneManagement;

public class Door : MonoBehaviour
{
    [Header("Какая сцена загрузится")]
    public string sceneToLoad = "SceneName";

    [Header("ID этой двери (например: ToKitchen, ToHall)")]
    public string doorID = "Door1";

    [Header("Настройки курсора")]
    public Texture2D doorCursorLeft;
    public Texture2D doorCursorRight;
    public Vector2 cursorHotspot = new Vector2(16, 16);

    [Header("Направление стрелки")]
    public bool arrowPointLeft = true;

    private bool playerInRange = false;
    private PlayerController playerController;
    private Texture2D defaultCursor;
    private CursorMode cursorMode = CursorMode.Auto;
    private bool isLoading = false;

    void Start()
    {
        defaultCursor = null;

        if (GetComponent<Collider2D>() == null)
        {
            BoxCollider2D col = gameObject.AddComponent<BoxCollider2D>();
            col.isTrigger = false;
        }
    }

    void OnMouseEnter()
    {
        if (arrowPointLeft && doorCursorLeft != null)
        {
            Cursor.SetCursor(doorCursorLeft, cursorHotspot, cursorMode);
        }
        else if (!arrowPointLeft && doorCursorRight != null)
        {
            Cursor.SetCursor(doorCursorRight, cursorHotspot, cursorMode);
        }
    }

    void OnMouseExit()
    {
        Cursor.SetCursor(defaultCursor, Vector2.zero, cursorMode);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
            playerController = other.GetComponent<PlayerController>();
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            playerController = null;
        }
    }

    void Update()
    {
        if (playerInRange && Input.GetKeyDown(KeyCode.E) && !isLoading)
        {
            LoadScene();
        }
    }

    void OnMouseDown()
    {
        if (isLoading) return;

        if (playerController == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                playerController = player.GetComponent<PlayerController>();
            }
        }

        if (playerController != null)
        {
            playerController.CancelMoveIndicator();

            if (playerInRange)
            {
                LoadScene();
            }
            else
            {
                playerController.MoveToDoor(this);
            }
        }
    }

    void LoadScene()
    {
        if (isLoading) return;
        isLoading = true;

        // СОХРАНЯЕМ ПОЗИЦИЮ ТЕКУЩЕЙ ДВЕРИ
        string currentScene = SceneManager.GetActiveScene().name;
        string saveKey = currentScene + "_" + doorID;

        SceneTransfer.doorPositions[saveKey] = transform.position;

        Debug.Log($"Сохранено: {saveKey} = {transform.position}");

        Cursor.SetCursor(defaultCursor, Vector2.zero, cursorMode);
        SceneManager.LoadScene(sceneToLoad);
    }

    // ТОЛЬКО ЭТОТ МЕТОД ДОБАВЛЕН (ничего больше не менялось!)
    public void OnPlayerArrived()
    {
        if (!isLoading && playerInRange)
        {
            LoadScene();
        }
    }

    void OnDestroy()
    {
        Cursor.SetCursor(defaultCursor, Vector2.zero, cursorMode);
    }
}