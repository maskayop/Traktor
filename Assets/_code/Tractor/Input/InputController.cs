using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Tractor
{
    [Serializable]
    public class CustomInput
    {
        public string inputName;
        public bool useDeadzone = false;
        public bool normalize = false;
        public float deadzone = 0.05f;

        public float inputValue;
        public float InputValue { get { return inputValue; } set { inputValue = value; } }

        InputAction on_InputAction;

        public void InitializeInputAction(InputActionMap INactionMap)
        {
            on_InputAction = INactionMap.FindAction(inputName);
        }

        public void EnableInputActions()
        {
            if (on_InputAction != null)
            {
                on_InputAction.performed += OnValueChanged;
                on_InputAction.canceled += OnValueChanged;
            }
        }

        public void DisableInputActions()
        {
            if (on_InputAction != null)
            {
                on_InputAction.performed -= OnValueChanged;
                on_InputAction.canceled -= OnValueChanged;
            }
        }

        void OnValueChanged(InputAction.CallbackContext context)
        {
            float value = context.ReadValue<float>();

            if (useDeadzone && Mathf.Abs(value) < deadzone)
                value = 0f;

            if (normalize)
                inputValue = (value + 1) / 2;
            else
                inputValue = value;
        }
    }

    public class InputController : MonoBehaviour
    {
        public static InputController Instance;

        [Header("Input System")]
        [SerializeField] InputActionAsset inputActionsAsset;
        [SerializeField] string inputActionMapName;

        public bool overrideInputs = false;

        [Header("Inputs")]
        [SerializeField] List<CustomInput> inputs = new List<CustomInput>();

        InputActionMap actionMap;
        bool isInitialized = false;

        void Awake()
        {
            if (Instance != null)
            {
                Debug.LogWarning("Cannot create InputController");
                Destroy(gameObject);
                return;
            }

            Instance = this;
        }

        void Start()
        {
            Init();

        }

        public void Init()
        {
            InitializeInputActions();
        }

        void OnEnable()
        {
            if (isInitialized)
                EnableInputActions();
        }

        void OnDisable()
        {
            DisableInputActions();
        }

        void OnDestroy()
        {
            DisableInputActions();
        }

        void InitializeInputActions()
        {
            if (!inputActionsAsset)
                return;

            actionMap = inputActionsAsset.FindActionMap(inputActionMapName);

            if (actionMap == null)
                return;

            for (int i = 0; i < inputs.Count; i++)
                inputs[i].InitializeInputAction(actionMap);

            isInitialized = true;

            if (gameObject.activeInHierarchy && enabled)
                EnableInputActions();
        }

        void EnableInputActions()
        {
            if (actionMap != null)
                actionMap.Enable();

            for (int i = 0; i < inputs.Count; i++)
                inputs[i].EnableInputActions();
        }

        void DisableInputActions()
        {
            if (actionMap != null)
                actionMap.Disable();

            for (int i = 0; i < inputs.Count; i++)
                inputs[i].DisableInputActions();
        }

        public CustomInput GetInputByName(string INname)
        {
            for (int i = 0; i < inputs.Count; i++)
                if (inputs[i].inputName == INname)
                    return inputs[i];

            return null;
        }
    }
}
