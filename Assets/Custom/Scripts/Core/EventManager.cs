using System;
using UnityEngine;

public static class EventManager
{
    // --- Flujo General ---
    public static Action OnObjectSearchGameStarted;
    public static Action OnPlatformGameStarted;
    public static Action OnGameCancelled;
    public static Action OnGameResetAndExit;

    // --- AR System ---
    public static Action OnARSystemInitialized;

    // --- Bird Search Game ---
    public static Action<string> OnBirdCollected;
    public static Action<int> OnScoreAdded;
    public static Action OnAllBirdsCollected;

    // --- Platform Game ---
    public static Action<int> OnBananaCollected;
    public static Action OnAllBananasCollected;
    public static Action OnLevelCompleted; // Ejemplo, si se necesita

    // --- UI Events ---
    public static Action<int, int, int> OnObjectSearchUIPanelUpdate; // currentScore, found, total
    public static Action<float> OnTimerUpdate; // timeInSeconds
    public static Action<int> OnRemainingBananasUpdate; // bananasLeft
}
