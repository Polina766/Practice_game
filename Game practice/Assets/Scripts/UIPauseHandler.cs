using UnityEngine;

public class UIAnimationOnPause : MonoBehaviour
{
    private Animator animator;
    private float lastUnscaledTime;

    void Start()
    {
        animator = GetComponent<Animator>();
        lastUnscaledTime = Time.unscaledTime;
    }

    void Update()
    {
        if (animator != null && PauseManager.isPaused)
        {
            // Ручное обновление анимации на паузе
            float currentTime = Time.unscaledTime;
            float delta = currentTime - lastUnscaledTime;
            lastUnscaledTime = currentTime;

            animator.Update(delta); // ← Заставляем аниматор работать даже на паузе
        }
        else if (animator != null)
        {
            lastUnscaledTime = Time.unscaledTime;
        }
    }
}