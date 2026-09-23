using UnityEngine;

namespace Tractor
{
    public class TractorInput : MonoBehaviour
    {
        [Header("Руль")]
        [SerializeField] string steerInputName;
        CustomInput steer_Input;

        [Header("Педали")]
        [SerializeField] string throttleInputName;
        CustomInput throttle_Input;

        [SerializeField] string brakeInputName;
        CustomInput brake_Input;

        [SerializeField] string clutchInputName;
        CustomInput clutch_Input;

        [Header("Ручник")]
        [SerializeField] string handbrakeInputName;
        CustomInput handbrake_Input;

        [Header("Передачи")]
        [SerializeField] string gearNInputName;
        CustomInput gearN_Input;
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

        [Header("Зажигание")]
        [SerializeField] string massInputName;
        CustomInput mass_Input;

        [SerializeField] string massOnInputName;
        CustomInput massOn_Input;

        [SerializeField] string massOffInputName;
        CustomInput massOff_Input;

        [SerializeField] string starterInputName;
        CustomInput starter_Input;

        [SerializeField] string starterOnInputName;
        CustomInput starterOn_Input;

        [SerializeField] string starterOffInputName;
        CustomInput starterOff_Input;

        [SerializeField] string ignitionInputName;
        CustomInput ignition_Input;

        [Header("Лампочки и фары")]
        [SerializeField] string turnLeftInputName;
        CustomInput turnLeft_Input;

        [SerializeField] string turnRightInputName;
        CustomInput turnRight_Input;
        [SerializeField] string alarmInputName;
        CustomInput alarm_Input;

        [Header("Звуки")]
        [SerializeField] string hornSignalInputName;
        CustomInput hornSignal_Input;

        RCCP_CarController vehicle;
        public RCCP_CarController RCCP_Vehicle { get { return vehicle; } }

        RCCP_Input vehicleInput;

        InputController inputController;
        bool overrideInputs = false;

        TractorController tractorMain;
        TractorGearbox tractorGearbox;
        TractorEngine tractorEngine;
        TractorLights tractorLights;
        TractorAudio tractorAudio;

        void Start()
        {
            inputController = InputController.Instance;
            overrideInputs = inputController.overrideInputs;

            steer_Input = inputController?.GetInputByName(steerInputName);
            throttle_Input = inputController?.GetInputByName(throttleInputName);
            brake_Input = inputController?.GetInputByName(brakeInputName);
            clutch_Input = inputController?.GetInputByName(clutchInputName);
            handbrake_Input = inputController?.GetInputByName(handbrakeInputName);

            gearN_Input = inputController?.GetInputByName(gearNInputName);
            gear1_Input = inputController?.GetInputByName(gear1InputName);
            gear2_Input = inputController?.GetInputByName(gear2InputName);
            gear3_Input = inputController?.GetInputByName(gear3InputName);
            gear4_Input = inputController?.GetInputByName(gear4InputName);

            level1_Input = inputController?.GetInputByName(level1InputName);
            level2_Input = inputController?.GetInputByName(level2InputName);

            range12_Input = inputController?.GetInputByName(range12InputName);
            range34_Input = inputController?.GetInputByName(range34InputName);
            rangeR_Input = inputController?.GetInputByName(rangeRInputName);

            mass_Input = inputController?.GetInputByName(massInputName);
            massOn_Input = inputController?.GetInputByName(massOnInputName);
            massOff_Input = inputController?.GetInputByName(massOffInputName);
            starter_Input = inputController?.GetInputByName(starterInputName);
            starterOn_Input = inputController?.GetInputByName(starterOnInputName);
            starterOff_Input = inputController?.GetInputByName(starterOffInputName);
            ignition_Input = inputController?.GetInputByName(ignitionInputName);

            turnLeft_Input = inputController?.GetInputByName(turnLeftInputName);
            turnRight_Input = inputController?.GetInputByName(turnRightInputName);
            alarm_Input = inputController?.GetInputByName(alarmInputName);

            hornSignal_Input = inputController?.GetInputByName(hornSignalInputName);
        }

