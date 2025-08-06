using Terresquall;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] float moveSpeed = 3f;
    [SerializeField] float gravity = -9.81f;
    private int joystickID = -1;
    private bool isActive = false;
    private MapBoundary mapBoundary;
    private CharacterController characterController;
    private Vector3 playerVelocity;
    private bool isGrounded;

    void Start()
    {
        characterController = GetComponent<CharacterController>();
        if (characterController == null)
        {
            characterController = gameObject.AddComponent<CharacterController>();
            characterController.center = new Vector3(0, 0.5f, 0);
            characterController.height = 1.8f;
            characterController.radius = 0.3f;
        }
        characterController.enabled = true;
    }

    public void SetJoystickID(int id)
    {
        joystickID = id;
        isActive = true;
        Debug.Log("Joystick ID set to: " + id);
    }

    public void SetMapBoundary(MapBoundary boundary)
    {
        mapBoundary = boundary;
    }

    void Update()
    {
        if (!isActive || joystickID < 0)
        {
            Debug.Log($"Player controller inactive: isActive={isActive}, joystickID={joystickID}");
            return;
        }

        if (characterController == null || !characterController.enabled)
        {
            Debug.Log("CharacterController not available");
            return;
        }

        if (GameManager.Instance != null && GameManager.Instance.currentGameMode == GameManager.GameMode.PlatformGame)
        {
            HandleMovement();
            ApplyGravity();
        }
        else
        {
            Debug.Log("Not in platform game mode");
        }
    }

    private void HandleMovement()
    {
        Vector2 input = VirtualJoystick.GetAxis(joystickID);
       // Debug.Log("Joystick Input: " + input);

        Vector3 movement = new Vector3(input.x, 0, input.y);

        if (Camera.main != null)
        {
            Vector3 cameraForward = Camera.main.transform.forward;
            cameraForward.y = 0;
            cameraForward.Normalize();

            Vector3 cameraRight = Camera.main.transform.right;
            cameraRight.y = 0;
            cameraRight.Normalize();

            movement = cameraForward * input.y + cameraRight * input.x;
        }

        if (movement.magnitude > 0.01f)
        {
            movement.Normalize();
            characterController.Move(movement * moveSpeed * Time.deltaTime);

            Quaternion targetRotation = Quaternion.LookRotation(movement);
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRotation,
                10f * Time.deltaTime
            );
        }
    }

    private void ApplyGravity()
    {
        if (characterController == null || !characterController.enabled) return;

        float rayLength = characterController.height / 2 + 0.2f;
        isGrounded = Physics.Raycast(
            transform.position,
            Vector3.down,
            rayLength
        );

        if (isGrounded && playerVelocity.y < 0)
        {
            playerVelocity.y = -2f;
        }
        else
        {
            playerVelocity.y += gravity * Time.deltaTime;
        }

        characterController.Move(playerVelocity * Time.deltaTime);
    }

    public void ResetPlayer()
    {
        isActive = false;
        joystickID = -1;
        mapBoundary = null;
        playerVelocity = Vector3.zero;

        if (characterController != null)
        {
            characterController.enabled = false;
        }
    }
}