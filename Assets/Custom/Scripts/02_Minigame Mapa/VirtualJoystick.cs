using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class VirtualJoystick : MonoBehaviour, IDragHandler, IPointerUpHandler, IPointerDownHandler
{
    [SerializeField] private RectTransform joystickBackground;
    [SerializeField] private RectTransform joystickHandle;
    [SerializeField] private float joystickRadius = 100f;
    [SerializeField] private bool alwaysVisible = false;

    public int joystickID = 1;
    public bool isActive = false;

    private Vector2 inputAxis = Vector2.zero;
    private Vector2 joystickInitialPosition;
    private Canvas parentCanvas;
    private GraphicRaycaster raycaster;

    void Start()
    {
        // Guardar la posición inicial
        joystickInitialPosition = joystickBackground.anchoredPosition;

        // Obtener referencias necesarias
        parentCanvas = GetComponentInParent<Canvas>();
        raycaster = parentCanvas.GetComponent<GraphicRaycaster>();

        // Configurar visibilidad inicial
        if (!alwaysVisible)
        {
            joystickBackground.gameObject.SetActive(false);
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!isActive) return;

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

    public void OnPointerDown(PointerEventData eventData)
    {
        // Solo procesar si no es un clic de UI
        if (EventSystem.current.IsPointerOverGameObject(eventData.pointerId))
            return;

        isActive = true;

        // Posicionar el joystick donde se tocó
        if (!alwaysVisible)
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                parentCanvas.transform as RectTransform,
                eventData.position,
                eventData.pressEventCamera,
                out Vector2 localPoint);

            joystickBackground.anchoredPosition = localPoint;
            joystickBackground.gameObject.SetActive(true);
        }

        OnDrag(eventData);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        isActive = false;
        inputAxis = Vector2.zero;
        joystickHandle.anchoredPosition = Vector2.zero;

        if (!alwaysVisible)
        {
            joystickBackground.gameObject.SetActive(false);
        }
    }

    public Vector2 GetInputAxis()
    {
        return inputAxis;
    }

    public static Vector2 GetAxis(int joystickID)
    {
        VirtualJoystick[] joysticks = FindObjectsByType<VirtualJoystick>(FindObjectsSortMode.None);
        foreach (VirtualJoystick joystick in joysticks)
        {
            if (joystick.joystickID == joystickID && joystick.isActive)
            {
                return joystick.GetInputAxis();
            }
        }
        return Vector2.zero;
    }
}
