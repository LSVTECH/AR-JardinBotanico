using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;
using Random = UnityEngine.Random;

public class GameManager : MonoBehaviour
{
    public ARCameraManager cameraManager;
    public static GameManager Instance;

    [Header("Game Settings")]
    public List<GameObject> objectPrefabs; // Lista de modelos 3D
    public int objectsToSpawn = 5;
    public float spawnRadius = 2f;
    public float minDistanceBetweenObjects = 1.0f;
    public float defaultFloatSpeed = 0.5f;
    public float defaultFloatHeight = 0.1f;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip objectFoundSound;

    [Header("UI Elements")]
    public Text scoreText;
    public Text timerText; // Nuevo: Texto para mostrar el tiempo
    public GameObject gameUI;
    public GameObject menuJardinBotanico;
    public Button backButton;

    private List<GameObject> spawnedObjects = new List<GameObject>();
    private int currentScore = 0;
    private int totalObjectsFound = 0;
    private bool gameActive = false;
    private List<Vector3> spawnedPositions = new List<Vector3>();
    private float gameStartTime; // Nuevo: Tiempo de inicio del juego
    private float gameTime; // Nuevo: Tiempo transcurrido

    const string HIGH_SCORE_KEY = "HighScore";
    const string BEST_TIME_KEY = "BestTime"; // Nuevo: Clave para mejor tiempo

    [Header("Result UI")]
    public GameObject resultsPanel;
    public Text finalScoreText;
    public Text highScoreText;
    public Text timeText; // Nuevo: Texto para tiempo actual
    public Text bestTimeText; // Nuevo: Texto para mejor tiempo

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        if (backButton != null)
        {
            backButton.onClick.AddListener(CancelGame);
        }
    }

    public void StartGame()
    {
        currentScore = 0;
        totalObjectsFound = 0;
        gameActive = true;
        gameStartTime = Time.time; // Registrar hora de inicio
        UpdateScoreUI();
        UpdateTimerUI(); // Iniciar actualización del tiempo

        gameUI.SetActive(true);
        menuJardinBotanico.SetActive(false);
        resultsPanel.SetActive(false);

        ClearExistingObjects();
        SpawnObjects();
    }

    Vector3 GetRandomPositionAroundDevice()
    {
        Vector3 center = Camera.main.transform.position;
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
            GameObject prefabToSpawn = objectPrefabs[Random.Range(0, objectPrefabs.Count)];
            Vector3 randomPos = GetRandomPositionAroundDevice();

            GameObject obj = Instantiate(prefabToSpawn, randomPos, prefabToSpawn.transform.rotation);
            spawnedObjects.Add(obj);
            spawnedPositions.Add(randomPos);

            // Asegurarse que siempre tenga el componente FloatingObject
            if (obj.GetComponent<FloatingObject>() == null)
            {
                obj.AddComponent<FloatingObject>();
            }
        }
    }

    void ClearExistingObjects()
    {
        foreach (GameObject obj in spawnedObjects)
        {
            if (obj != null)
            {
                FloatingObject floater = obj.GetComponent<FloatingObject>();
                if (floater != null) Destroy(floater);
                Destroy(obj);
            }
        }
        spawnedObjects.Clear();
        spawnedPositions.Clear();
    }

    public void AddScore(int points)
    {
        if (!gameActive) return;

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

    // Nuevo: Actualizar el texto del temporizador
    void UpdateTimerUI()
    {
        if (gameActive && timerText != null)
        {
            gameTime = Time.time - gameStartTime;
            timerText.text = FormatTime(gameTime);
            Invoke("UpdateTimerUI", 0.1f); // Actualizar cada 0.1 segundos
        }
    }

    // Nuevo: Formatear tiempo a mm:ss
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

    // Nuevo: Propiedad para mejor tiempo
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

    // Nuevo: Verificar y guardar mejor tiempo
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
        gameTime = Time.time - gameStartTime; // Calcular tiempo final
        CheckHighScore();
        CheckBestTime(); // Verificar si es récord de tiempo
        ShowResults();
        gameUI.SetActive(false);
        menuJardinBotanico.SetActive(true);
    }

    void ShowResults()
    {
        resultsPanel.SetActive(true);
        finalScoreText.text = currentScore.ToString();
        highScoreText.text = HighScore.ToString();

        // Nuevo: Mostrar tiempos
        timeText.text = FormatTime(gameTime);
        bestTimeText.text = BestTime == Mathf.Infinity ? "--:--" : FormatTime(BestTime);
    }

    public void CancelGame()
    {
        if (!gameActive) return;

        ClearExistingObjects();
        gameActive = false;

        gameUI.SetActive(false);
        menuJardinBotanico.SetActive(true);
        resultsPanel.SetActive(false);

        // No guardar tiempo al cancelar
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

// Clase modificada para permitir configuración individual
public class FloatingObject : MonoBehaviour
{
    public float floatSpeed = 0.5f;
    public float floatHeight = 0.1f;

    private Vector3 startPosition;
    private float randomOffset;

    void Start()
    {
        startPosition = transform.position;
        randomOffset = Random.Range(0f, 2f * Mathf.PI);

        // Intentar obtener configuración específica del objeto
        ObjectFloatSettings settings = GetComponent<ObjectFloatSettings>();
        if (settings != null)
        {
            floatSpeed = settings.customFloatSpeed;
            floatHeight = settings.customFloatHeight;
        }
    }

    void Update()
    {
        float newY = startPosition.y + Mathf.Sin((Time.time + randomOffset) * floatSpeed) * floatHeight;
        transform.position = new Vector3(startPosition.x, newY, startPosition.z);
        transform.Rotate(Vector3.up * Time.deltaTime * 20f);
    }
}

// Nuevo: Componente para configuración individual de objetos
public class ObjectFloatSettings : MonoBehaviour
{
    public float customFloatSpeed = 0.5f;
    public float customFloatHeight = 0.1f;
}