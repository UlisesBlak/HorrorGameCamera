using System.Collections;
using System.IO;
using InputActions;
using UnityEngine;
using Utils;

public class PlaPicture : MonoBehaviour
{
    [Header("Camera Picture Stuffs")]
    [Tooltip("Be mindful... Don't spam files to the folder! 💀")]
    [SerializeField] private bool saveToFolder;
    [Tooltip("Disable logs if they annoy you (Errors and Warnings will still be printed)")]
    [SerializeField] private bool enableDebugLogs = true;
    [Tooltip("Number of frames to validate if the player is still holding and/or actioning the camera. \n(Separates photo, flash and photo with flash actions)")]
    [SerializeField] private int framesToValidate = 8;
    [Tooltip("Light emitter object to enable and disable when using the flash")]
    [SerializeField] private GameObject flashEffectLight;
    [Tooltip("Duration of the flash (light effect) in seconds")]
    [SerializeField] private float flashDuration = 0.25f; // Will be converted to frames for random performance micro optimizations
    [Tooltip("The material in which the picture will be shown")]
    [SerializeField] private Material pictureMaterial;
    
    private const int MaxTries = 20;
    private static readonly string FolderPath = "Assets/Screenshots/";

    private Camera _cam;
    private int _layer = -1;
    private FerInputActions _inputActions; 
    private Coroutine _getInputActionsCoroutine, _readActionsCoroutine, _handleActionsCoroutine;
    private bool _isCamActioning, _isCamHolding;
    
    private void Awake()
    {
        _cam ??= this.GetComponent<Camera>();
        _cam ??= Camera.main;
        _layer = LayerMask.NameToLayer("FirstPerson");
        if (!_inputActions) _getInputActionsCoroutine ??= StartCoroutine(GetInputActions());
        if (!flashEffectLight) EDebug.LogWarning("Flash Effect Light reference is missing!");
        else flashEffectLight.SetActive(false);
    }
    
    private IEnumerator GetInputActions()
    {
        int tries = 0;
        _inputActions = FerInputActions.Instance;
        while (!_inputActions && tries < MaxTries) {
            _inputActions = FerInputActions.Instance;
            tries++; yield return null;
        }
        if (!_inputActions) {
            EDebug.LogError($"Tried {MaxTries} times and couldn't find FerInputActions, PlaController will disable itself");
            this.enabled = false; yield break;
        }
        if (enableDebugLogs) EDebug.LogGood($"{this.name} ► Found InputActions");
        SubscribeToActions(); _getInputActionsCoroutine = null;
    }
    
    private void SubscribeToActions()
    {
        if (!_inputActions) {
            EDebug.LogError("Tried to subscribe to InputActions but reference is null");
            return;
        }
        _inputActions.OnCamHoldStartedEvent += OnCamHold;
        _inputActions.OnCamHoldPerformedEvent += OnCamHold;
        _inputActions.OnCamHoldCanceledEvent += OnCamHoldCanceled;
        _inputActions.OnCamActionStartedEvent += OnCamAction;
        _inputActions.OnCamActionPerformedEvent += OnCamAction;
        _inputActions.OnCamActionCanceledEvent += OnCamActionCanceled;
        if (enableDebugLogs) EDebug.LogGood($"{this.name} ► Subscribed to InputActions events");
    }

    private void UnsubscribeToActions()
    {
        if (!_inputActions || !Application.isPlaying) {
            EDebug.LogWarning($"{this.name} ► Failed to unsubscribe from InputActions, the reference may be null or the application is quitting");
            return;
        }
        _inputActions.OnCamHoldStartedEvent -= OnCamHold;
        _inputActions.OnCamHoldPerformedEvent -= OnCamHold;
        _inputActions.OnCamHoldCanceledEvent -= OnCamHoldCanceled;
        _inputActions.OnCamActionStartedEvent -= OnCamAction;
        _inputActions.OnCamActionPerformedEvent -= OnCamAction;
        _inputActions.OnCamActionCanceledEvent -= OnCamActionCanceled;
        EDebug.LogWarning($"{this.name} ► Unsubscribed from InputActions events");
    }
    
