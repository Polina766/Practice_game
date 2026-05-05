using UnityEngine;
using UnityEngine.EventSystems;

public class ButtonHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    [Header("Анимация")]
    public Vector3 normalScale = Vector3.one;
    public Vector3 hoverScale = new Vector3(1.1f, 1.1f, 1.1f);
    public float speed = 10f;

    [Header("Звуки (перетащите аудиофайлы)")]
    public AudioClip clickSound;
    public AudioClip hoverSound;

    private Vector3 targetScale;
    private bool isRealClick = false;

    void Awake()
    {
        targetScale = normalScale;
    }

    void Update()
    {
        transform.localScale = Vector3.Lerp(transform.localScale, targetScale, Time.unscaledDeltaTime * speed);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        targetScale = hoverScale;

        if (hoverSound != null && AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX(hoverSound, 0.7f);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        targetScale = normalScale;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        // Игнорируем первый реальный клик, но запоминаем что был клик
        if (!isRealClick)
        {
            isRealClick = true;
            return;
        }

        if (eventData == null || eventData.button != PointerEventData.InputButton.Left) return;

        if (PauseManager.isPaused) return;

        if (clickSound != null && AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX(clickSound);
        }
    }

    void OnDisable()
    {
        transform.localScale = normalScale;
        targetScale = normalScale;
    }
}