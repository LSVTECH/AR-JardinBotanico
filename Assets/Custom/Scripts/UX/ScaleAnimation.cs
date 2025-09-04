using UnityEngine;
using System.Collections;

public class ScaleAnimation : MonoBehaviour
{
    [SerializeField] private float animationSpeed = 2f;
    [SerializeField] private bool animateOnEnable = true;

    private Vector3 targetScale;
    private Coroutine scaleCoroutine;

    private void OnEnable()
    {
        if (animateOnEnable)
        {
            StartScaleAnimation();
        }
    }

    public void StartScaleAnimation()
    {
        if (scaleCoroutine != null)
        {
            StopCoroutine(scaleCoroutine);
        }
        scaleCoroutine = StartCoroutine(ScaleRoutine());
    }

    private IEnumerator ScaleRoutine()
    {
        transform.localScale = Vector3.zero;
        targetScale = Vector3.one;

        float progress = 0f;

        while (progress < 1f)
        {
            progress += Time.deltaTime * animationSpeed;
            transform.localScale = Vector3.Lerp(Vector3.zero, targetScale, progress);
            yield return null;
        }

        transform.localScale = targetScale;
    }

    public void SetAnimationSpeed(float speed)
    {
        animationSpeed = Mathf.Max(0.1f, speed);
    }
}