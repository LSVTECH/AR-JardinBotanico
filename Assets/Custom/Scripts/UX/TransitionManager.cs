using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class TransitionManager : MonoBehaviour
{
    [Header("Transition Settings")]
    public float fadeDuration = 0.5f;
    public float scaleDuration = 0.7f;
    public AnimationCurve fadeCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    public AnimationCurve scaleCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    public enum TransitionType
    {
        Fade,
        Scale,
        Slide,
        Combined
    }

    // Transición básica de fundido (Fade)
    public IEnumerator FadeTransition(CanvasGroup group, float targetAlpha, bool setActiveAfter = false)
    {
        if (group == null) yield break;

        float startAlpha = group.alpha;
        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float t = fadeCurve.Evaluate(elapsed / fadeDuration);
            group.alpha = Mathf.Lerp(startAlpha, targetAlpha, t);
            yield return null;
        }

        group.alpha = targetAlpha;

        if (setActiveAfter)
        {
            group.gameObject.SetActive(targetAlpha > 0);
        }
    }

    // Transición de escala
    public IEnumerator ScaleTransition(Transform target, Vector3 targetScale, bool setActiveAfter = false)
    {
        if (target == null) yield break;

        Vector3 startScale = target.localScale;
        float elapsed = 0f;

        if (targetScale != Vector3.zero && !target.gameObject.activeSelf)
        {
            target.gameObject.SetActive(true);
        }

        while (elapsed < scaleDuration)
        {
            elapsed += Time.deltaTime;
            float t = scaleCurve.Evaluate(elapsed / scaleDuration);
            target.localScale = Vector3.Lerp(startScale, targetScale, t);
            yield return null;
        }

        target.localScale = targetScale;

        if (setActiveAfter)
        {
            target.gameObject.SetActive(targetScale != Vector3.zero);
        }
    }

    // Transición combinada (fade + scale)
    public IEnumerator CombinedTransition(CanvasGroup canvasGroup, Transform targetTransform,
                                        float targetAlpha, Vector3 targetScale, bool setActiveAfter = false)
    {
        if (canvasGroup == null || targetTransform == null) yield break;

        float startAlpha = canvasGroup.alpha;
        Vector3 startScale = targetTransform.localScale;
        float elapsed = 0f;

        if ((targetAlpha > 0 || targetScale != Vector3.zero) && !targetTransform.gameObject.activeSelf)
        {
            targetTransform.gameObject.SetActive(true);
        }

        while (elapsed < Mathf.Max(fadeDuration, scaleDuration))
        {
            elapsed += Time.deltaTime;

            // Aplicar fade
            if (elapsed < fadeDuration)
            {
                float t = fadeCurve.Evaluate(elapsed / fadeDuration);
                canvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, t);
            }

            // Aplicar escala
            if (elapsed < scaleDuration)
            {
                float t = scaleCurve.Evaluate(elapsed / scaleDuration);
                targetTransform.localScale = Vector3.Lerp(startScale, targetScale, t);
            }

            yield return null;
        }

        canvasGroup.alpha = targetAlpha;
        targetTransform.localScale = targetScale;

        if (setActiveAfter)
        {
            targetTransform.gameObject.SetActive(targetAlpha > 0 || targetScale != Vector3.zero);
        }
    }

    // Transición de deslizamiento (slide)
    public IEnumerator SlideTransition(RectTransform rectTransform, Vector2 targetPosition,
                                     float duration, bool setActiveAfter = false)
    {
        if (rectTransform == null) yield break;

        Vector2 startPosition = rectTransform.anchoredPosition;
        float elapsed = 0f;

        if (!rectTransform.gameObject.activeSelf)
        {
            rectTransform.gameObject.SetActive(true);
        }

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = fadeCurve.Evaluate(elapsed / duration);
            rectTransform.anchoredPosition = Vector2.Lerp(startPosition, targetPosition, t);
            yield return null;
        }

        rectTransform.anchoredPosition = targetPosition;
    }
}