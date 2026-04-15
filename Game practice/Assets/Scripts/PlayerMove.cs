using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public string CurrentAnimation = "Idle Player";

    public bool OnGround = false;
    public float MoveSpeed = 5f;

    private Rigidbody2D PlayerRigidbody2D;
    private SpriteRenderer PlayerSpriteRenderer;
    private Animator Player;

    // Для движения по клику
    private Vector2 clickTarget;
    private bool isMovingToClick = false;

    void Start()
    {
        PlayerRigidbody2D = GetComponent<Rigidbody2D>();
        PlayerSpriteRenderer = GetComponent<SpriteRenderer>();
        Player = GetComponent<Animator>();

        PlayerRigidbody2D.gravityScale = 1;
        clickTarget = transform.position;
    }

    void Update()
    {
        // ============ ДВИЖЕНИЕ ПО КЛАВИШАМ ============
        float moveX = 0f;

        // WASD + Стрелочки (только влево-вправо)
        if (Input.GetKey("d") || Input.GetKey(KeyCode.RightArrow))
            moveX = 1f;
        else if (Input.GetKey("a") || Input.GetKey(KeyCode.LeftArrow))
            moveX = -1f;

        // Если нажата клавиша движения
        if (moveX != 0)
        {
            // Отменяем движение по клику
            isMovingToClick = false;

            // Движение через velocity (быстрое)
            Vector2 moveVector = new Vector2(moveX * MoveSpeed, PlayerRigidbody2D.linearVelocity.y);
            PlayerRigidbody2D.linearVelocity = moveVector;

            // Поворот спрайта
            RotatePlayer(moveX < 0);

            // Включаем анимацию
            if (OnGround)
                Player.SetBool("isWalking", true);
        }

        // ============ ДВИЖЕНИЕ ПО КЛИКУ МЫШИ ============
        // Проверяем клик левой кнопкой мыши
        if (Input.GetMouseButtonDown(0))
        {
            // Получаем позицию мыши в мире
            Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mouseWorldPos.z = 0;
            clickTarget = mouseWorldPos;
            isMovingToClick = true;
        }

        // Движение к точке клика (только если нет движения с клавиатуры)
        if (isMovingToClick && moveX == 0)
        {
            // Используем такой же метод движения, как и для клавиш
            float direction = 0f;

            if (clickTarget.x > transform.position.x)
                direction = 1f;
            else if (clickTarget.x < transform.position.x)
                direction = -1f;

            // Применяем скорость (как в движении по клавишам)
            Vector2 moveVector = new Vector2(direction * MoveSpeed, PlayerRigidbody2D.linearVelocity.y);
            PlayerRigidbody2D.linearVelocity = moveVector;

            // Поворачиваем персонажа
            if (direction != 0)
                RotatePlayer(direction < 0);

            // Включаем анимацию
            if (OnGround)
                Player.SetBool("isWalking", true);

            // Проверяем, дошли ли до цели
            if (Mathf.Abs(transform.position.x - clickTarget.x) < 0.1f)
            {
                StopMovingToClick();
            }
        }

        // ============ ОСТАНОВКА АНИМАЦИИ ============
        // Если нет никакого движения
        if (moveX == 0 && !isMovingToClick)
        {
            AnimationStop();
            PlayerRigidbody2D.linearVelocity = new Vector2(0, PlayerRigidbody2D.linearVelocity.y);
        }

        // ============ ОТМЕНА ДВИЖЕНИЯ ПО КЛИКУ ============
        // Правая кнопка мыши
        if (Input.GetMouseButtonDown(1))
        {
            StopMovingToClick();
        }

        // Пробел
        if (Input.GetKeyDown(KeyCode.Space))
        {
            StopMovingToClick();
        }

        // Escape
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            StopMovingToClick();
        }
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

    // Метод для остановки движения по клику
    void StopMovingToClick()
    {
        isMovingToClick = false;
        if (OnGround)
            Player.SetBool("isWalking", false);
        // Останавливаем физическое движение
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
}