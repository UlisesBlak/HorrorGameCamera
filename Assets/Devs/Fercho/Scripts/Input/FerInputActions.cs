using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using Utils;

namespace InputActions
{
    [DefaultExecutionOrder(-25)]
    public class FerInputActions : Singleton<FerInputActions>
    {
        public void Enable() { InitStuff().Enable(); }
        private FerTestInput _inputActions;
        public static string LastDeviceType { get; private set; }
        public bool IsGamepad() { return LastDeviceType == "Gamepad"; }
        
        public Vector2 MoveVec { get; private set; }
        public Vector2 LookVec { get; private set; }
        public bool camAction { get; private set; }
        public bool camSecAction { get; private set; }


        private FerTestInput InitStuff()
        {
            if(_inputActions != null) return _inputActions;
            _inputActions = new FerTestInput();
            
            EDebug.LogGood("Input Actions ► Initialized 👍");
            return _inputActions;
        }
        
        protected override void OnAwake()
        {
            if (_inputActions == null) InitStuff();
            EDebug.LogGood("Input Actions ► Awake");
        }

        private void OnEnable()
        {
            InitStuff().Enable();
            // Physical Movement
            _inputActions.GameActions.Move.started += OnMoveStarted;
            _inputActions.GameActions.Move.performed += OnMovePerformed;
            _inputActions.GameActions.Move.canceled += OnMoveCanceled;
            // Camera Movement
            _inputActions.GameActions.Look.started += OnLookStarted;
            _inputActions.GameActions.Look.performed += OnLookPerformed;
            _inputActions.GameActions.Look.canceled += OnLookCanceled;
            // Camera Primary Action
            _inputActions.GameActions.CameraAction.started += OnCamActionStarted;
            _inputActions.GameActions.CameraAction.performed += OnCamActionPerformed;
            _inputActions.GameActions.CameraAction.canceled += OnCamActionCanceled;
            // Camera Secondary Action
            _inputActions.GameActions.CameraHold.started += OnCamHoldStarted;
            _inputActions.GameActions.CameraHold.performed += OnCamHoldPerformed;
            _inputActions.GameActions.CameraHold.canceled += OnCamHoldCanceled;
            // Device detection
            InputSystem.onEvent += OnAnyButtonPress;
            EDebug.LogGood("Input Actions ► Enabled");
        }

        private void OnDisable()
        { // Oh boy... get ready for the copy-paste action as this thing grows
            _inputActions.GameActions.Move.started -= OnMoveStarted;
            _inputActions.GameActions.Move.performed -= OnMovePerformed;
            _inputActions.GameActions.Move.canceled -= OnMoveCanceled;
            _inputActions.GameActions.Look.started -= OnLookStarted;
            _inputActions.GameActions.Look.performed -= OnLookPerformed;
            _inputActions.GameActions.Look.canceled -= OnLookCanceled;
            InputSystem.onEvent -= OnAnyButtonPress;
        }
        
        private static void OnAnyButtonPress(InputEventPtr eventPtr, InputDevice device)
        {
            LastDeviceType = device switch {
                Gamepad => "Gamepad",
                Keyboard or Mouse => "Keyboard&Mouse",
                _ => device.displayName
            };
        }
        
        #region Movement Input
        public event Action<Vector2> OnMoveStartedEvent;
        private void OnMoveStarted(InputAction.CallbackContext context)
        {
            MoveVec = context.ReadValue<Vector2>();
            OnMoveStartedEvent?.Invoke(MoveVec);
        }
        public event Action<Vector2> OnMovePerformedEvent;
        private void OnMovePerformed(InputAction.CallbackContext context) {
            MoveVec = context.ReadValue<Vector2>(); 
            OnMovePerformedEvent?.Invoke(MoveVec);
        }
        public event Action<Vector2> OnMoveCanceledEvent;
        private void OnMoveCanceled(InputAction.CallbackContext context) {
            MoveVec = Vector2.zero;
            OnMoveCanceledEvent?.Invoke(MoveVec);
        }
        #endregion
        
        #region Look Input
        public event Action<Vector2> OnLookStartedEvent;
        private void OnLookStarted(InputAction.CallbackContext context) {
            LookVec = context.ReadValue<Vector2>();
            OnLookStartedEvent?.Invoke(LookVec);
        }
        public event Action<Vector2> OnLookPerformedEvent;
        private void OnLookPerformed(InputAction.CallbackContext context) {
            LookVec = context.ReadValue<Vector2>();
            OnLookPerformedEvent?.Invoke(LookVec);
        }
        public event Action<Vector2> OnLookCanceledEvent;
        private void OnLookCanceled(InputAction.CallbackContext context) {
            LookVec = Vector2.zero;
            OnLookCanceledEvent?.Invoke(LookVec);
        }
        #endregion
        
        #region Camera Action Input
        public event Action<bool> OnCamActionStartedEvent;
        private void OnCamActionStarted(InputAction.CallbackContext context) {
            camAction = context.ReadValue<float>() > 0.25f;
            OnCamActionStartedEvent?.Invoke(camAction);
        }
        public event Action<bool> OnCamActionPerformedEvent;
        private void OnCamActionPerformed(InputAction.CallbackContext context) {
            camAction = context.ReadValue<float>() > 0.25f;
            OnCamActionPerformedEvent?.Invoke(camAction);
        }
        public event Action<bool> OnCamActionCanceledEvent;
        private void OnCamActionCanceled(InputAction.CallbackContext context) {
            camAction = false;
            OnCamActionCanceledEvent?.Invoke(camAction);
        }
        #endregion
        
        #region Camera Secondary Action Input
        public event Action<bool> OnCamHoldStartedEvent;
        private void OnCamHoldStarted(InputAction.CallbackContext context) {
            camSecAction = context.ReadValue<float>() > 0.25f;
            OnCamHoldStartedEvent?.Invoke(camSecAction);
        }
        public event Action<bool> OnCamHoldPerformedEvent;
        private void OnCamHoldPerformed(InputAction.CallbackContext context) {
            camSecAction = context.ReadValue<float>() > 0.25f;
            OnCamHoldPerformedEvent?.Invoke(camSecAction);
        }
        public event Action<bool> OnCamHoldCanceledEvent;
        private void OnCamHoldCanceled(InputAction.CallbackContext context) {
            camSecAction = false;
            OnCamHoldCanceledEvent?.Invoke(camSecAction);
        }
        #endregion
        
    }
}
