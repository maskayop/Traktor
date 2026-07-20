using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Tractor
{
    [Serializable]
    public class CustomInput
    {
        public string inputName;
        public Slider slider;

        InputAction on_InputAction;

        public void InitializeInputAction(InputActionMap INactionMap)
        {
            on_InputAction = INactionMap.FindAction(inputName);
        }

        public void EnableInputActions()
        {
            if (on_InputAction != null)
            {
                on_InputAction.performed += OnButtonPressed;
                on_InputAction.canceled += OnButtonReleased;
            }
        }

        public void DisableInputActions()
        {
            if (on_InputAction != null)
            {
                on_InputAction.performed -= OnButtonPressed;
                on_InputAction.canceled -= OnButtonReleased;
            }
        }

        void OnButtonPressed(InputAction.CallbackContext context)
        {
            slider.value = 1;
        }

        void OnButtonReleased(InputAction.CallbackContext context)
        {
            slider.value = 0;
        }
    }

    public class TestInputs : MonoBehaviour
    {
        [Header("Input System")]
        [SerializeField] InputActionAsset inputActionsAsset;
        [SerializeField] string inputActionMapName;

        [Header("Inputs")]
        [SerializeField] List<CustomInput> inputs = new List<CustomInput>();

        InputActionMap actionMap;
        bool isInitialized = false;

        void Start()
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
    }
}
