using UnityEngine;

namespace Tractor
{
    public class TractorInput : MonoBehaviour
    {
        [SerializeField] string steerInputName;
        CustomInput steerInput;

        [SerializeField] string throttleInputName;
        CustomInput throttleInput;

        [SerializeField] string brakeInputName;
        CustomInput brakeInput;

        RCCP_CarController vehicle;
        RCCP_Input vehicleInput;

        InputController inputController;
        bool overrideInputs = false;

        void Start()
        {
            inputController = InputController.Instance;
            overrideInputs = inputController.overrideInputs;

            steerInput = inputController?.GetInputByName(steerInputName);
            throttleInput = inputController?.GetInputByName(throttleInputName);
            brakeInput = inputController?.GetInputByName(brakeInputName);

            UseOverrides();
        }

        void Update()
        {
            overrideInputs = inputController.overrideInputs;

            if (!vehicleInput || !overrideInputs)
                return;

            SetInputs();
        }

        void OnDisable()
        {
            if (vehicleInput)
                vehicleInput.overridePlayerInputs = false;

            if (vehicle)
                RCCP.SetExternalControl(vehicle, false);
        }

        void FindVehicle()
        {
            vehicle = FindAnyObjectByType<RCCP_CarController>();
            vehicleInput = vehicle.Inputs;
        }

        void SetInputs()
        {
            if (steerInput != null)
                vehicleInput.inputs.steerInput = steerInput.inputValue;

            if (throttleInput != null)
                vehicleInput.inputs.throttleInput = throttleInput.inputValue;

            if (brakeInput != null)
                vehicleInput.inputs.brakeInput = brakeInput.inputValue;
        }

        public void UseOverrides()
        {
            FindVehicle();

            RCCP.SetExternalControl(vehicle, overrideInputs);
            vehicleInput.overridePlayerInputs = overrideInputs;
        }
    }
}
