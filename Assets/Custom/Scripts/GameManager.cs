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
                Debug.Log("No se detectó colisión con raycast");
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
    
        Time.timeScale = 0f;
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
        // Verificar que existe un ARSessionOrigin
        ARSessionOrigin sessionOrigin = FindObjectOfType<ARSessionOrigin>();
        if (sessionOrigin == null)
        {
            Debug.LogError("No ARSessionOrigin found. Creating one...");
            InitializeAR();
            return;
        }

        // Verificar que la cámara está configurada correctamente
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

        // Verificar que existe un ARSession
        if (FindObjectOfType<ARSession>() == null)
        {
            Debug.LogWarning("No ARSession found. Creating one...");
            GameObject arSession = new GameObject("AR Session");
            arSession.AddComponent<ARSession>();
        }

        // Verificar que el ARCameraManager está configurado
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
    
    // Cambiado: Usar ResetAndExitGame() para limpiar completamente
    ResetAndExitGame();
    
    // Verificar y configurar AR antes de iniciar el juego
    EnsureARSetup();
    
    currentGameMode = GameMode.ObjectSearch;
    gameActive = true;
    gameStartTime = Time.time;
    
    Debug.Log($"Modo de juego establecido: {currentGameMode}");
    Debug.Log($"Juego activo: {gameActive}");
    
    UpdateScoreUI();
    UpdateTimerUI();

    // Activar UI correcta
    if (gameUI != null) gameUI.SetActive(true);
    if (menuJardinBotanico != null) menuJardinBotanico.SetActive(false);
    if (resultsPanel != null) resultsPanel.SetActive(false);

    // Reactivar objetos recolectables para modo ObjectSearch
    ReactivateCollectableObjects();
    
    Debug.Log("StartGame() completado");
}

    public void StartPlatformGame()
{
    Debug.Log("StartPlatformGame() llamado");
    
    // Desactivar objetos recolectables ANTES de cambiar el modo
    DeactivateCollectableObjects();
    
    // Limpiar bananas del object search
    CleanBananasFromObjectSearch();
    
    // Cambiado: Usar ResetAndExitGame() para limpiar completamente
    ResetAndExitGame();
    
    currentGameMode = GameMode.PlatformGame;
    platformGameActive = true;
    
    Debug.Log($"Modo de juego establecido: {currentGameMode}");

    // Activar UI correcta
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
        
        // Desactivar objetos recolectables cuando se cancele cualquier juego
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
        
        // Limpiar bananas
        foreach (GameObject banana in spawnedBananas)
        {
            if (banana != null)
            {
                Destroy(banana);
            }
        }
        spawnedBananas.Clear();
        bananasCollected = 0;
        
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
        
        // Spawnear bananas en el mapa
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
        
        // Limpiar bananas existentes
        foreach (GameObject banana in spawnedBananas)
        {
            if (banana != null)
            {
                Destroy(banana);
            }
        }
        spawnedBananas.Clear();
        bananasCollected = 0;
        
        // Spawnear bananas en posiciones fijas
        for (int i = 0; i < totalBananas && i < bananaSpawnPositions.Length; i++)
        {
            // Calcular posición relativa al mapa
            Vector3 spawnPosition = platformMap.transform.position + bananaSpawnPositions[i];
            GameObject banana = Instantiate(bananaPrefab, spawnPosition, Quaternion.identity);
            
            // Configurar la banana
            banana.tag = "Banana";
            
            // Agregar collider si no tiene
            Collider col = banana.GetComponent<Collider>();
            if (col == null)
            {
                col = banana.AddComponent<SphereCollider>();
            }
            col.isTrigger = true;
            
            spawnedBananas.Add(banana);
            Debug.Log($"Banana {i + 1} spawnada en posición fija: {spawnPosition}");
        }
        
        // Actualizar UI
        if (remainingText != null)
        {
            remainingText.text = $"Bananas restantes: {totalBananas}";
        }
    }
    
    private Vector3 GetRandomPositionOnMap()
    {
        if (platformMap == null) return Vector3.zero;
        
        // Obtener los límites del mapa
        Vector3 mapPosition = platformMap.transform.position;
        float mapSizeX = mapBounds.x;
        float mapSizeZ = mapBounds.y;
        
        // Generar posición aleatoria dentro del mapa
        float randomX = Random.Range(-mapSizeX/2, mapSizeX/2);
        float randomZ = Random.Range(-mapSizeZ/2, mapSizeZ/2);
        
        Vector3 randomPos = mapPosition + new Vector3(randomX, 0.5f, randomZ);
        
                 return randomPos;
     }

    private bool IsBananaPrefab(GameObject prefab)
    {
        if (prefab == null) return false;
        
        // Verificar por nombre del prefab
        string prefabName = prefab.name.ToLower();
        if (prefabName.Contains("banana") || prefabName.Contains("banano"))
        {
            return true;
        }
        
        // Verificar por tag
        if (prefab.CompareTag("Banana"))
        {
            return true;
        }
        
        // Verificar si es el mismo prefab que bananaPrefab
        if (bananaPrefab != null && prefab == bananaPrefab)
        {
            return true;
        }
        
        return false;
    }

    private void CleanBananasFromObjectSearch()
    {
        Debug.Log("Limpiando bananas del object search");
        
        // Remover bananas de la lista de objetos spawnados
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

        // Si ya hay objetos spawnados, reactivarlos en las mismas posiciones
        if (spawnedObjects.Count > 0)
        {
            Debug.Log($"Reactivating {spawnedObjects.Count} existing objects");
            
            for (int i = 0; i < spawnedObjects.Count; i++)
            {
                if (spawnedObjects[i] != null)
                {
                    // Reactivar el objeto en su posición original
                    spawnedObjects[i].SetActive(true);
                    
                    // Reactivar componentes
                    Collider col = spawnedObjects[i].GetComponent<Collider>();
                    if (col != null)
                    {
                        col.enabled = true;
                    }
                    
                    FloatingObj floater = spawnedObjects[i].GetComponent<FloatingObj>();
                    if (floater != null)
                    {
                        floater.enabled = true;
                    }
                    
                    // Reiniciar el CollectableItem
                    CollectableItem collectable = spawnedObjects[i].GetComponent<CollectableItem>();
                    if (collectable != null)
                    {
                        collectable.itemValue = 10;
                    }
                }
            }
        }
        else
        {
            // Crear nuevos objetos si no existen
            for (int i = 0; i < objectsToSpawn; i++)
            {
                if (objectPrefabs == null || objectPrefabs.Count == 0)
                {
                    Debug.LogError("No object prefabs assigned");
                    return;
                }

                // Filtrar bananas del object search
                List<GameObject> validPrefabs = new List<GameObject>();
                foreach (GameObject prefab in objectPrefabs)
                {
                    if (prefab != null && !IsBananaPrefab(prefab))
                    {
                        validPrefabs.Add(prefab);
                    }
                }

                if (validPrefabs.Count == 0)
                {
                    Debug.LogError("No valid prefabs found (all are bananas or null)");
                    return;
                }

                GameObject prefabToSpawn = validPrefabs[Random.Range(0, validPrefabs.Count)];
                Vector3 randomPos = GetRandomPositionAroundDevice();

                GameObject obj = Instantiate(prefabToSpawn, randomPos, prefabToSpawn.transform.rotation);
                spawnedObjects.Add(obj);
                spawnedPositions.Add(randomPos);

                // Configurar el componente CollectableItem
                CollectableItem collectable = obj.GetComponent<CollectableItem>();
                if (collectable == null) 
                {
                    collectable = obj.AddComponent<CollectableItem>();
                }
                collectable.itemValue = 10;

                // Configurar el componente FloatingObj
                FloatingObj floater = obj.GetComponent<FloatingObj>();
                if (floater == null)
                {
                    floater = obj.AddComponent<FloatingObj>();
                }
                floater.floatSpeed = defaultFloatSpeed;
                floater.floatHeight = defaultFloatHeight;
                
                // Verificar y configurar collider
                Collider col = obj.GetComponent<Collider>();
                if (col == null)
                {
                    // Agregar un collider si no existe
                    col = obj.AddComponent<SphereCollider>();
                    Debug.Log("Collider agregado al objeto");
                }
                
                // Asegurar que el collider esté configurado como trigger
                col.isTrigger = true;
                col.enabled = true;
                
                Debug.Log($"Objeto configurado: {obj.name} - Collider: {col != null}, CollectableItem: {collectable != null}");
            }
        }
    }

    void ClearExistingObjects()
    {
        foreach (GameObject obj in spawnedObjects)
        {
            if (obj != null)
            {
                // Desactivar el objeto en lugar de destruirlo
                obj.SetActive(false);
                
                // Desactivar componentes
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
        // No limpiar spawnedPositions para mantener las posiciones originales
    }

    void DeactivateCollectableObjects()
    {
        Debug.Log("Desactivando objetos recolectables para modo no-ObjectSearch");
        foreach (GameObject obj in spawnedObjects)
        {
            if (obj != null)
            {
                // Desactivar el objeto completamente
                obj.SetActive(false);
                
                // Desactivar el collider para evitar colisiones
                Collider col = obj.GetComponent<Collider>();
                if (col != null)
                {
                    col.enabled = false;
                }
                
                // Desactivar FloatingObj
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
        Debug.Log("Reactivating objetos recolectables para modo ObjectSearch");
        
        // Si no hay objetos spawnados, crearlos
        if (spawnedObjects.Count == 0)
        {
            SpawnObjects();
            return;
        }
        
        // Reactivar objetos existentes (excluyendo bananas)
        foreach (GameObject obj in spawnedObjects)
        {
            if (obj != null && !IsBananaPrefab(obj))
            {
                // Reactivar el objeto
                obj.SetActive(true);
                
                // Reactivar el collider
                Collider col = obj.GetComponent<Collider>();
                if (col != null)
                {
                    col.enabled = true;
                }
                
                // Reactivar FloatingObj
                FloatingObj floater = obj.GetComponent<FloatingObj>();
                if (floater != null)
                {
                    floater.enabled = true;
                }
                
                // Reiniciar CollectableItem
                CollectableItem collectable = obj.GetComponent<CollectableItem>();
                if (collectable != null)
                {
                    collectable.itemValue = 10;
                }
            }
            else if (obj != null && IsBananaPrefab(obj))
            {
                // Mantener las bananas desactivadas en modo ObjectSearch
                obj.SetActive(false);
                
                // Desactivar el collider
                Collider col = obj.GetComponent<Collider>();
                if (col != null)
                {
                    col.enabled = false;
                }
                
                // Desactivar FloatingObj
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
        Debug.Log($"AddScore() llamado con {points} puntos");
        Debug.Log($"gameActive: {gameActive}, currentGameMode: {currentGameMode}");
        
        if (!gameActive || currentGameMode != GameMode.ObjectSearch)
        {
            Debug.LogWarning($"AddScore() cancelado - gameActive: {gameActive}, currentGameMode: {currentGameMode}");
            return;
        }

        currentScore += points;
        totalObjectsFound++;
        UpdateScoreUI();

        // Mostrar cuántos faltan
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
            ShowCollectionCompletePopup();
        }
        
        Debug.Log($"Puntuación actualizada: {currentScore}, objetos encontrados: {totalObjectsFound}");
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
    
    // Funciones para manejo de bananas en PlatformGame
    public void OnBananaCollected(int collected, int total)
    {
        bananasCollected = collected;
        Debug.Log($"Banana recolectada: {collected}/{total}");
        
        // Actualizar UI si existe
        if (remainingText != null)
        {
            remainingText.text = $"Bananas restantes: {total - collected}";
        }
        
        // Reproducir sonido si existe
        if (audioSource != null && objectFoundSound != null)
        {
            audioSource.PlayOneShot(objectFoundSound);
        }
    }
    
    public void OnAllBananasCollected()
    {
        Debug.Log("¡Todas las bananas han sido recolectadas!");
        
        // Mostrar mensaje de victoria
        if (remainingText != null)
        {
            remainingText.text = "¡Juego completado!";
        }
        
        // Mostrar el popup de colección completa
        ShowCollectionCompletePopup();
    }
    
    private void ShowPlatformGameCompletePopup()
    {
        // Crear un popup simple para mostrar que se completó el juego
        Debug.Log("Mostrando popup de juego completado");
        // Aquí puedes implementar la lógica del popup
    }
}