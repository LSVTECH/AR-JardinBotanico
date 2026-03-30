using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [Header("UI Elements")]
    public Text scoreText;
    public Text timerText;
    public GameObject gameUI;
    public GameObject menuJardinBotanico;
    public GameObject backButtonObj;      // Cambiado de button a la referencia visual del GO
    
    [Header("Specific Minigame UIs")]
    public GameObject canvasPartInfo;
    public GameObject zoomMap;
    public GameObject bananasRestantesUI;
    public GameObject cameraHUD;
    public GameObject canvasJoystick;
    public GameObject tocaParaMoverteUI;
    public ScreenOrientationManager screenOrientationManager;
    
    [Header("Album UI")]
    public GameObject albumPanel;

    [Header("Collection UI")]
    public GameObject PopUpPlataformGame;
    public GameObject PopUpObjectGame;
    public Text remainingText;
    public GameObject ArrastrarTexto;
    public GameObject BGjoystick;

    [Header("Result UI")]
    public GameObject resultsPanel;
    public Text finalScoreText;
    public Text highScoreText;
    public Text timeText;
    public Text bestTimeText;

    private void Awake()
    {
        // Setup listener if backButton is assigned as a Button component
        if (backButtonObj != null)
        {
            Button btn = backButtonObj.GetComponent<Button>();
            if (btn != null) btn.onClick.AddListener(() => EventManager.OnGameCancelled?.Invoke());
        }
    }

    private void OnEnable()
    {
        EventManager.OnObjectSearchGameStarted += HandleObjectSearchGameStarted;
        EventManager.OnPlatformGameStarted += HandlePlatformGameStarted;
        EventManager.OnGameResetAndExit += HandleGameResetAndExit;
        
        EventManager.OnObjectSearchUIPanelUpdate += UpdateObjectSearchScoreUI;
        EventManager.OnTimerUpdate += UpdateTimerUI;
        
        EventManager.OnRemainingBananasUpdate += UpdateRemainingBananasUI;
        EventManager.OnAllBirdsCollected += ShowAlbum;
    }

    private void OnDisable()
    {
        EventManager.OnObjectSearchGameStarted -= HandleObjectSearchGameStarted;
        EventManager.OnPlatformGameStarted -= HandlePlatformGameStarted;
        EventManager.OnGameResetAndExit -= HandleGameResetAndExit;
        
        EventManager.OnObjectSearchUIPanelUpdate -= UpdateObjectSearchScoreUI;
        EventManager.OnTimerUpdate -= UpdateTimerUI;

        EventManager.OnRemainingBananasUpdate -= UpdateRemainingBananasUI;
        EventManager.OnAllBirdsCollected -= ShowAlbum;
    }

    private void HandleObjectSearchGameStarted()
    {
        if (menuJardinBotanico != null) menuJardinBotanico.SetActive(false);
        if (resultsPanel != null) resultsPanel.SetActive(false);
        if (canvasPartInfo != null) canvasPartInfo.SetActive(false);
        if (bananasRestantesUI != null) bananasRestantesUI.SetActive(false);

        if (gameUI != null) gameUI.SetActive(true);
        if (backButtonObj != null) backButtonObj.SetActive(true);
        if (zoomMap != null) zoomMap.SetActive(true);
        if (cameraHUD != null) cameraHUD.SetActive(true);
    }

    private void HandlePlatformGameStarted()
    {
        if (menuJardinBotanico != null) menuJardinBotanico.SetActive(false);
        if (gameUI != null) gameUI.SetActive(false);
        if (resultsPanel != null) resultsPanel.SetActive(false);
        if (canvasPartInfo != null) canvasPartInfo.SetActive(false);

        if (backButtonObj != null) backButtonObj.SetActive(true);
        if (bananasRestantesUI != null) bananasRestantesUI.SetActive(true);
        if (canvasJoystick != null) canvasJoystick.SetActive(true);
        if (tocaParaMoverteUI != null) tocaParaMoverteUI.SetActive(true);

        if (screenOrientationManager != null) screenOrientationManager.SetLandscapeOrientation();
    }

    private void HandleGameResetAndExit()
    {
        if (gameUI != null) gameUI.SetActive(false);
        if (PopUpPlataformGame != null) PopUpPlataformGame.SetActive(false);
        if (resultsPanel != null) resultsPanel.SetActive(false);
        if (albumPanel != null) albumPanel.SetActive(false);
        
        // Esconder lo de los minijuegos
        if (backButtonObj != null) backButtonObj.SetActive(false);
        if (zoomMap != null) zoomMap.SetActive(false);
        if (cameraHUD != null) cameraHUD.SetActive(false);
        if (bananasRestantesUI != null) bananasRestantesUI.SetActive(false);
        if (canvasJoystick != null) canvasJoystick.SetActive(false);
        if (tocaParaMoverteUI != null) tocaParaMoverteUI.SetActive(false);

        if (menuJardinBotanico != null) menuJardinBotanico.SetActive(true);
        if (screenOrientationManager != null) screenOrientationManager.SetPortraitOrientation();
    }

    private void UpdateObjectSearchScoreUI(int currentScore, int found, int totalObjectsToSpawn)
    {
        if (scoreText != null)
        {
            scoreText.text = $"Puntos: {currentScore}\nEncontrados: {found}/{totalObjectsToSpawn}";
        }

        if (remainingText != null)
        {
            remainingText.text = $"Faltan: {totalObjectsToSpawn - found}";
        }
    }

    private void UpdateTimerUI(float timeInSeconds)
    {
        if (timerText != null)
        {
            timerText.text = FormatTime(timeInSeconds);
        }
    }

    private string FormatTime(float timeInSeconds)
    {
        int minutes = Mathf.FloorToInt(timeInSeconds / 60f);
        int seconds = Mathf.FloorToInt(timeInSeconds % 60f);
        return string.Format("{0:00}:{1:00}", minutes, seconds);
    }

    private void UpdateRemainingBananasUI(int totalBananas)
    {
        if (remainingText != null)
        {
            remainingText.text = $"Bananas restantes: {totalBananas}";
        }
    }

    private void ShowAlbum()
    {
        if (albumPanel != null)
        {
            albumPanel.SetActive(true);
        }
        else
        {
            Debug.LogWarning("albumPanel no asignado en UIManager");
            if (PopUpPlataformGame != null)
            {
                PopUpPlataformGame.SetActive(true);
            }
        }
    }

    public void OcultarTextoGuia()
    {
        if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
        {
            if (ArrastrarTexto != null)
            {
                ArrastrarTexto.SetActive(false);
            }
        }
    }

    private void Update()
    {
        OcultarTextoGuia();
    }
}
