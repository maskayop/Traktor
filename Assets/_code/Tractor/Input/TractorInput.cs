using UnityEngine;

namespace Tractor
{
    public class TractorInput : MonoBehaviour
    {
        [Header("Механизмы")]
        [SerializeField] TractorMain tractorMain;
        [SerializeField] TractorGearbox tractorGearbox;

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

        [Header("Уровни")]
        [SerializeField] string level1InputName;
        CustomInput level1_Input;

        [SerializeField] string level2InputName;
        CustomInput level2_Input;

        [Header("Диапазоны")]
        [SerializeField] string range12InputName;
        CustomInput range12_Input;

        [SerializeField] string range34InputName;
        CustomInput range34_Input;

        [SerializeField] string rangeRInputName;
        CustomInput rangeR_Input;

        RCCP_CarController vehicle;
        public RCCP_CarController RCCP_Vehicle { get { return vehicle; } }

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
            clutchInput = inputController?.GetInputByName(clutchInputName);

            gear1_Input = inputController?.GetInputByName(gear1InputName);
            gear2_Input = inputController?.GetInputByName(gear2InputName);
            gear3_Input = inputController?.GetInputByName(gear3InputName);
            gear4_Input = inputController?.GetInputByName(gear4InputName);

            level1_Input = inputController?.GetInputByName(level1InputName);
            level2_Input = inputController?.GetInputByName(level2InputName);

            range12_Input = inputController?.GetInputByName(range12InputName);
            range34_Input = inputController?.GetInputByName(range34InputName);
            rangeR_Input = inputController?.GetInputByName(rangeRInputName);

            FindVehicle();
            UseOverrides();

            tractorGearbox.Init(this);
            tractorMain.Init(this);
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

            if (clutchInput != null)
                vehicleInput.inputs.clutchInput = clutchInput.inputValue;

            if (gear1_Input != null)
                if (gear1_Input.inputValue != 0)
                    tractorGearbox?.ShiftToGear(gear1_Input.inputValue, 1);

            if (gear2_Input != null)
                if (gear2_Input.inputValue != 0)
                    tractorGearbox?.ShiftToGear(gear2_Input.inputValue, 2);

            if (gear3_Input != null)
                if (gear3_Input.inputValue != 0)
                    tractorGearbox?.ShiftToGear(gear3_Input.inputValue, 3);

            if (gear4_Input != null)
                if (gear4_Input.inputValue != 0)
                    tractorGearbox?.ShiftToGear(gear4_Input.inputValue, 4);

            if (level1_Input != null)
                if (level1_Input.inputValue != 0)
                    tractorGearbox?.ChangeGearLevel(true);

            if (level2_Input != null)
                if (level2_Input.inputValue != 0)
                    tractorGearbox?.ChangeGearLevel(false);

            if (range12_Input != null)
                if (range12_Input.inputValue != 0)
                    tractorGearbox?.ChangeGearRange(1);

            if (range34_Input != null)
                if (range34_Input.inputValue != 0)
                    tractorGearbox?.ChangeGearRange(2);

            if (rangeR_Input != null)
                if (rangeR_Input.inputValue != 0)
                    tractorGearbox?.ChangeGearRange(-1);
        }

        public void UseOverrides()
        {
            RCCP.SetExternalControl(vehicle, overrideInputs);
            vehicleInput.overridePlayerInputs = overrideInputs;
        }
    }
}
