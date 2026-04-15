using UnityEngine;

public class MoveIndicator : MonoBehaviour
{
    [Header("Настройки анимации")]
    public float pulseSpeed = 3f;
    public float minScale = 0.7f;
    public float maxScale = 1.3f;

    private SpriteRenderer spriteRenderer;
    private bool isActive = false;
    private float time = 0f;
    private Vector3 originalScale; // ← ЭТО ОБЪЯВЛЕНИЕ ПЕРЕМЕННОЙ

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        originalScale = transform.localScale; // ← ЭТО ИНИЦИАЛИЗАЦИЯ

        if (spriteRenderer != null)
            spriteRenderer.enabled = true;

        gameObject.SetActive(false);
    }

    void Update()
    {
        if (isActive)
        {
            time += Time.deltaTime * pulseSpeed;
            float scale = Mathf.Lerp(minScale, maxScale, (Mathf.Sin(time) + 1f) / 2f);
            transform.localScale = originalScale * scale;
        }
    }

    public void ShowAtPosition(Vector2 position)
    {
        

        isActive = false;
        transform.position = new Vector3(position.x, position.y + 0.3f, 0);
        transform.localScale = originalScale;
        gameObject.SetActive(true);
        isActive = true;
        time = 0f;

        // Убедимся, что цвет правильный (если спрайт есть)
        if (spriteRenderer != null)
        {
            spriteRenderer.color = Color.white;
        }

        
    }

    public void Hide()
    {
        
        isActive = false;
        gameObject.SetActive(false);
    }

    // Метод для показа красной стрелочки (когда нельзя поставить)
    public void ShowRedAtPosition(Vector2 position)
    {
        transform.position = new Vector3(position.x, position.y + 0.3f, 0);
        transform.localScale = originalScale;

        if (spriteRenderer != null)
        {
            spriteRenderer.color = Color.red;
        }

        gameObject.SetActive(true);
        isActive = true;
        time = 0f;

        // Через 0.3 секунды скрыть
        CancelInvoke();
        Invoke("Hide", 0.3f);
    }
}