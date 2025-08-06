using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections;

[RequireComponent(typeof(CanvasGroup))]
public class VirtualJoystickFade : MonoBehaviour, IDragHandler, IPointerUpHandler, IPointerDownHandler
{
    [Header("Joystick Settings")]
    public RectTransform joystickBackground;
    public RectTransform joystickHandle;
    public float joystickRadius = 100f;
    public int joystickID = 1;
    public float fadeInDuration = 0.2f;
    public float fadeOutDuration = 0.5f;
    public float fadeOutDelay = 1f;

    private CanvasGroup canvasGroup;
    private Vector2 inputAxis = Vector2.zero;
    private bool isActive = false;
    private bool isDragging = false;
    private Coroutine fadeCoroutine;
    private Vector2 originalPosition;

    void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        canvasGroup.alpha = 0f; // Comienza invisible
        joystickHandle.anchoredPosition = Vector2.zero;

        // Guardar posición original para el fade out
        if (joystickBackground != null)
        {
            originalPosition = joystickBackground.anchoredPosition;
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        isActive = true;
        isDragging = false;

        // Posicionar el joystick donde se tocó la pantalla
        SetJoystickPosition(eventData.position);

        // Iniciar fade in
        if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);
        fadeCoroutine = StartCoroutine(FadeCanvasGroup(1f, fadeInDuration));

        // Procesar el toque inicial como arrastre
        OnDrag(eventData);
    }

    public void OnDrag(PointerEventData eventData)
    {
        isDragging = true;

        Vector2 position;
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
            joystickBackground,
            eventData.position,
            eventData.pressEventCamera,
            out position))
        {
            position = Vector2.ClampMagnitude(position, joystickRadius);
            joystickHandle.anchoredPosition = position;
            inputAxis = position / joystickRadius;
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        ResetJoystick();
    }

    public void ResetJoystick()
    {
        isActive = false;
        isDragging = false;
        inputAxis = Vector2.zero;
        joystickHandle.anchoredPosition = Vector2.zero;

        // Iniciar fade out después de un retraso
        if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);
        fadeCoroutine = StartCoroutine(FadeCanvasGroup(0f, fadeOutDuration, fadeOutDelay));
    }

    private void SetJoystickPosition(Vector2 screenPosition)
    {
        if (joystickBackground == null) return;

        Vector2 localPoint;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            transform.parent.GetComponent<RectTransform>(),
            screenPosition,
            null,
            out localPoint
        );
        joystickBackground.anchoredPosition = localPoint;
    }

    public Vector2 GetInputAxis()
    {
        return inputAxis;
    }

    public bool IsActive()
    {
        return isActive;
    }

    public static Vector2 GetAxis(int joystickID)
    {
        VirtualJoystickFade[] joysticks = FindObjectsOfType<VirtualJoystickFade>();
        foreach (VirtualJoystickFade joystick in joysticks)
        {
            if (joystick.joystickID == joystickID && joystick.isActive)
            {
                return joystick.GetInputAxis();
            }
        }
        return Vector2.zero;
    }

    private IEnumerator FadeCanvasGroup(float targetAlpha, float duration, float delay = 0f)
    {
        yield return new WaitForSeconds(delay);

        float startAlpha = canvasGroup.alpha;
        float time = 0f;

        while (time < duration)
        {
            canvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, time / duration);
            time += Time.deltaTime;
            yield return null;
        }

        canvasGroup.alpha = targetAlpha;

        // Restaurar posición original al completar fade out
        if (targetAlpha == 0f && joystickBackground != null)
        {
            joystickBackground.anchoredPosition = originalPosition;
        }
    }

    // Nuevo: Método para configurar la visibilidad manualmente
    public void SetVisibility(bool visible, bool immediate = false)
    {
        if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);

        if (immediate)
        {
            canvasGroup.alpha = visible ? 1f : 0f;
        }
        else
        {
            fadeCoroutine = StartCoroutine(FadeCanvasGroup(
                visible ? 1f : 0f,
                visible ? fadeInDuration : fadeOutDuration
            ));
        }
    }
}