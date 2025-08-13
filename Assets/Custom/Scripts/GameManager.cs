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

    [Header("Object Search Game Settings")]
    public List<GameObject> objectPrefabs;
    public int objectsToSpawn = 5;
    public float spawnRadius = 2f;
    public float minDistanceBetweenObjects = 1.0f;
    public float defaultFloatSpeed = 0.5f;
    public float defaultFloatHeight = 0.1f;

    // --- Album / colección ---
    [Header("Album UI")]
    public GameObject albumPanel;            // Panel del álbum
    public Transform albumGrid;              // Grid/Content para ítems
    public GameObject albumItemPrefab;       // Prefab UI con Image+Text (icono y nombre)
    private HashSet<string> collectedBirdIds = new HashSet<string>();


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



    [Header("Banana Spawn Positions")]
    public Vector3[] bananaSpawnPositions = new Vector3[]
    {
        new Vector3(-1.5f, 0.5f, -1.5f),
        new Vector3(1.5f, 0.5f, -1.5f),
        new Vector3(0f, 0.5f, 1.5f)
    };

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

    [Header("Collection UI")]
    public GameObject collectionCompletePopup;
    public Text remainingText;

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
    private List<GameObject> spawnedBananas = new List<GameObject>();
    private int bananasCollected = 0;

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
        Debug.Log("GameManager Awake() llamado");

        if (Instance == null)
        {
            Instance = this;
            Debug.Log("GameManager Instance establecido");
        }
        else if (Instance != this)
        {
            Debug.Log("GameManager duplicado encontrado, destruyendo...");
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
        Debug.Log($"GameManager inicializado. Modo actual: {currentGameMode}");
    }

    void Update()
    {
        if (currentGameMode == GameMode.ObjectSearch && Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            if (touch.phase == TouchPhase.Began)
            {
                Ray ray = Camera.main.ScreenPointToRay(touch.position);
                RaycastHit hit;

                if (Physics.Raycast(ray, out hit))
                {
                    Debug.Log($"Touch detectado en: {hit.collider.gameObject.name}");
                    CollectableItem item = hit.collider.GetComponent<CollectableItem>();
                    if (item != null)
                    {
                        Debug.Log("CollectableItem encontrado, llamando Collect()");
                        item.Collect();
                    }
                    else
                    {
                        Debug.Log("No se encontró CollectableItem en el objeto tocado");
                    }
                }
                else
                {
                    Debug.Log("");
                }
            }
        }
    }

    public void ShowCollectionCompletePopup()
    {
        if (collectionCompletePopup != null)
        {
            collectionCompletePopup.SetActive(true);
        }
        else
        {
            Debug.LogWarning("collectionCompletePopup no asignado en GameManager");
        }
    }

    public void ResetAndExitGame()
    {
        // 1. Reiniciar variables del juego
        currentScore = 0;
        totalObjectsFound = 0;
        gameActive = false;
        platformGameActive = false;
        currentGameMode = GameMode.None;

        // 2. Limpiar objetos recolectables (solo desactivar, no reactivar)
        ClearExistingObjects();

        // 3. Limpiar juego de plataformas
        if (platformPlayer != null)
        {
            Destroy(platformPlayer);
            platformPlayer = null;
        }

        if (platformMap != null)
        {
            Destroy(platformMap);
            platformMap = null;
        }

        // 4. Ocultar todos los UI
        if (gameUI != null) gameUI.SetActive(false);
        if (collectionCompletePopup != null) collectionCompletePopup.SetActive(false);
        if (resultsPanel != null) resultsPanel.SetActive(false);

        // 5. Mostrar menú principal
        if (menuJardinBotanico != null) menuJardinBotanico.SetActive(true);

        // 6. Reanudar tiempo de juego
        Time.timeScale = 1f;

        // 7. Desactivar joystick si está visible
        VirtualJoystickFade joystick = FindObjectOfType<VirtualJoystickFade>();
        if (joystick != null)
        {
            joystick.SetVisibility(false);
        }
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

            // Crear AR Session Origin
            GameObject arSessionOrigin = new GameObject("AR Session Origin");
            ARSessionOrigin sessionOrigin = arSessionOrigin.AddComponent<ARSessionOrigin>();

            // Crear AR Camera
            GameObject arCamera = new GameObject("AR Camera");
            arCamera.transform.SetParent(arSessionOrigin.transform);

            // Configurar la cámara
            Camera camera = arCamera.AddComponent<Camera>();
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = Color.black;
            camera.nearClipPlane = 0.1f;
            camera.farClipPlane = 20f;

            // Agregar ARCameraManager
            cameraManager = arCamera.AddComponent<ARCameraManager>();

            // Configurar la cámara en el ARSessionOrigin
            sessionOrigin.camera = camera;

            // Agregar ARSession si no existe
            if (FindObjectOfType<ARSession>() == null)
            {
                GameObject arSession = new GameObject("AR Session");
                arSession.AddComponent<ARSession>();
            }

            Debug.Log("AR Camera setup completed successfully");
        }
    }

    private void EnsureARSetup()
    {
        ARSessionOrigin sessionOrigin = FindObjectOfType<ARSessionOrigin>();
        if (sessionOrigin == null)
        {
            Debug.LogError("No ARSessionOrigin found. Creating one...");
            InitializeAR();
            return;
        }

        if (sessionOrigin.camera == null)
        {
            Debug.LogError("AR Session Origin camera is null. Reconfiguring...");
            Camera arCamera = sessionOrigin.GetComponentInChildren<Camera>();
            if (arCamera != null)
            {
                sessionOrigin.camera = arCamera;
                Debug.Log("AR Camera reconfigured successfully");
            }
            else
            {
                Debug.LogError("No camera found in AR Session Origin. Recreating AR setup...");
                InitializeAR();
            }
        }

        if (FindObjectOfType<ARSession>() == null)
        {
            Debug.LogWarning("No ARSession found. Creating one...");
            GameObject arSession = new GameObject("AR Session");
            arSession.AddComponent<ARSession>();
        }

        if (cameraManager == null)
        {
            cameraManager = sessionOrigin.GetComponentInChildren<ARCameraManager>();
            if (cameraManager == null)
            {
                Debug.LogError("No ARCameraManager found. Recreating AR setup...");
                InitializeAR();
            }
        }
    }

    public void StartGame()
    {
        Debug.Log("StartGame() llamado");

        ResetAndExitGame();
        EnsureARSetup();

        currentGameMode = GameMode.ObjectSearch;
        gameActive = true;
        gameStartTime = Time.time;

        objectsToSpawn = 4;

        Debug.Log($"Modo de juego establecido: {currentGameMode}");
        Debug.Log($"Juego activo: {gameActive}");

        UpdateScoreUI();
        UpdateTimerUI();

        if (gameUI != null) gameUI.SetActive(true);
        if (menuJardinBotanico != null) menuJardinBotanico.SetActive(false);
        if (resultsPanel != null) resultsPanel.SetActive(false);

        ReactivateCollectableObjects();

        Debug.Log("StartGame() completado");
    }

    public void StartPlatformGame()
    {
        Debug.Log("StartPlatformGame() llamado");

        DeactivateCollectableObjects();
        CleanBananasFromObjectSearch();

        ResetAndExitGame();

        currentGameMode = GameMode.PlatformGame;
        platformGameActive = true;

        Debug.Log($"Modo de juego establecido: {currentGameMode}");

        if (menuJardinBotanico != null) menuJardinBotanico.SetActive(false);
        if (gameUI != null) gameUI.SetActive(false);
        if (resultsPanel != null) resultsPanel.SetActive(false);

        PlacePlatformGame();
    }

    public void CancelCurrentGame()
    {
        Debug.Log($"Cancelando juego actual: {currentGameMode}");

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

        DeactivateCollectableObjects();

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

        foreach (GameObject banana in spawnedBananas)
        {
            if (banana != null)
            {
                Destroy(banana);
            }
        }
        spawnedBananas.Clear();
        bananasCollected = 0;

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

        SpawnBananas();
    }

    private void SpawnBananas()
    {
        if (bananaPrefab == null)
        {
            Debug.LogError("Banana prefab no asignado");
            return;
        }

        if (platformMap == null)
        {
            Debug.LogError("Platform map no encontrado");
            return;
        }

        foreach (GameObject banana in spawnedBananas)
        {
            if (banana != null)
            {
                Destroy(banana);
            }
        }
        spawnedBananas.Clear();
        bananasCollected = 0;

        for (int i = 0; i < totalBananas && i < bananaSpawnPositions.Length; i++)
        {
            Vector3 spawnPosition = platformMap.transform.position + bananaSpawnPositions[i];
            GameObject banana = Instantiate(bananaPrefab, spawnPosition, Quaternion.identity);

            banana.tag = "Banana";

            Collider col = banana.GetComponent<Collider>();
            if (col == null)
            {
                col = banana.AddComponent<SphereCollider>();
            }
            col.isTrigger = true;

            spawnedBananas.Add(banana);
            Debug.Log($"Banana {i + 1} spawnada en posición fija: {spawnPosition}");
        }

        if (remainingText != null)
        {
            remainingText.text = $"Bananas restantes: {totalBananas}";
        }
    }

    private Vector3 GetRandomPositionOnMap()
    {
        if (platformMap == null) return Vector3.zero;

        Vector3 mapPosition = platformMap.transform.position;
        float mapSizeX = mapBounds.x;
        float mapSizeZ = mapBounds.y;

        float randomX = Random.Range(-mapSizeX / 2, mapSizeX / 2);
        float randomZ = Random.Range(-mapSizeZ / 2, mapSizeZ / 2);

        Vector3 randomPos = mapPosition + new Vector3(randomX, 0.5f, randomZ);

        return randomPos;
    }

    private bool IsBananaPrefab(GameObject prefab)
    {
        if (prefab == null) return false;

        string prefabName = prefab.name.ToLower();
        if (prefabName.Contains("banana") || prefabName.Contains("banano"))
        {
            return true;
        }

        if (prefab.CompareTag("Banana"))
        {
            return true;
        }

        if (bananaPrefab != null && prefab == bananaPrefab)
        {
            return true;
        }

        return false;
    }

    private void CleanBananasFromObjectSearch()
    {
        Debug.Log("Limpiando bananas del object search");

        for (int i = spawnedObjects.Count - 1; i >= 0; i--)
        {
            if (spawnedObjects[i] != null && IsBananaPrefab(spawnedObjects[i]))
            {
                Debug.Log($"Removiendo banana del object search: {spawnedObjects[i].name}");
                Destroy(spawnedObjects[i]);
                spawnedObjects.RemoveAt(i);
            }
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

        // 1. Limpiar objetos existentes si los hay
        foreach (GameObject obj in spawnedObjects)
        {
            if (obj != null) Destroy(obj);
        }
        spawnedObjects.Clear();

        // 2. Tomar exactamente los primeros 4 prefabs de aves
        List<GameObject> birds = new List<GameObject>();
        int count = 0;
        foreach (var prefab in objectPrefabs)
        {
            if (prefab != null && !IsBananaPrefab(prefab) && count < 4)
            {
                birds.Add(prefab);
                count++;
            }
        }

        if (birds.Count < 4)
        {
            Debug.LogError("Se necesitan al menos 4 prefabs de aves en la lista");
            return;
        }

        // 3. Instanciar los 4 modelos específicos
        for (int i = 0; i < birds.Count; i++)
        {
            Vector3 randomPos = GetRandomPositionAroundDevice();
            GameObject obj = Instantiate(birds[i], randomPos, birds[i].transform.rotation);
            spawnedObjects.Add(obj);
            spawnedPositions.Add(randomPos);

            // Configurar componentes
            var collectable = obj.GetComponent<CollectableItem>();
            if (!collectable) collectable = obj.AddComponent<CollectableItem>();
            collectable.itemValue = 10; // Valor por defecto

            var floater = obj.GetComponent<FloatingObj>();
            if (!floater) floater = obj.AddComponent<FloatingObj>();
            floater.floatSpeed = defaultFloatSpeed;
            floater.floatHeight = defaultFloatHeight;

            var col = obj.GetComponent<Collider>();
            if (!col) col = obj.AddComponent<SphereCollider>();
            col.isTrigger = true;
            col.enabled = true;
        }

        objectsToSpawn = birds.Count;
    }


    void ClearExistingObjects()
    {
        foreach (GameObject obj in spawnedObjects)
        {
            if (obj != null)
            {
                obj.SetActive(false);
                FloatingObj floater = obj.GetComponent<FloatingObj>();
                if (floater != null)
                {
                    floater.enabled = false;
                }

                Collider col = obj.GetComponent<Collider>();
                if (col != null)
                {
                    col.enabled = false;
                }
            }
        }
    }

    void DeactivateCollectableObjects()
    {
        foreach (GameObject obj in spawnedObjects)
        {
            if (obj != null)
            {
                obj.SetActive(false);
                Collider col = obj.GetComponent<Collider>();
                if (col != null)
                {
                    col.enabled = false;
                }

                FloatingObj floater = obj.GetComponent<FloatingObj>();
                if (floater != null)
                {
                    floater.enabled = false;
                }
            }
        }
    }

    void ReactivateCollectableObjects()
    {
        if (spawnedObjects.Count == 0)
        {
            SpawnObjects();
            return;
        }

        foreach (GameObject obj in spawnedObjects)
        {
            if (obj != null && !IsBananaPrefab(obj))
            {
                obj.SetActive(true);
                Collider col = obj.GetComponent<Collider>();
                if (col != null)
                {
                    col.enabled = true;
                }

                FloatingObj floater = obj.GetComponent<FloatingObj>();
                if (floater != null)
                {
                    floater.enabled = true;
                }

                CollectableItem collectable = obj.GetComponent<CollectableItem>();
                if (collectable != null)
                {
                    collectable.itemValue = 10;
                }
            }
            else if (obj != null && IsBananaPrefab(obj))
            {
                obj.SetActive(false);
                Collider col = obj.GetComponent<Collider>();
                if (col != null)
                {
                    col.enabled = false;
                }

                FloatingObj floater = obj.GetComponent<FloatingObj>();
                if (floater != null)
                {
                    floater.enabled = false;
                }
            }
        }
    }

    public void AddScore(int points)
    {
        if (!gameActive || currentGameMode != GameMode.ObjectSearch)
        {
            return;
        }

        // Solo incrementar si aún no hemos alcanzado el total
        if (totalObjectsFound < objectsToSpawn)
        {
            currentScore += points;
            totalObjectsFound++;
            Debug.Log($"Objeto recolectado! Total: {totalObjectsFound}/{objectsToSpawn}");

            UpdateScoreUI();

            if (remainingText != null)
            {
                remainingText.text = $"Faltan: {objectsToSpawn - totalObjectsFound}";
            }

            if (audioSource != null && objectFoundSound != null)
            {
                audioSource.PlayOneShot(objectFoundSound);
            }

            if (totalObjectsFound >= objectsToSpawn)
            {
                ShowAlbum();
            }
        }
    }

    // Nuevo método para mostrar el álbum
    private void ShowAlbum()
    {
        if (albumPanel != null)
        {
            albumPanel.SetActive(true);

            // Pausar el juego
            Time.timeScale = 0f;
        }
        else
        {
            Debug.LogWarning("albumPanel no asignado en GameManager");
            // Mostrar popup normal como fallback
            ShowCollectionCompletePopup();
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
    public void CollectBanana()
    {
        bananasCollected++;
        Debug.Log($"Bananas recolectadas: {bananasCollected}/{totalBananas}");

        if (remainingText != null)
        {
            remainingText.text = $"Bananas restantes: {totalBananas - bananasCollected}";
        }

        if (bananasCollected >= totalBananas)
        {
            Debug.Log("¡Todas las bananas recolectadas!");
            ShowCollectionCompletePopup();
        }
    }
    public void CollectBird(string birdID)
    {
        if (string.IsNullOrEmpty(birdID)) return;

        // Registrar el ave recolectada
        if (!collectedBirdIds.Contains(birdID))
        {
            collectedBirdIds.Add(birdID);
            Debug.Log($"Ave recolectada: {birdID}");

            // Actualizar UI del álbum
            UpdateAlbumUI();
        }
    }
    private void UpdateAlbumUI()
    {
        // Aquí implementarías la lógica para actualizar el álbum
        // Mostrar las aves recolectadas, etc.
    }
}
