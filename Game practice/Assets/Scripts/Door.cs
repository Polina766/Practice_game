using UnityEngine;
using UnityEngine.SceneManagement;

public class Door : MonoBehaviour
{
    [Header("Какая сцена загрузится")]
    public string sceneToLoad = "SceneName";

    [Header("Настройки курсора")]
    public Texture2D doorCursor;
    public Vector2 cursorHotspot = new Vector2(16, 16);

    private bool playerInRange = false;
    private PlayerController playerController;
    private Texture2D defaultCursor;
    private CursorMode cursorMode = CursorMode.Auto;
    private bool isLoading = false;

    void Start()
    {
        defaultCursor = null;
    }

    void OnMouseEnter()
    {
        if (doorCursor != null)
        {
            Cursor.SetCursor(doorCursor, cursorHotspot, cursorMode);
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
            Debug.Log("Игрок вошёл в зону двери");
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            playerController = null;
            Debug.Log("Игрок вышел из зоны двери");
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
        Debug.Log("Клик по двери!");

        if (isLoading) return;

        if (playerController != null)
        {
            if (playerInRange)
            {
                LoadScene();
            }
            else
            {
                playerController.MoveToDoor(this);
            }
        }
        else
        {
            // Если игрок далеко - всё равно идём к двери
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                playerController = player.GetComponent<PlayerController>();
                if (playerController != null)
                {
                    playerController.MoveToDoor(this);
                }
            }
        }
    }

    public void OnPlayerArrived()
    {
        if (!isLoading)
        {
            Debug.Log("Игрок подошёл к двери!");
            LoadScene();
        }
    }

    void LoadScene()
    {
        if (isLoading) return;
        isLoading = true;

        Cursor.SetCursor(defaultCursor, Vector2.zero, cursorMode);
        SceneManager.LoadScene(sceneToLoad);
    }

    void OnDestroy()
    {
        Cursor.SetCursor(defaultCursor, Vector2.zero, cursorMode);
    }
}