using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;

public class QRModelViewer : MonoBehaviour
{
    [Header("AR Components")]
    public ARTrackedImageManager trackedImageManager;
    public Camera arCamera;

    [Header("Model Settings")]
    public ModelInfo[] models;
    public float trackingTimeout = 1.5f;

    [Header("UI Components")]
    public TextMeshProUGUI modelNameText;
    public TextMeshProUGUI modelInfoText;
    public GameObject infoPanel;
    public Button[] partButtons;
    public GameObject menuPanel;
    public GameObject backButton;

    private Dictionary<string, GameObject> activeModels = new Dictionary<string, GameObject>();
    private Dictionary<string, ModelInfo> modelInfoDictionary = new Dictionary<string, ModelInfo>();
    private string currentActiveQR = "";
    private float trackingTimer = 0f;
    private bool isScanning = false;
    private int currentSelectedPart = -1;
    private bool partSelected = false;

    [Header("AR Session")]
    public ARSession arSession;

    private Coroutine delayedResetCoroutine;
    private bool isResettingSession = false;

    private ModelInteractor currentModelInteractor;

    [Header("Manual Rotation and Scale")]
    private Quaternion manualRotationOffset = Quaternion.identity;
    private Vector3 manualScale = Vector3.one;
    private bool isManipulating = false;

    void Start()
    {
        for (int i = 0; i < partButtons.Length; i++)
        {
            int index = i;
            partButtons[i].onClick.AddListener(() => ShowPartInfo(index));
            partButtons[i].gameObject.SetActive(false);
        }

        menuPanel.SetActive(true);
        infoPanel.SetActive(false);
        if (backButton != null) backButton.SetActive(false);

        foreach (ModelInfo model in models)
        {
            if (!string.IsNullOrEmpty(model.qrCodeName))
            {
                modelInfoDictionary[model.qrCodeName] = model;
            }
        }
    }

    void OnEnable()
    {
        if (trackedImageManager != null)
        {
            trackedImageManager.trackedImagesChanged += OnImageChanged;
        }
    }

    void OnDisable()
    {
        if (trackedImageManager != null)
        {
            trackedImageManager.trackedImagesChanged -= OnImageChanged;
        }
    }

    void Update()
    {
        if (!isScanning) return;

        bool anyTracked = false;

        foreach (var qrCode in new List<string>(activeModels.Keys))
        {
            bool isTracked = IsQRBeingTracked(qrCode);

            if (isTracked)
            {
                SetModelVisibility(qrCode, true);
                anyTracked = true;
            }
            else
            {
                trackingTimer += Time.deltaTime;
                if (trackingTimer > trackingTimeout)
                {
                    SetModelVisibility(qrCode, false);

                    if (currentActiveQR == qrCode)
                    {
                        currentActiveQR = "";
                        UpdateModelUI();
                    }
                }
            }
        }

        if (anyTracked) trackingTimer = 0f;
    }

    void OnImageChanged(ARTrackedImagesChangedEventArgs eventArgs)
    {
        if (!isScanning || isResettingSession) return;

        foreach (var trackedImage in eventArgs.added)
        {
            string qrCode = trackedImage.referenceImage.name;
            if (!activeModels.ContainsKey(qrCode))
            {
                LoadModel(trackedImage);
            }
        }

        foreach (var trackedImage in eventArgs.updated)
        {
            string qrCode = trackedImage.referenceImage.name;
            if (trackedImage.trackingState == UnityEngine.XR.ARSubsystems.TrackingState.Tracking)
            {
                UpdateModelPosition(trackedImage);
                SetModelVisibility(qrCode, true);

                if (currentActiveQR != qrCode)
                {
                    currentActiveQR = qrCode;
                    UpdateModelUI();
                }
            }
        }

        foreach (var trackedImage in eventArgs.removed)
        {
            string qrCode = trackedImage.referenceImage.name;
            RemoveModel(qrCode);
        }
    }

    void LoadModel(ARTrackedImage trackedImage)
    {
        string qrCode = trackedImage.referenceImage.name;

        if (modelInfoDictionary.TryGetValue(qrCode, out ModelInfo modelInfo))
        {
            GameObject model = Instantiate(modelInfo.modelPrefab, trackedImage.transform);
            model.transform.localPosition = Vector3.zero;

            manualRotationOffset = Quaternion.Euler(modelInfo.initialRotation);
            manualScale = Vector3.one * modelInfo.initialScale;

            model.transform.localRotation = manualRotationOffset;
            model.transform.localScale = manualScale;

            activeModels[qrCode] = model;
            currentActiveQR = qrCode;

            currentModelInteractor = model.AddComponent<ModelInteractor>();
            currentModelInteractor.Initialize(this);

            UpdateModelUI();
            trackingTimer = 0f;
        }
    }

    // UNICO METODO UpdateModelPosition
    void UpdateModelPosition(ARTrackedImage trackedImage)
    {
        if (isManipulating) return;

        string qrCode = trackedImage.referenceImage.name;
        if (activeModels.TryGetValue(qrCode, out GameObject model))
        {
            model.transform.position = trackedImage.transform.position;
            model.transform.rotation = trackedImage.transform.rotation * manualRotationOffset;
            model.transform.localScale = manualScale;
        }
    }

    public void StartManipulation()
    {
        isManipulating = true;
    }

    public void StopManipulation(Quaternion newRotation, Vector3 newScale)
    {
        isManipulating = false;
        manualRotationOffset = newRotation;
        manualScale = newScale;
    }

