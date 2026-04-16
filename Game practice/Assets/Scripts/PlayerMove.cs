using UnityEngine;
using UnityEngine.EventSystems;

public class PlayerController : MonoBehaviour
{
    public string CurrentAnimation = "Idle Player";

    public bool OnGround = false;
    public float MoveSpeed = 5f;

    [Header("Настройки пола")]
    public float floorY = -1.5f;

    [Header("Стрелочка")]
    public MoveIndicator moveIndicator;

    [Header("Настройки препятствий")]
    public LayerMask obstacleLayer;

    private Rigidbody2D PlayerRigidbody2D;
    private SpriteRenderer PlayerSpriteRenderer;
    private Animator Player;

    private Vector2 clickTarget;
    private bool isMovingToClick = false;

    private Vector2 lastPosition;
    private float stuckTime = 0f;

    void Start()
    {
        // Проверяем существует ли ControlManager, если нет - создаём
        if (ControlManager.Instance == null)
        {
            GameObject go = new GameObject("ControlManager");
            go.AddComponent<ControlManager>();
        }

        PlayerRigidbody2D = GetComponent<Rigidbody2D>();
        PlayerSpriteRenderer = GetComponent<SpriteRenderer>();
        Player = GetComponent<Animator>();

        PlayerRigidbody2D.gravityScale = 1;
        clickTarget = transform.position;
        lastPosition = transform.position;

        if (gameObject.tag != "Player")
        {
            gameObject.tag = "Player";
        }

        if (obstacleLayer == 0)
        {
            obstacleLayer = LayerMask.GetMask("Default");
        }
    }

    void Update()
    {
        // ЕСЛИ ИГРА НА ПАУЗЕ - НИЧЕГО НЕ ДЕЛАЕМ
        if (PauseManager.isPaused)
        {
            return;
        }

        float moveX = 0f;

        // ============ ЧИТАЕМ НАЖАТИЯ КЛАВИШ (ВСЕГДА) ============
        if (Input.GetKey("d") || Input.GetKey(KeyCode.RightArrow))
            moveX = 1f;
        else if (Input.GetKey("a") || Input.GetKey(KeyCode.LeftArrow))
            moveX = -1f;

        // ============ ДВИЖЕНИЕ ПО КЛАВИШАМ (только если выбран режим клавиатуры) ============
        if (!ControlManager.Instance.useMouseMovement && moveX != 0)
        {
            if (isMovingToClick)
            {
                isMovingToClick = false;
                if (moveIndicator != null)
                    moveIndicator.Hide();
            }

            Vector2 moveVector = new Vector2(moveX * MoveSpeed, PlayerRigidbody2D.linearVelocity.y);
            PlayerRigidbody2D.linearVelocity = moveVector;
            RotatePlayer(moveX < 0);

            if (OnGround)
                Player.SetBool("isWalking", true);

            stuckTime = 0f;
            lastPosition = transform.position;
        }

        // ============ ДВИЖЕНИЕ ПО КЛИКУ МЫШИ (только если выбран режим мыши) ============
        if (ControlManager.Instance.useMouseMovement && Input.GetMouseButtonDown(0))
        {
            // Проверяем клик по UI (кнопки, головоломки и т.д.)
            if (IsPointerOverUI())
            {
                return;
            }

            Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mouseWorldPos.z = 0;

            Vector2 targetPosition = new Vector2(mouseWorldPos.x, floorY);

            if (CanReachTarget(targetPosition))
            {
                clickTarget = targetPosition;
                isMovingToClick = true;
                stuckTime = 0f;
                lastPosition = transform.position;

                if (moveIndicator != null)
                    moveIndicator.ShowAtPosition(clickTarget);

                Debug.Log("✅ Стрелочка поставлена! Цель: " + clickTarget);
            }
            else
            {
                Debug.Log("❌ Нельзя поставить стрелочку! На пути стена.");

                
            }
        }

        // ============ ДВИЖЕНИЕ К ТОЧКЕ КЛИКА (только для режима мыши) ============
        if (ControlManager.Instance.useMouseMovement && isMovingToClick && moveX == 0)
        {
            float direction = 0f;

            if (clickTarget.x > transform.position.x)
                direction = 1f;
            else if (clickTarget.x < transform.position.x)
                direction = -1f;

            if (direction != 0)
            {
                Vector2 moveVector = new Vector2(direction * MoveSpeed, PlayerRigidbody2D.linearVelocity.y);
                PlayerRigidbody2D.linearVelocity = moveVector;
                RotatePlayer(direction < 0);
            }

            if (OnGround)
                Player.SetBool("isWalking", true);

            // Проверка на застревание
            float distanceMoved = Vector2.Distance(transform.position, lastPosition);

            if (distanceMoved < 0.02f)
            {
                stuckTime += Time.deltaTime;
                if (stuckTime > 0.5f)
                {
                    Debug.Log("⚠️ Персонаж застрял! Стрелочка исчезает.");
                    StopMovingToClick();
                    return;
                }
            }
            else
            {
                stuckTime = 0f;
            }

            lastPosition = transform.position;

            // Проверка достижения цели
            if (Mathf.Abs(transform.position.x - clickTarget.x) < 0.1f)
            {
                Debug.Log("✅ Дошли до цели!");
                StopMovingToClick();
            }
        }

        // ============ ОСТАНОВКА АНИМАЦИИ ============
        if (moveX == 0 && !isMovingToClick)
        {
            AnimationStop();
            // Не обнуляем скорость в режиме мыши, чтобы не мешать физике
            if (!ControlManager.Instance.useMouseMovement)
            {
                PlayerRigidbody2D.linearVelocity = new Vector2(0, PlayerRigidbody2D.linearVelocity.y);
            }
        }

        // ============ ОТМЕНА ДВИЖЕНИЯ ДЛЯ МЫШИ ============
        if (ControlManager.Instance.useMouseMovement && Input.GetKeyDown(KeyCode.Escape))
        {
            StopMovingToClick();
        }
    }