        public void Init(TractorController INtractorMain, TractorGearbox INtractorGearbox, TractorEngine INtractorEngine,
            TractorLights INtractorLights, TractorAudio INtractorAudio)
        {
            if (INtractorMain == null || INtractorGearbox == null || INtractorEngine == null || INtractorLights == null || INtractorAudio == null)
                return;

            tractorMain = INtractorMain;
            tractorGearbox = INtractorGearbox;
            tractorEngine = INtractorEngine;
            tractorLights = INtractorLights;
            tractorAudio = INtractorAudio;

            FindVehicle();
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
            //Руль
            if (steer_Input != null)
                vehicleInput.inputs.steerInput = steer_Input.inputValue;

            //Педали
            if (throttle_Input != null)
                vehicleInput.inputs.throttleInput = throttle_Input.inputValue;

            if (brake_Input != null)
                vehicleInput.inputs.brakeInput = brake_Input.inputValue;

            if (clutch_Input != null)
                vehicleInput.inputs.clutchInput = clutch_Input.inputValue;

            //Ручник
            if (handbrake_Input != null)
            {
                if (handbrake_Input.inputValue != 0)
                    tractorMain.SetHandbrake(true);
                else
                    tractorMain.SetHandbrake(false);
            }

            //Передачи
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

            if (gearN_Input != null)
            {
                if (gearN_Input.inputValue != 0)
                    tractorGearbox?.SetNeutralGear();
            }
            else if (gear1_Input != null && gear2_Input != null && gear3_Input != null && gear4_Input != null)
            {
                if (gear1_Input.inputValue == 0 && gear2_Input.inputValue == 0 &&
                    gear3_Input.inputValue == 0 && gear4_Input.inputValue == 0)
                    tractorGearbox?.SetNeutralGear();
            }

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

            //Зажигание
            if (massOn_Input != null)
                if (massOn_Input.inputValue != 0)
                    tractorEngine.MassTurnOn();

            if (massOff_Input != null)
                if (massOff_Input.inputValue != 0)
                    tractorEngine.MassTurnOff();

            if (starterOn_Input != null)
                if (starterOn_Input.inputValue != 0)
                    tractorEngine.Starter = true;

            if (starterOff_Input != null)
                if (starterOff_Input.inputValue != 0)
                    tractorEngine.StarterTurnOff();

            if (ignition_Input != null)
            {
                if (ignition_Input.inputValue != 0)
                    tractorEngine.StarterTurnOn();
                else
                    tractorEngine.Ignition = false;
            }

            //Лампочки
            if (turnLeft_Input != null)
                if (turnLeft_Input.inputValue != 0)
                    tractorLights.TurnOnLeftTurnLight();

            if (turnRight_Input != null)
                if (turnRight_Input.inputValue != 0)
                    tractorLights.TurnOnRightTurnLight();

            if (turnLeft_Input != null && turnRight_Input != null)
                if (turnLeft_Input.inputValue == 0 && turnRight_Input.inputValue == 0)
                    tractorLights.TurnOffTurnLight();

            if (alarm_Input != null)
            {
                if (alarm_Input.inputValue != 0)
                    tractorLights.ActivateAlarmLights(true);
                else
                    tractorLights.ActivateAlarmLights(false);
            }

            //Звуки
            if (hornSignal_Input != null)
            {
                if (hornSignal_Input.inputValue != 0)
                    tractorAudio.ActivateHornSignal(true);
                else
                    tractorAudio.ActivateHornSignal(false);
            }
        }

        public void UseOverrides()
        {
            RCCP.SetExternalControl(vehicle, overrideInputs);
            vehicleInput.overridePlayerInputs = overrideInputs;
        }
    }
}
