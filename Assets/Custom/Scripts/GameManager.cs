using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public enum GameMode
    {
        None,
        ObjectSearch,
        PlatformGame
    }
    public GameMode currentGameMode = GameMode.None;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        EventManager.OnARSystemInitialized?.Invoke();
    }

    private void OnEnable()
    {
        EventManager.OnObjectSearchGameStarted += SetObjectSearchMode;
        EventManager.OnPlatformGameStarted += SetPlatformGameMode;
        EventManager.OnGameCancelled += SetNoneMode;
        EventManager.OnGameResetAndExit += SetNoneMode;
    }

    private void OnDisable()
    {
        EventManager.OnObjectSearchGameStarted -= SetObjectSearchMode;
        EventManager.OnPlatformGameStarted -= SetPlatformGameMode;
        EventManager.OnGameCancelled -= SetNoneMode;
        EventManager.OnGameResetAndExit -= SetNoneMode;
    }
    
    public void StartGameBirds()
    {
        ResetAndExitGame();
        EventManager.OnObjectSearchGameStarted?.Invoke();
    }

    public void StartGameMonkey()
    {
        ResetAndExitGame();
        EventManager.OnPlatformGameStarted?.Invoke();
    }

    public void CancelCurrentGame()
    {
        EventManager.OnGameCancelled?.Invoke();
    }
    
    public void ResetAndExitGame()
    {
        EventManager.OnGameResetAndExit?.Invoke();
    }

    private void SetObjectSearchMode() => currentGameMode = GameMode.ObjectSearch;
    private void SetPlatformGameMode() => currentGameMode = GameMode.PlatformGame;
    private void SetNoneMode() => currentGameMode = GameMode.None;
    
    public void ShowPopUpPlataformGame()
    {
        // Se preserva el método para evitar null references en botones existentes
    }

    public void ShowPopUpObjectGame()
    {
        // Se preserva el método para evitar null references en botones existentes
    }
}