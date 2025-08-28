using System;
using System.Collections;
using InputActions;
using UnityEngine;
using Utils;
using Object = System.Object;

public class PlaController : MonoBehaviour // Same as "PlayerController" it's just a quick take on it
{
    [Header("Variables")]
    [SerializeField] private float moveSpeed;
    
    
    private const int MaxTries = 50;
    
    private float _mouseSensitivity, _pitchValue = 90;
    private GameManager _gameManager;
    private FerInputActions _inputActions;
    private Coroutine _getManagerStuffCoroutine, _getInputActionsCoroutine;
    private Vector2 _moveVec, _lookVec;
    private Camera _cam;
    private Rigidbody _rb;
    private Transform _camTrans;

    private void Awake()
    {
        if (!_gameManager || _mouseSensitivity <= 0) _getManagerStuffCoroutine ??= StartCoroutine(GetManagerStuff());
        if (!_inputActions) _getInputActionsCoroutine ??= StartCoroutine(GetInputActions());
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        _cam ??= Camera.main;
        _rb ??= GetComponent<Rigidbody>();
        _camTrans ??= _cam?.transform;
    }

    private IEnumerator GetManagerStuff()
    {
        int tries = 0;
        _gameManager = GameManager.Instance;
        while (!_gameManager && tries < MaxTries) {
            _gameManager = GameManager.Instance;
            tries++; yield return null;
        }
        if (!_gameManager) {
            EDebug.LogError($"Tried {MaxTries} times and couldn't find GameManager");
            EDebug.LogWarning("❌PlaController will disable itself❌");
            this.enabled = false; yield break;
        }
        tries = 0;
        _mouseSensitivity = _gameManager.MouseSensitivity;
        while (_mouseSensitivity <= 0 && tries < MaxTries) {
            _mouseSensitivity = _gameManager.MouseSensitivity;
            tries++; yield return null;
        }
        if (_mouseSensitivity <= 0) {
            EDebug.LogWarning($"Tried {MaxTries} times and couldn't find MouseSensitivity, setting to 5");
            _mouseSensitivity = 5;
        } else EDebug.LogWarning($"{this.name} ► Got Manager references but not it's values");
        EDebug.LogGood($"{this.name} ► Got Manager references and values correctly");
        _getManagerStuffCoroutine = null; // We let go of the reference so that it can be reused if need be
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
        EDebug.LogGood($"{this.name} ► Found InputActions");
        SubscribeToActions(); _getInputActionsCoroutine = null;
    }

    private void SubscribeToActions()
    {
        if (!_inputActions) {
            EDebug.LogError("Tried to subscribe to InputActions but reference is null");
            return;
        }
        _inputActions.OnMovePerformedEvent += OnMovePerformed;
        _inputActions.OnMoveCanceledEvent += OnMoveCanceledEvent;
        _inputActions.OnLookPerformedEvent += OnLookPerformed;
        _inputActions.OnLookCanceledEvent += OnLookCanceledEvent;
        EDebug.LogGood($"{this.name} ► Subscribed to InputActions events");
    }

    private void UnsubscribeToActions()
    {
        if (!_inputActions || !Application.isPlaying) {
            EDebug.LogWarning($"{this.name} ► Failed to unsubscribe from InputActions, the reference may be null or the application is quitting");
            return;
        }
        _inputActions.OnMovePerformedEvent -= OnMovePerformed;
        _inputActions.OnMoveCanceledEvent -= OnMoveCanceledEvent;
        _inputActions.OnLookPerformedEvent -= OnLookPerformed;
        _inputActions.OnLookCanceledEvent -= OnLookCanceledEvent;
        EDebug.LogWarning($"{this.name} ► Unsubscribed from InputActions events");
    }
    
    private void OnMovePerformed(Vector2 moveVec) => _moveVec = moveVec;
    private void OnMoveCanceledEvent(Vector2 moveVec) => _moveVec = Vector2.zero;
    private void OnLookPerformed(Vector2 lookVec) => _lookVec = lookVec;
    private void OnLookCanceledEvent(Vector2 lookVec) => _lookVec = Vector2.zero;

    private void FixedUpdate()
    {
        float delta = Time.fixedDeltaTime;
        float sideRot = _lookVec.x * _mouseSensitivity * delta;
        transform.Rotate(Vector3.up * sideRot);
        _pitchValue -= (_lookVec.y * _mouseSensitivity * delta);
        _pitchValue = Mathf.Clamp(_pitchValue, -85f, 85f);
        _camTrans.localRotation = Quaternion.Euler(_pitchValue, 0f, 0f);
        transform.position += (transform.forward * _moveVec.y + transform.right * _moveVec.x).normalized * (moveSpeed * delta);
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
