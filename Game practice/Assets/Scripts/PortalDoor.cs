using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

// Наследуемся от вашего старого Door (вместо MonoBehaviour)
public class PortalDoor : Door
{
    [Header("Настройки появления (PortalDoor)")]
    public string questCompleteID = "d11";          // Какой диалог должен завершиться
    public float appearDuration = 1f;               // Длительность появления
    public AnimationCurve appearCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("Эффекты появления")]
    public AudioClip appearSound;                   // Звук появления
    public ParticleSystem appearParticles;          // Частицы при появлении

    private bool hasAppeared = false;
    private SpriteRenderer portalSprite;
    private SpriteRenderer[] allSprites;

    void Start()
    {
        // Находим компоненты для анимации появления
        portalSprite = GetComponent<SpriteRenderer>();
        allSprites = GetComponentsInChildren<SpriteRenderer>(true);

        // Изначально портал прозрачный
        SetPortalAlpha(0f);

        // Отключаем коллайдер (через компонент Door он неактивен)
        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
            col.enabled = false;

        // Проверяем, не пройден ли уже квест
        CheckIfAlreadyUnlocked();
    }

    void Update()
    {
        // Проверяем, не пора ли активировать портал
        if (!hasAppeared && GameManager.Instance != null)
        {
            int currentStep = GameManager.Instance.currentStepIndex;
            int requiredStep = GetStepIndex(questCompleteID);

            if (currentStep > requiredStep)
            {
                ActivatePortal();
            }
        }
    }

    void CheckIfAlreadyUnlocked()
    {
        if (GameManager.Instance != null)
        {
            int currentStep = GameManager.Instance.currentStepIndex;
            int requiredStep = GetStepIndex(questCompleteID);

            if (currentStep > requiredStep)
            {
                // Уже пройдено - портал активен сразу
                hasAppeared = true;
                SetPortalAlpha(1f);

                Collider2D col = GetComponent<Collider2D>();
                if (col != null)
                    col.enabled = true;
            }
        }
    }

    void ActivatePortal()
    {
        if (hasAppeared) return;
        hasAppeared = true;

        Debug.Log($"✨ Портал '{gameObject.name}' появился после {questCompleteID}!");

        if (appearSound != null)
            AudioSource.PlayClipAtPoint(appearSound, transform.position);

        if (appearParticles != null)
            appearParticles.Play();

        StartCoroutine(AppearAnimation());
    }

    IEnumerator AppearAnimation()
    {
        float elapsed = 0f;

        while (elapsed < appearDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / appearDuration;
            float alpha = appearCurve.Evaluate(t);
            SetPortalAlpha(alpha);
            yield return null;
        }

        SetPortalAlpha(1f);

        // Включаем коллайдер после анимации
        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
            col.enabled = true;

        Debug.Log("✅ Портал полностью появился!");
    }

    void SetPortalAlpha(float alpha)
    {
        if (portalSprite != null)
        {
            Color color = portalSprite.color;
            color.a = alpha;
            portalSprite.color = color;
        }

        foreach (SpriteRenderer sr in allSprites)
        {
            if (sr != portalSprite)
            {
                Color color = sr.color;
                color.a = alpha;
                sr.color = color;
            }
        }
    }

    int GetStepIndex(string triggerID)
    {
        if (GameManager.Instance != null && GameManager.Instance.storySequence != null)
        {
            for (int i = 0; i < GameManager.Instance.storySequence.Count; i++)
            {
                if (GameManager.Instance.storySequence[i] == triggerID)
                    return i;
            }
        }
        return -1;
    }
}