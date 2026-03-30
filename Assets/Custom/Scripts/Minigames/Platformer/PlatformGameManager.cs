using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlatformGameManager : MonoBehaviour
{
    [Header("Platform Game Settings")]
    public GameObject mapPrefab;
    public GameObject playerPrefab;
    public GameObject bananaPrefab;
    public int joystickID = 1;
    public float placementDistance = 1.0f;
    public float placementHeight = -0.5f;
    public Vector3 mapRotation = Vector3.zero;
    public Vector2 mapBounds = new Vector2(4f, 4f);
    public Vector3 playerSpawnOffset = new Vector3(0, 0.5f, 0);
    public int totalBananas = 3;

    private GameObject platformMap;
    private GameObject platformPlayer;
    private bool platformGameActive = false;
    private int bananasCollected = 0;

    private void OnEnable()
    {
        EventManager.OnPlatformGameStarted += StartPlatformGame;
        EventManager.OnGameCancelled += CancelPlatformGame;
        EventManager.OnGameResetAndExit += CancelPlatformGame;
        EventManager.OnBananaCollected += CollectBanana;
    }

    private void OnDisable()
    {
        EventManager.OnPlatformGameStarted -= StartPlatformGame;
        EventManager.OnGameCancelled -= CancelPlatformGame;
        EventManager.OnGameResetAndExit -= CancelPlatformGame;
        EventManager.OnBananaCollected -= CollectBanana;
    }

    private void StartPlatformGame()
    {
        platformGameActive = true;
        bananasCollected = 0;
        
        if (totalBananas <= 0) totalBananas = 3;

        PlacePlatformGame();
        
        EventManager.OnRemainingBananasUpdate?.Invoke(totalBananas);
    }

    private void CancelPlatformGame()
    {
        if (!platformGameActive) return;
        platformGameActive = false;

        if (platformPlayer != null)
        {
            PlayerController playerController = platformPlayer.GetComponent<PlayerController>();
            if (playerController != null)
            {
                playerController.ResetPlayer();
            }
            Destroy(platformPlayer);
            platformPlayer = null;
        }

        if (platformMap != null)
        {
            Destroy(platformMap);
            platformMap = null;
        }

        bananasCollected = 0;

        // Try to handle virtual joystick
        var joystickMethods = FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None);
        foreach(var mono in joystickMethods)
        {
            if (mono.GetType().Name == "VirtualJoystickFade")
            {
                var method = mono.GetType().GetMethod("SetVisibility");
                if (method != null)
                {
                    method.Invoke(mono, new object[] { false });
                }
            }
        }
    }

    private void PlacePlatformGame()
    {
        Camera arCamera = Camera.main;
        if (arCamera == null)
        {
            Debug.LogError("Main camera not found");
            return;
        }

        Vector3 cameraPosition = arCamera.transform.position;
        Vector3 cameraForward = Vector3.ProjectOnPlane(arCamera.transform.forward, Vector3.up).normalized;
        Vector3 placementPosition = cameraPosition + cameraForward * placementDistance;
        placementPosition.y = cameraPosition.y + placementHeight;

        if (mapPrefab != null)
        {
            platformMap = Instantiate(mapPrefab, placementPosition, Quaternion.Euler(mapRotation));
            MapBoundary boundary = platformMap.GetComponent<MapBoundary>();
            if (boundary == null)
            {
                boundary = platformMap.AddComponent<MapBoundary>();
            }
            boundary.SetBounds(mapBounds);
        }
        else
        {
            Debug.LogError("Map prefab is not assigned");
        }

        if (playerPrefab != null && platformMap != null)
        {
            Vector3 playerPosition = platformMap.transform.position + playerSpawnOffset;
            platformPlayer = Instantiate(playerPrefab, playerPosition, Quaternion.identity);

            PlayerController playerController = platformPlayer.GetComponent<PlayerController>();
            if (playerController == null)
            {
                playerController = platformPlayer.AddComponent<PlayerController>();
            }

            CharacterController characterController = platformPlayer.GetComponent<CharacterController>();
            if (characterController == null)
            {
                characterController = platformPlayer.AddComponent<CharacterController>();
                characterController.center = new Vector3(0, 0.5f, 0);
                characterController.height = 1.8f;
            }
            characterController.enabled = true;

            Rigidbody rb = platformPlayer.GetComponent<Rigidbody>();
            if (rb != null)
            {
                Destroy(rb);
            }

            playerController.SetJoystickID(joystickID);
            playerController.SetMapBoundary(platformMap.GetComponent<MapBoundary>());
            playerController.enabled = true;
        }
        else
        {
            Debug.LogError("Player prefab or map is not assigned");
        }

        // Try to handle virtual joystick
        var joystickMethods = FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None);
        foreach(var mono in joystickMethods)
        {
            if (mono.GetType().Name == "VirtualJoystickFade")
            {
                var method = mono.GetType().GetMethod("SetVisibility");
                if (method != null)
                {
                    method.Invoke(mono, new object[] { true });
                }
            }
        }
    }

    private void CollectBanana(int value)
    {
        if (bananasCollected < 0) bananasCollected = 0;
        if (totalBananas <= 0) totalBananas = 3;

        if (bananasCollected >= totalBananas) return;

        bananasCollected++;
        EventManager.OnRemainingBananasUpdate?.Invoke(totalBananas - bananasCollected);

        if (bananasCollected >= totalBananas && totalBananas > 0)
        {
            EventManager.OnAllBananasCollected?.Invoke();
        }
    }
}