    void RemoveModel(string qrCode)
    {
        if (activeModels.TryGetValue(qrCode, out GameObject model))
        {
            if (currentActiveQR == qrCode)
            {
                currentActiveQR = "";
                UpdateModelUI();
            }

            Destroy(model);
            activeModels.Remove(qrCode);
        }
    }

    bool IsQRBeingTracked(string qrCode)
    {
        if (trackedImageManager == null) return false;

        var trackedImages = trackedImageManager.trackables;
        foreach (var trackedImage in trackedImages)
        {
            if (trackedImage.referenceImage.name == qrCode &&
                trackedImage.trackingState == UnityEngine.XR.ARSubsystems.TrackingState.Tracking)
            {
                return true;
            }
        }
        return false;
    }

    void SetModelVisibility(string qrCode, bool visible)
    {
        if (activeModels.TryGetValue(qrCode, out GameObject model) && model != null)
        {
            model.SetActive(visible);
        }
    }

    void UpdateModelUI()
    {
        if (!string.IsNullOrEmpty(currentActiveQR) &&
            modelInfoDictionary.TryGetValue(currentActiveQR, out ModelInfo modelInfo))
        {
            modelNameText.text = modelInfo.modelName;

            if (partSelected && currentSelectedPart >= 0 &&
                currentSelectedPart < modelInfo.partInfo.Length)
            {
                modelInfoText.text = modelInfo.partInfo[currentSelectedPart];
            }
            else
            {
                modelInfoText.text = modelInfo.generalInfo;
            }

            for (int i = 0; i < partButtons.Length; i++)
            {
                bool shouldActivate = i < modelInfo.partNames.Length;
                partButtons[i].gameObject.SetActive(shouldActivate);

                if (shouldActivate)
                {
                    partButtons[i].GetComponentInChildren<TextMeshProUGUI>().text = modelInfo.partNames[i];

                    var colors = partButtons[i].colors;
                    colors.normalColor = (i == currentSelectedPart) ? Color.yellow : Color.white;
                    partButtons[i].colors = colors;
                }
            }
        }
        else
        {
            modelNameText.text = "";
            modelInfoText.text = "";
            foreach (var button in partButtons)
            {
                button.gameObject.SetActive(false);
            }
        }
    }

    public void ShowPartInfo(int partIndex)
    {
        if (!infoPanel.activeSelf) infoPanel.SetActive(true);

        partSelected = true;
        currentSelectedPart = partIndex;

        if (!string.IsNullOrEmpty(currentActiveQR))
        {
            if (modelInfoDictionary.TryGetValue(currentActiveQR, out ModelInfo modelInfo))
            {
                if (partIndex < modelInfo.partInfo.Length)
                {
                    modelInfoText.text = modelInfo.partInfo[partIndex];

                    for (int i = 0; i < partButtons.Length; i++)
                    {
                        if (i < modelInfo.partNames.Length)
                        {
                            var colors = partButtons[i].colors;
                            colors.normalColor = (i == partIndex) ? Color.yellow : Color.white;
                            partButtons[i].colors = colors;
                        }
                    }
                }
            }
        }

        if (currentModelInteractor != null)
        {
            currentModelInteractor.StartInteraction();
        }
    }

    public void ResetPartSelection()
    {
        partSelected = false;
        currentSelectedPart = -1;
        UpdateModelUI();
    }

    void ToggleScanning(bool enable)
    {
        isScanning = enable;
        if (trackedImageManager != null)
        {
            trackedImageManager.enabled = enable;
        }
    }

    public void OnQRButtonPressed()
    {
        menuPanel.SetActive(false);
        infoPanel.SetActive(true);
        if (backButton != null) backButton.SetActive(true);

        ResetPartSelection();
        ToggleScanning(true);

        if (arSession != null)
        {
            arSession.Reset();
        }
    }

    public void OnBackButtonPressed()
    {
        ToggleScanning(false);
        infoPanel.SetActive(false);
        menuPanel.SetActive(true);
        if (backButton != null) backButton.SetActive(false);
        ResetPartSelection();
        ClearAllModels();

        if (arSession != null && !isResettingSession)
        {
            isResettingSession = true;
            if (delayedResetCoroutine != null) StopCoroutine(delayedResetCoroutine);
            delayedResetCoroutine = StartCoroutine(DelayedSessionReset());
        }

        if (currentModelInteractor != null)
        {
            currentModelInteractor.StopInteraction();
        }
        currentModelInteractor = null;
    }

    IEnumerator DelayedSessionReset()
    {
        yield return null;

        if (arSession != null)
        {
            arSession.Reset();
        }

        isResettingSession = false;
    }

    void ClearAllModels()
    {
        foreach (var pair in activeModels)
        {
            if (pair.Value != null)
            {
                DestroyImmediate(pair.Value);
            }
        }
        activeModels.Clear();
        currentActiveQR = "";

        Resources.UnloadUnusedAssets();
        System.GC.Collect();

        currentModelInteractor = null;
    }
}

[System.Serializable]
public class ModelInfo
{
    public string qrCodeName;
    public GameObject modelPrefab;
    public string modelName;
    [TextArea(3, 5)]
    public string generalInfo;
    public float initialScale = 0.1f;
    public Vector3 initialRotation;

    [Header("Part Information")]
    public string[] partNames;
    [TextArea(3, 5)]
    public string[] partInfo;
}