using UnityEngine;

namespace Tractor
{
    public class TractorInput : MonoBehaviour
    {
        [Header("Руль")]
        [SerializeField] string steerInputName;
        CustomInput steerInput;

        [Header("Педали")]
        [SerializeField] string throttleInputName;
        CustomInput throttleInput;

        [SerializeField] string brakeInputName;
        CustomInput brakeInput;

        [SerializeField] string clutchInputName;
        CustomInput clutchInput;

        [Header("Передачи")]
        [SerializeField] string gear1InputName;
        CustomInput gear1_Input;

        [SerializeField] string gear2InputName;
        CustomInput gear2_Input;

        [SerializeField] string gear3InputName;
        CustomInput gear3_Input;

        [SerializeField] string gear4InputName;
        CustomInput gear4_Input;

        RCCP_CarController vehicle;
        RCCP_Input vehicleInput;
        RCCP_Gearbox gearbox;

        InputController inputController;
        bool overrideInputs = false;

        void Start()
        {
            inputController = InputController.Instance;
            overrideInputs = inputController.overrideInputs;

            steerInput = inputController?.GetInputByName(steerInputName);
            throttleInput = inputController?.GetInputByName(throttleInputName);
            brakeInput = inputController?.GetInputByName(brakeInputName);
            clutchInput = inputController?.GetInputByName(clutchInputName);

            gear1_Input = inputController?.GetInputByName(gear1InputName);
            gear2_Input = inputController?.GetInputByName(gear2InputName);
            gear3_Input = inputController?.GetInputByName(gear3InputName);
            gear4_Input = inputController?.GetInputByName(gear4InputName);

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
            gearbox = vehicle.Gearbox;
        }

        void SetInputs()
        {
            if (steerInput != null)
                vehicleInput.inputs.steerInput = steerInput.inputValue;

            if (throttleInput != null)
                vehicleInput.inputs.throttleInput = throttleInput.inputValue;

            if (brakeInput != null)
                vehicleInput.inputs.brakeInput = brakeInput.inputValue;

            if (clutchInput != null)
                vehicleInput.inputs.clutchInput = clutchInput.inputValue;

            if (gear1_Input != null)
                ShiftToGear(gear1_Input.inputValue, 1);

            if (gear2_Input != null)
                ShiftToGear(gear2_Input.inputValue, 2);

            if (gear3_Input != null)
                ShiftToGear(gear3_Input.inputValue, 3);

            if (gear4_Input != null)
                ShiftToGear(gear4_Input.inputValue, 4);
        }

        void ShiftToGear(float input, int value)
        {
            if (input >= 0.5f)
                gearbox.ShiftToGear(value - 1);
        }

        public void UseOverrides()
        {
            FindVehicle();

            RCCP.SetExternalControl(vehicle, overrideInputs);
            vehicleInput.overridePlayerInputs = overrideInputs;
        }
    }
}
