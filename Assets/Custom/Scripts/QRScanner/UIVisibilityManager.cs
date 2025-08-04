using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasGroup))]
public class UIVisibilityManager : MonoBehaviour
{
    public float fadeDuration = 0.3f;
    private CanvasGroup canvasGroup;
    private float targetAlpha = 0f;
    private float currentAlpha = 0f;

    void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        canvasGroup.alpha = 0f;
        gameObject.SetActive(false);
    }

    public void SetVisible(bool visible, bool immediate = false)
    {
        if (visible)
        {
            gameObject.SetActive(true);
            targetAlpha = 1f;
        }
        else
        {
            targetAlpha = 0f;
        }

        if (immediate)
        {
            canvasGroup.alpha = targetAlpha;
            if (targetAlpha == 0f) gameObject.SetActive(false);
        }
    }

    void Update()
    {
        if (Mathf.Approximately(currentAlpha, targetAlpha)) return;

        currentAlpha = Mathf.MoveTowards(currentAlpha, targetAlpha,
                                        Time.deltaTime / fadeDuration);
        canvasGroup.alpha = currentAlpha;

        if (targetAlpha == 0f && currentAlpha < 0.01f)
        {
            gameObject.SetActive(false);
        }
    }
}