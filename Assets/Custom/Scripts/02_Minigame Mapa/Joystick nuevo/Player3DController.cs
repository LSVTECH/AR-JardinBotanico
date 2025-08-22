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
        Vector3 movementDirection = joystick.GetMovementDirection();
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
            animator.SetBool("IsMoving", isMoving);

            if (isMoving && joystick != null)
            {
                animator.SetFloat("MoveSpeed", joystick.GetMovementIntensity());
            }
            else
            {
                animator.SetFloat("MoveSpeed", 0);
            }
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
    }
}