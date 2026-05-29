using UnityEngine;
using UnityEngine.EventSystems;


public class PlayerController : MonoBehaviour
{
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

    // Движение к двери
    private Door targetDoor;
    private bool isMovingToDoor = false;

    [Header("Движение к двери")]
    public float doorMoveSpeed = 10f; // скорость к двери

    void Start()
    {
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
        // Если игра на паузе - ничего не делаем
        if (PauseManager.isPaused)
        {
            return;
        }

        // Движение к двери (приоритет)
        if (isMovingToDoor && targetDoor != null)
        {
            MoveToDoorTarget();
            return;
        }

        // Обработка клика мыши
        if (Input.GetMouseButtonDown(0))
        {
            // Проверяем клик по UI
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
            
        }

        // Движение к точке клика
        if (isMovingToClick)
        {
            MoveToClickTarget();
        }
        else if (!isMovingToDoor) // Если никуда не двигаемся - останавливаем анимацию
        {
            AnimationStop();
        }

        // Отмена движения по ESC
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            StopMovingToClick();
        }
    }

    void MoveToClickTarget()
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

    void MoveToDoorTarget()
    {
        Vector2 doorPos = targetDoor.transform.position;

        // ВРЕМЕННО УВЕЛИЧИВАЕМ СКОРОСТЬ ПРЯМО ЗДЕСЬ
        float currentSpeed = MoveSpeed;

        // Двигаемся ТОЛЬКО по горизонтали
        float step = currentSpeed * Time.deltaTime;
        float newX = Mathf.MoveTowards(transform.position.x, doorPos.x, step);

        // МЕНЯЕМ ПОЗИЦИЮ ЧЕРЕЗ velocity, НЕ MovePosition
        Vector2 newVelocity = new Vector2(
            (newX - transform.position.x) / Time.deltaTime,
            PlayerRigidbody2D.linearVelocity.y
        );
        PlayerRigidbody2D.linearVelocity = newVelocity;

        // Поворот в сторону двери
        if (doorPos.x < transform.position.x)
            RotatePlayer(true);
        else if (doorPos.x > transform.position.x)
            RotatePlayer(false);

        // Анимация
        if (OnGround)
        {
            Player.SetBool("isWalking", true);
        }

        // Проверка достижения двери
        if (Mathf.Abs(transform.position.x - doorPos.x) < 0.2f)
        {
            isMovingToDoor = false;

            if (OnGround)
            {
                Player.SetBool("isWalking", false);
            }

            PlayerRigidbody2D.linearVelocity = new Vector2(0, PlayerRigidbody2D.linearVelocity.y);

            if (targetDoor != null)
            {
                targetDoor.OnPlayerArrived();
            }
            targetDoor = null;
        }

        
    }
    bool IsPointerOverUI()
    {
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
        {
            return true;
        }

        // ЭТОТ БЛОК ДОЛЖЕН БЫТЬ!
        Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        RaycastHit2D hit = Physics2D.Raycast(mousePos, Vector2.zero);

        if (hit.collider != null && hit.collider.GetComponent<Door>() != null)
        {
            return true; // Не ставим стрелочку при клике на дверь
        }

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

    // Методы для двери
    public void MoveToDoor(Door door)
    {
        // Останавливаем текущее движение
        if (isMovingToClick)
        {
            StopMovingToClick();
        }

        // Останавливаем движение
        PlayerRigidbody2D.linearVelocity = new Vector2(0, PlayerRigidbody2D.linearVelocity.y);

        // Прячем стрелочку
        if (moveIndicator != null)
        {
            moveIndicator.Hide();
        }

        // Начинаем движение к двери
        targetDoor = door;
        isMovingToDoor = true;
        Debug.Log("Идём к двери...");
    }

    public void StopMovingToDoor()
    {
        isMovingToDoor = false;
        targetDoor = null;
        if (OnGround)
            Player.SetBool("isWalking", false);
        PlayerRigidbody2D.linearVelocity = new Vector2(0, PlayerRigidbody2D.linearVelocity.y);
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

        // Останавливаем движение к двери
        if (isMovingToDoor)
        {
            StopMovingToDoor();
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

        
    }

    public void CancelMoveIndicator()
    {
        if (isMovingToClick)
        {
            StopMovingToClick();
        }
        if (moveIndicator != null)
        {
            moveIndicator.Hide();
        }
        isMovingToClick = false;
        stuckTime = 0f;

        
    }
}