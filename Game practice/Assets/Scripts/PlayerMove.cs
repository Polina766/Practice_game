using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private Vector2 VectorToRight = new Vector2(1, 0);
    private Vector2 VectorToLeft = new Vector2(-1, 0);
    public string CurrentAnimation = "Idle Player";

    public bool OnGround = false;
    public float MoveSpeed = 5f;

    private Rigidbody2D PlayerRigidbody2D;
    private SpriteRenderer PlayerSpriteRenderer;
    private Animator Player;

    void Start()
    {
        PlayerRigidbody2D = GetComponent<Rigidbody2D>();
        PlayerSpriteRenderer = GetComponent<SpriteRenderer>();
        Player = GetComponent<Animator>();

        PlayerRigidbody2D.gravityScale = 1;
    }

    void Update()
    {
        if (Input.GetKey("d"))
        {
            PlayerMoving(VectorToRight);
            RotatePlayer(false);
        }
        else if (Input.GetKey("a"))
        {
            PlayerMoving(VectorToLeft);
            RotatePlayer(true);
        }
        else
        {
            AnimationStop();
            PlayerRigidbody2D.linearVelocity = new Vector2(0, PlayerRigidbody2D.linearVelocity.y);
        }
    }

    void PlayerMoving(Vector2 MoveVector)
    {
        Vector2 NewMoveVector = new Vector2(MoveVector.x * MoveSpeed, PlayerRigidbody2D.linearVelocity.y);
        PlayerRigidbody2D.linearVelocity = NewMoveVector;

        if (OnGround == true)
        {
            // Устанавливаем параметр isWalking в true
            Player.SetBool("isWalking", true);
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
            // Устанавливаем параметр isWalking в false
            Player.SetBool("isWalking", false);
        }
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
            // Когда не на полу - останавливаем анимацию ходьбы
            Player.SetBool("isWalking", false);
        }
    }
}