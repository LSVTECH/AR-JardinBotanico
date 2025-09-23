// QRModelViewer.cs
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

    private Dictionary<string, ModelState> activeModels = new Dictionary<string, ModelState>();
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

    private class ModelState
    {
        public GameObject modelObject;
        public ModelInteractor interactor;
        public Quaternion manualRotationOffset;
        public Vector3 manualScale;
        public bool isManipulating;
    }

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
                modelInfoDictionary[model.qrCodeName] = model;
        }
    }

    void OnEnable()
    {
        if (trackedImageManager != null)
            trackedImageManager.trackedImagesChanged += OnImageChanged;
    }

    void OnDisable()
    {
        if (trackedImageManager != null)
            trackedImageManager.trackedImagesChanged -= OnImageChanged;
    }

    void Update()
    {
        if (!isScanning) return;

        bool anyTracked = false;
        foreach (var qrCode in new List<string>(activeModels.Keys))
        {
            if (IsQRBeingTracked(qrCode))
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
            OnTrackedImageAdded(trackedImage);

        foreach (var trackedImage in eventArgs.updated)
            OnTrackedImageUpdated(trackedImage);

        foreach (var trackedImage in eventArgs.removed)
            OnTrackedImageRemoved(trackedImage);
    }

    private void OnTrackedImageAdded(ARTrackedImage trackedImage)
    {
        string qrCode = trackedImage.referenceImage.name;
        if (!activeModels.ContainsKey(qrCode))
            LoadModel(trackedImage);
    }

    private void OnTrackedImageUpdated(ARTrackedImage trackedImage)
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

    private void OnTrackedImageRemoved(ARTrackedImage trackedImage)
    {
        string qrCode = trackedImage.referenceImage.name;
        RemoveModel(qrCode);
    }

    void LoadModel(ARTrackedImage trackedImage)
    {
        string qrCode = trackedImage.referenceImage.name;
        if (modelInfoDictionary.TryGetValue(qrCode, out ModelInfo modelInfo))
        {
            GameObject model = Instantiate(modelInfo.modelPrefab, trackedImage.transform);
            model.transform.localPosition = Vector3.zero;

            ModelState state = new ModelState
            {
                modelObject = model,
                manualRotationOffset = Quaternion.Euler(modelInfo.initialRotation),
                manualScale = Vector3.one * modelInfo.initialScale,
                isManipulating = false
            };

            model.transform.localRotation = state.manualRotationOffset;
            model.transform.localScale = state.manualScale;

            ModelInteractor interactor = model.AddComponent<ModelInteractor>();
            interactor.Initialize(this, qrCode);
            state.interactor = interactor;

            activeModels[qrCode] = state;
            currentActiveQR = qrCode;
            UpdateModelUI();
            trackingTimer = 0f;
        }
    }

    void UpdateModelPosition(ARTrackedImage trackedImage)
    {
        string qrCode = trackedImage.referenceImage.name;
        if (activeModels.TryGetValue(qrCode, out ModelState state))
        {
            if (state.isManipulating) return;

            state.modelObject.transform.position = trackedImage.transform.position;
            state.modelObject.transform.rotation = trackedImage.transform.rotation * state.manualRotationOffset;
            state.modelObject.transform.localScale = state.manualScale;
        }
    }

    public void StartManipulation(string qrCode)
    {
        if (activeModels.TryGetValue(qrCode, out ModelState state))
            state.isManipulating = true;
    }

    public void StopManipulation(string qrCode, Quaternion newRotation, Vector3 newScale)
    {
        if (activeModels.TryGetValue(qrCode, out ModelState state))
        {
            state.isManipulating = false;
            state.manualRotationOffset = newRotation;
            state.manualScale = newScale;
        }
    }

    void RemoveModel(string qrCode)
    {
        if (activeModels.TryGetValue(qrCode, out ModelState state))
        {
            if (currentActiveQR == qrCode)
            {
                currentActiveQR = "";
                UpdateModelUI();
            }

            Destroy(state.modelObject);
            activeModels.Remove(qrCode);
        }
    }

    bool IsQRBeingTracked(string qrCode)
    {
        if (trackedImageManager == null) return false;

        foreach (var trackedImage in trackedImageManager.trackables)
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
        if (activeModels.TryGetValue(qrCode, out ModelState state) && state.modelObject != null)
            state.modelObject.SetActive(visible);
    }

    void UpdateModelUI()
    {
        if (!string.IsNullOrEmpty(currentActiveQR) &&
            modelInfoDictionary.TryGetValue(currentActiveQR, out ModelInfo modelInfo))
        {
            modelNameText.text = modelInfo.modelName;
            modelInfoText.text = partSelected && currentSelectedPart >= 0 &&
                currentSelectedPart < modelInfo.partInfo.Length ?
                modelInfo.partInfo[currentSelectedPart] :
                modelInfo.generalInfo;

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
                button.gameObject.SetActive(false);
        }
    }

    public void ShowPartInfo(int partIndex)
    {
        if (!infoPanel.activeSelf) infoPanel.SetActive(true);

        partSelected = true;
        currentSelectedPart = partIndex;

        if (!string.IsNullOrEmpty(currentActiveQR) &&
            modelInfoDictionary.TryGetValue(currentActiveQR, out ModelInfo modelInfo) &&
            partIndex < modelInfo.partInfo.Length)
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

        if (activeModels.TryGetValue(currentActiveQR, out ModelState state) &&
            state.interactor != null)
        {
            state.interactor.StartInteraction();
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
            trackedImageManager.enabled = enable;
    }

    public void OnQRButtonPressed()
    {
        menuPanel.SetActive(false);
        infoPanel.SetActive(true);
        if (backButton != null) backButton.SetActive(true);

        ResetPartSelection();
        ToggleScanning(true);

        if (arSession != null)
            arSession.Reset();
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
    }

    IEnumerator DelayedSessionReset()
    {
        yield return null;
        if (arSession != null)
            arSession.Reset();
        isResettingSession = false;
    }

    void ClearAllModels()
    {
        foreach (var pair in activeModels)
            if (pair.Value.modelObject != null)
                DestroyImmediate(pair.Value.modelObject);

        activeModels.Clear();
        currentActiveQR = "";
        Resources.UnloadUnusedAssets();
        System.GC.Collect();
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