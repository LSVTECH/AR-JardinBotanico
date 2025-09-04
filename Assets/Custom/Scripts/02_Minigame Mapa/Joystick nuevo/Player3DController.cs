using PinePie.SimpleJoystick;
using UnityEngine;

public class Player3DController : MonoBehaviour
{
    [Header("Joystick Reference")]
    public JoystickController joystick;

    [Header("Movement Settings")]
    public float movementSpeed = 5f;
    public float rotationSpeed = 10f;

    private CharacterController characterController;
    private Animator animator;
    private bool isMoving = false;
    private float currentSpeed;
    private float speedVelocity;
    private Vector3 movementDirection;

    void Start()
    {
        characterController = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();

        // Buscar el joystick si no está asignado
        if (joystick == null)
        {
            joystick = FindObjectOfType<JoystickController>();

            // Configurar el joystick para que no aplique movimiento automático
            if (joystick != null)
            {
                joystick.enable3DMovement = false;
                joystick.playerTransform = this.transform;
            }
        }
    }

    void Update()
    {
        HandleMovement();
        HandleAnimation();
    }

    void HandleMovement()
    {
        if (joystick == null) return;

        // Obtener dirección del joystick
        movementDirection = joystick.GetMovementDirection();
        float movementIntensity = joystick.GetMovementIntensity();

        isMoving = movementIntensity > 0.1f;

        if (isMoving)
        {
            // Mover el personaje
            Vector3 movement = movementDirection * movementSpeed * movementIntensity * Time.deltaTime;

            if (characterController != null && characterController.enabled)
            {
                characterController.Move(movement);
            }
            else
            {
                transform.Translate(movement, Space.World);
            }

            // Rotar el personaje hacia la dirección del movimiento
            if (movementDirection != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(movementDirection);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
            }
        }
    }

    void HandleAnimation()
    {
        if (animator != null)
        {
            // Calcular la velocidad actual basada en la intensidad del movimiento
            float targetSpeed = isMoving ? joystick.GetMovementIntensity() : 0f;

            // Suavizar la transición de velocidad
            currentSpeed = Mathf.SmoothDamp(currentSpeed, targetSpeed, ref speedVelocity, 0.1f);

            // Actualizar parámetros del Animator
            animator.SetFloat("Speed", currentSpeed);
            animator.SetBool("Moving", isMoving);

            // Para movimiento en X específicamente, puedes agregar un parámetro adicional si es necesario
            animator.SetFloat("MoveX", movementDirection.x);
        }
    }

    // Método para habilitar/deshabilitar el control del jugador
    public void SetControllerActive(bool active)
    {
        this.enabled = active;
        if (characterController != null)
        {
            characterController.enabled = active;
        }

        // Resetear animaciones cuando se desactiva el control
        if (!active && animator != null)
        {
            animator.SetFloat("Speed", 0);
            animator.SetBool("Moving", false);
            animator.SetFloat("MoveX", 0);
        }
    }
}