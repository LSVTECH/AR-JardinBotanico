using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;
using Random = UnityEngine.Random;
using Terresquall;

public class GameManager : MonoBehaviour
{
    public ARCameraManager cameraManager;
    public static GameManager Instance;

    [Header("Game Settings")]
    public List<GameObject> objectPrefabs;
    public int objectsToSpawn = 5;
    public float spawnRadius = 2f;
    public float minDistanceBetweenObjects = 1.0f;
    public float defaultFloatSpeed = 0.5f;
    public float defaultFloatHeight = 0.1f;

    [Header("Platform Game Settings")]
    public GameObject mapPrefab;
    public GameObject playerPrefab;
    public int joystickID = 1;
    public float placementDistance = 1.0f;
    public float placementHeight = -0.5f;
    public Vector3 mapRotation = Vector3.zero;
    public Vector2 mapBounds = new Vector2(4f, 4f);
    public Vector3 playerSpawnOffset = new Vector3(0, 0.5f, 0);

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip objectFoundSound;

    [Header("UI Elements")]
    public Text scoreText;
    public Text timerText;
    public GameObject gameUI;
    public GameObject menuJardinBotanico;
    public Button backButton;
    public Button startPlatformGameButton;

    private List<GameObject> spawnedObjects = new List<GameObject>();
    private int currentScore = 0;
    private int totalObjectsFound = 0;
    private bool gameActive = false;
    private List<Vector3> spawnedPositions = new List<Vector3>();
    private float gameStartTime;
    private float gameTime;

    private GameObject platformMap;
    private GameObject platformPlayer;
    private bool platformGameActive = false;

    public enum GameMode
    {
        None,
        ObjectSearch,
        PlatformGame
    }
    public GameMode currentGameMode = GameMode.None;

    const string HIGH_SCORE_KEY = "HighScore";
    const string BEST_TIME_KEY = "BestTime";

    [Header("Result UI")]
    public GameObject resultsPanel;
    public Text finalScoreText;
    public Text highScoreText;
    public Text timeText;
    public Text bestTimeText;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        if (backButton != null)
        {
            backButton.onClick.AddListener(CancelCurrentGame);
        }

        if (startPlatformGameButton != null)
        {
            startPlatformGameButton.onClick.AddListener(StartPlatformGame);
        }

