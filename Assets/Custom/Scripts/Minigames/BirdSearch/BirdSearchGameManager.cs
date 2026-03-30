using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BirdSearchGameManager : MonoBehaviour
{
    [Header("Object Search Game Settings")]
    public List<GameObject> objectPrefabs;
    public int objectsToSpawn = 4;
    public float spawnRadius = 2f;
    public float minDistanceBetweenObjects = 1.0f;
    public float defaultFloatSpeed = 0.5f;
    public float defaultFloatHeight = 0.1f;

    [Header("Bird Check Animations")]
    public Animator[] checkAnimators; // Arreglo de animadores de checks
    public string[] birdIds; // IDs de las aves en el mismo orden que los prefabs

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip objectFoundSound;
    public AudioClip backgroundMusic;

    private List<GameObject> spawnedObjects = new List<GameObject>();
    private List<Vector3> spawnedPositions = new List<Vector3>();
    private HashSet<string> collectedBirdIds = new HashSet<string>();

    private int currentScore = 0;
    private int totalObjectsFound = 0;
    private bool gameActive = false;
    private float gameStartTime;

    private void OnEnable()
    {
        EventManager.OnObjectSearchGameStarted += StartGame;
        EventManager.OnGameCancelled += CancelGame;
        EventManager.OnGameResetAndExit += CancelGame;
        EventManager.OnBirdCollected += CollectBird;
    }

    private void OnDisable()
    {
        EventManager.OnObjectSearchGameStarted -= StartGame;
        EventManager.OnGameCancelled -= CancelGame;
        EventManager.OnGameResetAndExit -= CancelGame;
        EventManager.OnBirdCollected -= CollectBird;
    }

    private void StartGame()
    {
        gameActive = true;
        currentScore = 0;
        totalObjectsFound = 0;
        collectedBirdIds.Clear();
        gameStartTime = Time.time;
        objectsToSpawn = 4;

        ResetCheckAnimations();
        ReactivateCollectableObjects();
        PlayBackgroundMusic();
        
        EventManager.OnObjectSearchUIPanelUpdate?.Invoke(currentScore, totalObjectsFound, objectsToSpawn);
    }

    private void CancelGame()
    {
        if (!gameActive) return;
        gameActive = false;
        ClearExistingObjects();
        StopBackgroundMusic();
        ResetCheckAnimations();
    }

    private void Update()
    {
        if (gameActive)
        {
            EventManager.OnTimerUpdate?.Invoke(Time.time - gameStartTime);

            if (Input.touchCount > 0)
            {
                Touch touch = Input.GetTouch(0);
                if (touch.phase == TouchPhase.Began)
                {
                    Ray ray = Camera.main.ScreenPointToRay(touch.position);
                    if (Physics.Raycast(ray, out RaycastHit hit))
                    {
                        CollectableItem item = hit.collider.GetComponent<CollectableItem>();
                        if (item != null)
                        {
                            string originalName = hit.collider.gameObject.name.Replace("(Clone)", "").Trim();
                            item.Collect();
                            CollectBird(originalName);
                        }
                    }
                }
            }

            for (int i = 0; i < checkAnimators.Length; i++)
            {
                if (checkAnimators[i] != null && !checkAnimators[i].gameObject.activeSelf)
                {
                    checkAnimators[i].gameObject.SetActive(true);
                }
            }
        }
    }

    private void PlayBackgroundMusic()
    {
        if (audioSource != null && backgroundMusic != null)
        {
            audioSource.clip = backgroundMusic;
            audioSource.loop = true;
            audioSource.Play();
        }
    }

    private void StopBackgroundMusic()
    {
        if (audioSource != null)
        {
            audioSource.Stop();
        }
    }

    private void ReactivateCollectableObjects()
    {
        ClearExistingObjects();
        SpawnObjects();
    }

    private void ClearExistingObjects()
    {
        foreach (GameObject obj in spawnedObjects)
        {
            if (obj != null) Destroy(obj);
        }
        spawnedObjects.Clear();
        spawnedPositions.Clear();
    }

    private void SpawnObjects()
    {
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

        for (int i = 0; i < birds.Count; i++)
        {
            Vector3 randomPos = GetRandomPositionAroundDevice();
            GameObject obj = Instantiate(birds[i], randomPos, birds[i].transform.rotation);
            spawnedObjects.Add(obj);
            spawnedPositions.Add(randomPos);

            var collectable = obj.GetComponent<CollectableItem>();
            if (!collectable) collectable = obj.AddComponent<CollectableItem>();
            collectable.itemValue = 10;
            // collectable.birdID should ideally be set here based on prefab, but we rely on existing logic

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

    private bool IsBananaPrefab(GameObject prefab)
    {
        if (prefab == null) return false;
        string prefabName = prefab.name.ToLower();
        return prefabName.Contains("banana") || prefabName.Contains("banano") || prefab.CompareTag("Banana");
    }

    private Vector3 GetRandomPositionAroundDevice()
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

    private bool IsPositionValid(Vector3 position)
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

    public void CollectBird(string birdID)
    {
        if (!gameActive) return;

        if (totalObjectsFound < objectsToSpawn)
        {
            currentScore += 10; // Valor harcodeado como en original itemValue=10
            totalObjectsFound++;

            EventManager.OnObjectSearchUIPanelUpdate?.Invoke(currentScore, totalObjectsFound, objectsToSpawn);

            if (audioSource != null && objectFoundSound != null)
            {
                audioSource.PlayOneShot(objectFoundSound);
            }

            if (!string.IsNullOrEmpty(birdID) && !collectedBirdIds.Contains(birdID))
            {
                collectedBirdIds.Add(birdID);
                ActivateCheckAnimation(birdID);
            }

            if (totalObjectsFound >= objectsToSpawn)
            {
                EventManager.OnAllBirdsCollected?.Invoke();
            }
        }
    }

    private void ResetCheckAnimations()
    {
        for (int i = 0; i < checkAnimators.Length; i++)
        {
            if (checkAnimators[i] != null)
            {
                checkAnimators[i].gameObject.SetActive(true);
                checkAnimators[i].SetBool("Activate", false);
                checkAnimators[i].Play("EmptyState", -1, 0f);
                checkAnimators[i].Update(0f);
            }
        }
    }

    private void ActivateCheckAnimation(string birdID)
    {
        int birdIndex = System.Array.IndexOf(birdIds, birdID);
        if (birdIndex >= 0 && birdIndex < checkAnimators.Length && checkAnimators[birdIndex] != null)
        {
            checkAnimators[birdIndex].gameObject.SetActive(true);
            StartCoroutine(ActivateCheckWithDelay(checkAnimators[birdIndex]));
        }
    }

    private IEnumerator ActivateCheckWithDelay(Animator animator)
    {
        yield return new WaitForEndOfFrame();
        animator.SetBool("Activate", true);
    }
}