    bool IsPointerOverUI()
    {
        // Проверка клика по UI
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
        {
            return true;
        }

        // Опционально: проверка клика по объектам головоломок (раскомментируй если нужно)
        /*
        Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        RaycastHit2D hit = Physics2D.Raycast(mousePos, Vector2.zero);
        
        if (hit.collider != null && hit.collider.CompareTag("Puzzle"))
        {
            return true; // Не двигаемся, если кликнули по головоломке
        }
        */

        return false;
    }

    bool CanReachTarget(Vector2 targetPosition)
    {
        Vector2 currentPos = transform.position;
        Vector2 direction = targetPosition - currentPos;
        float distance = direction.magnitude;

        if (distance < 0.1f)
            return true;

        direction.Normalize();

        RaycastHit2D hit = Physics2D.Raycast(currentPos, direction, distance, obstacleLayer);

        if (hit.collider != null && hit.collider.gameObject != gameObject)
        {
            if (!hit.collider.CompareTag("Floor"))
            {
                Debug.Log("Препятствие на пути: " + hit.collider.gameObject.name);
                return false;
            }
        }

        return true;
    }

    void RotatePlayer(bool Bool_Value)
    {
        PlayerSpriteRenderer.flipX = Bool_Value;
    }

    void AnimationStop()
    {
        if (OnGround == true)
        {
            Player.SetBool("isWalking", false);
        }
    }

    void StopMovingToClick()
    {
        isMovingToClick = false;
        if (OnGround)
            Player.SetBool("isWalking", false);
        PlayerRigidbody2D.linearVelocity = new Vector2(0, PlayerRigidbody2D.linearVelocity.y);

        if (moveIndicator != null)
            moveIndicator.Hide();
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Floor"))
        {
            OnGround = true;
        }
    }

    void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Floor"))
        {
            OnGround = false;
            Player.SetBool("isWalking", false);
        }
    }

    public void StopAllMovement()
    {
        // Останавливаем движение к стрелочке
        if (isMovingToClick)
        {
            StopMovingToClick();
        }

        // Останавливаем физическое движение
        if (PlayerRigidbody2D != null)
        {
            PlayerRigidbody2D.linearVelocity = new Vector2(0, PlayerRigidbody2D.linearVelocity.y);
        }

        // Останавливаем анимацию ходьбы
        if (OnGround)
        {
            Player.SetBool("isWalking", false);
        }

        // Прячем стрелочку
        if (moveIndicator != null)
        {
            moveIndicator.Hide();
        }

        // Сбрасываем флаги
        isMovingToClick = false;
        stuckTime = 0f;

        Debug.Log("Движение остановлено при смене управления");
    }
}