        InitializeAR();
    }

    private void InitializeAR()
    {
        if (cameraManager == null)
        {
            cameraManager = FindObjectOfType<ARCameraManager>();
        }

        if (cameraManager == null)
        {
            Debug.LogWarning("ARCameraManager not found. Creating one...");
            GameObject arSessionOrigin = new GameObject("AR Session Origin");
            arSessionOrigin.AddComponent<ARSessionOrigin>();
            GameObject arCamera = new GameObject("AR Camera");
            arCamera.transform.SetParent(arSessionOrigin.transform);
            arCamera.AddComponent<Camera>();
            cameraManager = arCamera.AddComponent<ARCameraManager>();
        }
    }

    public void StartGame()
    {
        CancelCurrentGame();
        currentGameMode = GameMode.ObjectSearch;
        currentScore = 0;
        totalObjectsFound = 0;
        gameActive = true;
        gameStartTime = Time.time;
        UpdateScoreUI();
        UpdateTimerUI();

        if (gameUI != null) gameUI.SetActive(true);
        if (menuJardinBotanico != null) menuJardinBotanico.SetActive(false);
        if (resultsPanel != null) resultsPanel.SetActive(false);

        ClearExistingObjects();
        SpawnObjects();
    }

    public void StartPlatformGame()
    {
        CancelCurrentGame();
        currentGameMode = GameMode.PlatformGame;
        platformGameActive = true;

        if (menuJardinBotanico != null) menuJardinBotanico.SetActive(false);
        if (gameUI != null) gameUI.SetActive(false);
        if (resultsPanel != null) resultsPanel.SetActive(false);

        PlacePlatformGame();
    }

    public void CancelCurrentGame()
    {
        switch (currentGameMode)
        {
            case GameMode.ObjectSearch:
                CancelObjectSearchGame();
                break;
            case GameMode.PlatformGame:
                CancelPlatformGame();
                break;
        }

        currentGameMode = GameMode.None;

        if (menuJardinBotanico != null) menuJardinBotanico.SetActive(true);
        if (gameUI != null) gameUI.SetActive(false);
        if (resultsPanel != null) resultsPanel.SetActive(false);
    }

    private void CancelObjectSearchGame()
    {
        if (!gameActive) return;
        gameActive = false;
        ClearExistingObjects();
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
        // Desactivar joystick
        VirtualJoystickFade joystick = FindObjectOfType<VirtualJoystickFade>();
        if (joystick != null)
        {
            joystick.SetVisibility(false);
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
        VirtualJoystickFade joystick = FindObjectOfType<VirtualJoystickFade>();
        if (joystick != null)
        {
            joystick.SetVisibility(true);
        }
    }

    Vector3 GetRandomPositionAroundDevice()
    {
        Vector3 center = Camera.main != null ? Camera.main.transform.position : Vector3.zero;
        center.y = 0;
        Vector3 randomPos = Vector3.zero;
        bool validPosition = false;
        int attempts = 0;
        const int maxAttempts = 50;

        while (!validPosition && attempts < maxAttempts)
        {
            Vector2 randomCircle = Random.insideUnitCircle * spawnRadius;
            randomPos = center + new Vector3(randomCircle.x, 0, randomCircle.y);

            RaycastHit hit;
            if (Physics.Raycast(randomPos + Vector3.up * 2f, Vector3.down, out hit, 3f))
            {
                randomPos = hit.point + Vector3.up * 0.1f;
            }
            else
            {
                randomPos.y = center.y;
            }

            validPosition = IsPositionValid(randomPos);
            attempts++;
        }

        return randomPos;
    }

    bool IsPositionValid(Vector3 position)
    {
        foreach (Vector3 existingPos in spawnedPositions)
        {
            if (Vector3.Distance(position, existingPos) < minDistanceBetweenObjects)
            {
                return false;
            }
        }
        return true;
    }

    void SpawnObjects()
    {
        spawnedPositions.Clear();

        for (int i = 0; i < objectsToSpawn; i++)
        {
            if (objectPrefabs == null || objectPrefabs.Count == 0)
            {
                Debug.LogError("No object prefabs assigned");
                return;
            }

            GameObject prefabToSpawn = objectPrefabs[Random.Range(0, objectPrefabs.Count)];
            Vector3 randomPos = GetRandomPositionAroundDevice();

            GameObject obj = Instantiate(prefabToSpawn, randomPos, prefabToSpawn.transform.rotation);
            spawnedObjects.Add(obj);
            spawnedPositions.Add(randomPos);

            FloatingObj floater = obj.GetComponent<FloatingObj>();
            if (floater == null)
            {
                floater = obj.AddComponent<FloatingObj>();
            }
            floater.floatSpeed = defaultFloatSpeed;
            floater.floatHeight = defaultFloatHeight;
        }
    }

    void ClearExistingObjects()
    {
        foreach (GameObject obj in spawnedObjects)
        {
            if (obj != null)
            {
                FloatingObj floater = obj.GetComponent<FloatingObj>();
                if (floater != null)
                {
                    floater.enabled = false;
                    Destroy(floater);
                }
                Destroy(obj);
            }
        }
        spawnedObjects.Clear();
        spawnedPositions.Clear();
    }

    public void AddScore(int points)
    {
        if (!gameActive || currentGameMode != GameMode.ObjectSearch) return;

        currentScore += points;
        totalObjectsFound++;
        UpdateScoreUI();

        if (audioSource != null && objectFoundSound != null)
        {
            audioSource.PlayOneShot(objectFoundSound);
        }

        if (totalObjectsFound >= objectsToSpawn)
        {
            EndGame();
        }
    }

    void UpdateScoreUI()
    {
        if (scoreText != null)
        {
            scoreText.text = $"Puntos: {currentScore}\nEncontrados: {totalObjectsFound}/{objectsToSpawn}";
        }
    }

    void UpdateTimerUI()
    {
        if (gameActive && currentGameMode == GameMode.ObjectSearch && timerText != null)
        {
            gameTime = Time.time - gameStartTime;
            timerText.text = FormatTime(gameTime);
            Invoke("UpdateTimerUI", 0.1f);
        }
    }

    string FormatTime(float timeInSeconds)
    {
        int minutes = Mathf.FloorToInt(timeInSeconds / 60f);
        int seconds = Mathf.FloorToInt(timeInSeconds % 60f);
        return string.Format("{0:00}:{1:00}", minutes, seconds);
    }

    public int HighScore
    {
        get => PlayerPrefs.GetInt(HIGH_SCORE_KEY, 0);
        set => PlayerPrefs.SetInt(HIGH_SCORE_KEY, value);
    }

    public float BestTime
    {
        get => PlayerPrefs.GetFloat(BEST_TIME_KEY, Mathf.Infinity);
        set => PlayerPrefs.SetFloat(BEST_TIME_KEY, value);
    }

    void CheckHighScore()
    {
        if (currentScore > HighScore)
        {
            HighScore = currentScore;
        }
    }

    void CheckBestTime()
    {
        if (gameTime < BestTime)
        {
            BestTime = gameTime;
        }
    }

    void EndGame()
    {
        gameActive = false;
        gameTime = Time.time - gameStartTime;
        CheckHighScore();
        CheckBestTime();
        ShowResults();

        if (gameUI != null) gameUI.SetActive(false);
        if (menuJardinBotanico != null) menuJardinBotanico.SetActive(true);
    }

    void ShowResults()
    {
        if (resultsPanel != null)
        {
            resultsPanel.SetActive(true);
            finalScoreText.text = currentScore.ToString();
            highScoreText.text = HighScore.ToString();
            timeText.text = FormatTime(gameTime);
            bestTimeText.text = BestTime == Mathf.Infinity ? "--:--" : FormatTime(BestTime);
        }
    }

    public void ToggleCamera(bool enable)
    {
        if (cameraManager != null)
        {
            cameraManager.enabled = enable;

            if (Application.isMobilePlatform)
            {
                WebCamTexture webcamTexture = new WebCamTexture();
                if (enable)
                {
                    webcamTexture.Play();
                }
                else
                {
                    webcamTexture.Stop();
                }
            }
        }
    }
}