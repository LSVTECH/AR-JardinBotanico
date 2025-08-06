using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class VirtualJoystick : MonoBehaviour, IDragHandler, IPointerUpHandler, IPointerDownHandler
{
    [SerializeField] private RectTransform joystickBackground;
    [SerializeField] private RectTransform joystickHandle;
    [SerializeField] private float joystickRadius = 100f;

    public int joystickID = 1;
    public bool isActive = false;

    private Vector2 inputAxis = Vector2.zero;

    public void OnDrag(PointerEventData eventData)
    {
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
        isActive = true;
        OnDrag(eventData);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        isActive = false;
        inputAxis = Vector2.zero;
        joystickHandle.anchoredPosition = Vector2.zero;
    }

    public Vector2 GetInputAxis()
    {
        return inputAxis;
    }

    // Método estático para obtener el input por ID
    public static Vector2 GetAxis(int joystickID)
    {
        VirtualJoystick[] joysticks = FindObjectsOfType<VirtualJoystick>();
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