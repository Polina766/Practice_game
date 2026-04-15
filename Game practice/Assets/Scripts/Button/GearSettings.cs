using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections;

public class GearSettings : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
    public Sprite normalSprite;
    public Sprite pressedSprite;
    public float hoverScale = 1.1f;
    public float pressScale = 0.9f;
    public float animDuration = 0.1f;

    private Image imageComponent;
    private RectTransform rectTransform;
    private Vector3 originalScale;
    private Vector2 originalPosition;

    void Start()
    {
        imageComponent = GetComponent<Image>();
        rectTransform = GetComponent<RectTransform>();

        Vector2 oldPivot = rectTransform.pivot;
        rectTransform.pivot = new Vector2(0.5f, 0.5f);

        rectTransform.anchoredPosition += new Vector2(
            (0.5f - oldPivot.x) * rectTransform.rect.width,
            (0.5f - oldPivot.y) * rectTransform.rect.height
        );

        originalPosition = rectTransform.anchoredPosition;
        originalScale = rectTransform.localScale;

        if (normalSprite != null)
            imageComponent.sprite = normalSprite;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        StopAllCoroutines();
        StartCoroutine(AnimateScaleAndPosition(originalScale * hoverScale));
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        StopAllCoroutines();
        StartCoroutine(AnimateScaleAndPosition(originalScale));
        if (normalSprite != null) imageComponent.sprite = normalSprite;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        StopAllCoroutines();
        StartCoroutine(AnimateScaleAndPosition(originalScale * pressScale));
        if (pressedSprite != null) imageComponent.sprite = pressedSprite;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        StopAllCoroutines();
        StartCoroutine(AnimateScaleAndPosition(originalScale));
        if (normalSprite != null) imageComponent.sprite = normalSprite;
    }

    private IEnumerator AnimateScaleAndPosition(Vector3 targetScale)
    {
        Vector3 startScale = rectTransform.localScale;
        float elapsed = 0f;

        while (elapsed < animDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / animDuration;
            rectTransform.localScale = Vector3.Lerp(startScale, targetScale, t);
            rectTransform.anchoredPosition = originalPosition;
            yield return null;
        }

        rectTransform.localScale = targetScale;
        rectTransform.anchoredPosition = originalPosition;
    }
}