    private void OnCamHoldCanceled(bool isHolding) {
        _isCamHolding = false;
        if (_readActionsCoroutine != null) StopCoroutine(_readActionsCoroutine);
        _readActionsCoroutine = null;
    }
    private void OnCamHold(bool isHolding) {
        _isCamHolding = true;
        if (_isCamHolding) _readActionsCoroutine ??= StartCoroutine(ReadActions());
    }
    private void OnCamActionCanceled(bool isActioning) {
        _isCamActioning = false;
        if (_readActionsCoroutine != null) StopCoroutine(_readActionsCoroutine);
        _readActionsCoroutine = null;
    }
    private void OnCamAction(bool isActioning) {
        _isCamActioning = true;
        if (_isCamActioning) _readActionsCoroutine ??= StartCoroutine(ReadActions());
    }

    private IEnumerator ReadActions()
    {   // We validate if the player is still holding or actioning the camera for a few frames
        for (int i = 0; i < framesToValidate; i++) {
            if (!_isCamHolding && !_isCamActioning) {
                if (enableDebugLogs) EDebug.Log("Stopped reading actions");
                _readActionsCoroutine = null;
                yield break;
            }
            if (_isCamHolding && _isCamActioning) {
                if (enableDebugLogs) EDebug.Log("Holding and Actioning the camera");
                _handleActionsCoroutine ??= StartCoroutine(HandleActions(true, true));
                _readActionsCoroutine = null;
                yield break;
            }
        }
        if (_isCamHolding) {
                if (enableDebugLogs) EDebug.Log("Holding the camera");
                _handleActionsCoroutine ??= StartCoroutine(HandleActions(false, true));
                _readActionsCoroutine = null; yield break;
        }
        if (_isCamActioning) {
            _handleActionsCoroutine ??= StartCoroutine(HandleActions(true, false));
            if (enableDebugLogs) EDebug.Log("Actioning the camera");
            _readActionsCoroutine = null; yield break;
        } // The last two wait till the end frame to confirm
        yield return null;
        
    }

    private IEnumerator HandleActions(bool photo, bool flash)
    {
        int frames = MathUtils.SecondsToFrames(flashDuration);
        int midFrame = frames / 2;
        if (flash) {
            flashEffectLight.SetActive(true);
            for (int i = 0; i < frames; i++) {
                if (photo && i == midFrame) {
                    if (enableDebugLogs) EDebug.Log("Taking Photo");
                    TakePhoto();
                }
                yield return null;
            }
            flashEffectLight.SetActive(false);
        }
        else if (photo) {
            if (enableDebugLogs) EDebug.Log("Taking Photo");
            TakePhoto();
        }
        
        _handleActionsCoroutine = null;
    }

    private void TakePhoto()
    {
        int ogMask = _cam.cullingMask;
        if (_layer != -1) _cam.cullingMask &= ~(1 << _layer);
        RenderTexture rt = new RenderTexture(Screen.width, Screen.height, 24 /*or 32, IDK*/);
        _cam.targetTexture = rt;
        _cam.Render();
        RenderTexture.active = rt;
        Texture2D screenshot = new Texture2D(Screen.width, Screen.height, TextureFormat.RGB24, false);
        screenshot.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0);
        screenshot.Apply();
        _cam.targetTexture = null;
        RenderTexture.active = null;
        _cam.cullingMask = ogMask; //if (_layer != -1) _cam.cullingMask |= (1 << _layer); (Same thing)
        Destroy(rt);
        var screenshotName = $"Ss{System.DateTime.Now:yyyyMMddHHmmss}.png";
        if (saveToFolder) { // WriteAllBytes seems to take some time at random inrtervals, you may not see it in the folder immediately
            if (!Directory.Exists(FolderPath)) Directory.CreateDirectory(FolderPath);
            File.WriteAllBytes(Path.Combine(FolderPath, screenshotName), screenshot.EncodeToPNG());
        }
        if (pictureMaterial) pictureMaterial.mainTexture = screenshot;
        if (enableDebugLogs && saveToFolder) EDebug.LogGood($"Screenshot taken and saved to {FolderPath}{screenshotName}");
    }
    
    private void OnDisable()
    {
        UnsubscribeToActions();
    }

    private void OnDestroy()
    {
        UnsubscribeToActions();
    }
    
}
