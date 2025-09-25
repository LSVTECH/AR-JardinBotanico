using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class AlbumEntrance : MonoBehaviour
{
    [Header("Refs")]
    public CanvasGroup canvasGroup;
    public RectTransform panel;

    [Header("Duraciones")]
    public float slideTime = 0.40f;
    public float fadeTime  = 0.30f;
    public float popTime   = 0.35f;

    [Header("Offsets")]
    public float startYOffset = -200f;

    [Header("Curvas")]
    public AnimationCurve slideCurve = AnimationCurve.EaseInOut(0,0,1,1);
    public AnimationCurve popCurve   = new AnimationCurve(
        new Keyframe(0f, 0.9f),
        new Keyframe(0.7f, 1.05f),
        new Keyframe(1f, 1f)
    );

    Vector2 targetPos;

    void Awake()
    {
        if (!canvasGroup) canvasGroup = GetComponent<CanvasGroup>();
        if (!panel) panel = GetComponent<RectTransform>();

        targetPos = panel.anchoredPosition;
        panel.anchoredPosition = targetPos + new Vector2(0, startYOffset);
        canvasGroup.alpha = 0f;
        panel.localScale = Vector3.one * 0.9f;
    }

    void OnEnable()
    {
        StopAllCoroutines();
        StartCoroutine(PlayEntrance());
    }

    IEnumerator PlayEntrance()
    {
        // Slide
        float t = 0f;
        Vector2 startPos = panel.anchoredPosition;
        while (t < slideTime)
        {
            t += Time.deltaTime;
            float k = slideCurve.Evaluate(Mathf.Clamp01(t / slideTime));
            panel.anchoredPosition = Vector2.LerpUnclamped(startPos, targetPos, k);
            yield return null;
        }
        panel.anchoredPosition = targetPos;

        // Fade
        t = 0f;
        while (t < fadeTime)
        {
            t += Time.deltaTime;
            canvasGroup.alpha = Mathf.Clamp01(t / fadeTime);
            yield return null;
        }
        canvasGroup.alpha = 1f;

        // Pop
        t = 0f;
        while (t < popTime)
        {
            t += Time.deltaTime;
            float k = popCurve.Evaluate(Mathf.Clamp01(t / popTime));
            panel.localScale = Vector3.one * k;
            yield return null;
        }
        panel.localScale = Vector3.one;
    }
}
