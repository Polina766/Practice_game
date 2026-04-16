using UnityEngine;

public class SimpleDoor : MonoBehaviour
{
    public Sprite closedSprite;
    public Sprite openedSprite;

    private SpriteRenderer sr;
    private bool isOpen = false;

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        if (closedSprite != null)
            sr.sprite = closedSprite;

        Debug.Log("Дверь создана! Проверьте:");
        Debug.Log("- SpriteRenderer есть? " + (sr != null));
        Debug.Log("- Collider2D есть? " + (GetComponent<Collider2D>() != null));
        Debug.Log("- Тег игрока: " + GameObject.FindGameObjectWithTag("Player")?.name);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log("Кто-то вошел в триггер: " + other.gameObject.name + " с тегом: " + other.tag);

        if (other.CompareTag("Player"))
        {
            Debug.Log("ИГРОК ВОШЕЛ! Открываем дверь");
            if (!isOpen && openedSprite != null)
            {
                sr.sprite = openedSprite;
                isOpen = true;
            }
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        Debug.Log("Кто-то вышел из триггера: " + other.gameObject.name);

        if (other.CompareTag("Player"))
        {
            Debug.Log("ИГРОК ВЫШЕЛ! Закрываем дверь");
            if (isOpen && closedSprite != null)
            {
                sr.sprite = closedSprite;
                isOpen = false;
            }
        }
    }
}