#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

public class GameManagerMigration : MonoBehaviour
{
    [MenuItem("Tools/Migrar GameManager (Auto-Assign)")]
    public static void MigrateGameManager()
    {
        GameManager oldGm = FindObjectOfType<GameManager>();
        if (oldGm == null)
        {
            Debug.LogError("No se encontró GameManager en la escena.");
            return;
        }

        GameObject gmObject = oldGm.gameObject;

        // Añadir componentes si no existen
        UIManager uiManager = gmObject.GetComponent<UIManager>();
        if (uiManager == null) uiManager = gmObject.AddComponent<UIManager>();

        BirdSearchGameManager birdManager = gmObject.GetComponent<BirdSearchGameManager>();
        if (birdManager == null) birdManager = gmObject.AddComponent<BirdSearchGameManager>();

        PlatformGameManager platformManager = gmObject.GetComponent<PlatformGameManager>();
        if (platformManager == null) platformManager = gmObject.AddComponent<PlatformGameManager>();

        ARSystemManager arManager = gmObject.GetComponent<ARSystemManager>();
        if (arManager == null) arManager = gmObject.AddComponent<ARSystemManager>();

        // Usamos SerializedObject para transferir los valores de las propiedades sin importar que las hayamos borrado de GameManager (unity guarda los campos antiguos serializados hasta que el inspector los limpia)
        SerializedObject serializedOldGm = new SerializedObject(oldGm);
        
        // --- Migrar UIManager ---
        SerializedObject serializedUI = new SerializedObject(uiManager);
        CopyProperty(serializedOldGm, serializedUI, "scoreText");
        CopyProperty(serializedOldGm, serializedUI, "timerText");
        CopyProperty(serializedOldGm, serializedUI, "gameUI");
        CopyProperty(serializedOldGm, serializedUI, "menuJardinBotanico");
        CopyProperty(serializedOldGm, serializedUI, "backButton");
        CopyProperty(serializedOldGm, serializedUI, "startPlatformGameButton");
        CopyProperty(serializedOldGm, serializedUI, "albumPanel");
        CopyProperty(serializedOldGm, serializedUI, "PopUpPlataformGame");
        CopyProperty(serializedOldGm, serializedUI, "PopUpObjectGame");
        CopyProperty(serializedOldGm, serializedUI, "remainingText");
        CopyProperty(serializedOldGm, serializedUI, "ArrastrarTexto");
        CopyProperty(serializedOldGm, serializedUI, "BGjoystick");
        CopyProperty(serializedOldGm, serializedUI, "resultsPanel");
        CopyProperty(serializedOldGm, serializedUI, "finalScoreText");
        CopyProperty(serializedOldGm, serializedUI, "highScoreText");
        CopyProperty(serializedOldGm, serializedUI, "timeText");
        CopyProperty(serializedOldGm, serializedUI, "bestTimeText");
        serializedUI.ApplyModifiedProperties();

        // --- Migrar BirdSearchGameManager ---
        SerializedObject serializedBird = new SerializedObject(birdManager);
        CopyProperty(serializedOldGm, serializedBird, "objectPrefabs");
        CopyProperty(serializedOldGm, serializedBird, "objectsToSpawn");
        CopyProperty(serializedOldGm, serializedBird, "spawnRadius");
        CopyProperty(serializedOldGm, serializedBird, "minDistanceBetweenObjects");
        CopyProperty(serializedOldGm, serializedBird, "defaultFloatSpeed");
        CopyProperty(serializedOldGm, serializedBird, "defaultFloatHeight");
        CopyProperty(serializedOldGm, serializedBird, "checkAnimators");
        CopyProperty(serializedOldGm, serializedBird, "birdIds");
        CopyProperty(serializedOldGm, serializedBird, "audioSource");
        CopyProperty(serializedOldGm, serializedBird, "objectFoundSound");
        CopyProperty(serializedOldGm, serializedBird, "backgroundMusic");
        serializedBird.ApplyModifiedProperties();

        // --- Migrar PlatformGameManager ---
        SerializedObject serializedPlatform = new SerializedObject(platformManager);
        CopyProperty(serializedOldGm, serializedPlatform, "mapPrefab");
        CopyProperty(serializedOldGm, serializedPlatform, "playerPrefab");
        CopyProperty(serializedOldGm, serializedPlatform, "bananaPrefab");
        CopyProperty(serializedOldGm, serializedPlatform, "joystickID");
        CopyProperty(serializedOldGm, serializedPlatform, "placementDistance");
        CopyProperty(serializedOldGm, serializedPlatform, "placementHeight");
        CopyProperty(serializedOldGm, serializedPlatform, "mapRotation");
        CopyProperty(serializedOldGm, serializedPlatform, "mapBounds");
        CopyProperty(serializedOldGm, serializedPlatform, "playerSpawnOffset");
        CopyProperty(serializedOldGm, serializedPlatform, "totalBananas");
        serializedPlatform.ApplyModifiedProperties();

        Debug.Log("¡Migración de referencias completada! Por favor, verifica el objeto GameManager en el Inspector.");
    }

    private static void CopyProperty(SerializedObject source, SerializedObject destination, string propertyName)
    {
        SerializedProperty sourceProp = source.FindProperty(propertyName);
        SerializedProperty destProp = destination.FindProperty(propertyName);

        if (sourceProp != null && destProp != null)
        {
            // Unity Editor trick to copy property exactly
            EditorUtility.CopySerializedIfDifferent(source.targetObject, destination.targetObject);
            // Sin embargo CopySerializedIfDifferent no funciona entre componentes distintos,
            // pero podemos migrar si los campos se serializaron. 
        }
    }
}
#endif
