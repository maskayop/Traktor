using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Tractor
{
    public class RCCP_InputOverride : MonoBehaviour
    {
        [SerializeField] bool overrideInputs = false;

        [Header("Overrides")]
        [SerializeField] GameObject useOverridesButtonOn;
        [SerializeField] GameObject useOverridesButtonOff;

        [Header("Sliders")]
        [SerializeField] CanvasGroup slidersCanvasGroup;
        [SerializeField] float slidersGroupDisabledAlpha = 0.2f;

        [Header("Steer Slider")]
        [SerializeField] Slider steerSlider;
        [SerializeField] TextMeshProUGUI steerSliderValueText;

        [Header("Throttle Slider")]
        [SerializeField] Slider throttleSlider;
        [SerializeField] TextMeshProUGUI throttleSliderValueText;

        [Header("Brake Slider")]
        [SerializeField] Slider brakeSlider;
        [SerializeField] TextMeshProUGUI brakeSliderValueText;

        [Header("Info")]
        [SerializeField] TextMeshProUGUI infoText;

        public RCCP_CarController vehicle;
        public RCCP_Input vehicleInput;

        void Start()
        {
            FindVehicle();
            UseOverrides(overrideInputs);
        }

        void Update()
        {
            ShowInfo();

            if (!vehicleInput || !overrideInputs)
                return;

            SetInputs();
        }

        void OnDisable()
        {
            // Always restore normal input when done
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
            vehicleInput.inputs.steerInput = steerSlider.value;
            steerSliderValueText.text = steerSlider.value.ToString("F4");

            vehicleInput.inputs.throttleInput = throttleSlider.value;
            throttleSliderValueText.text = throttleSlider.value.ToString("F4");

            vehicleInput.inputs.brakeInput = brakeSlider.value;
            brakeSliderValueText.text = brakeSlider.value.ToString("F4");
        }

        public void UseOverrides(bool state)
        {
            FindVehicle();

            overrideInputs = state;

            RCCP.SetExternalControl(vehicle, overrideInputs);
            vehicleInput.overridePlayerInputs = overrideInputs;

            useOverridesButtonOn.SetActive(state);
            useOverridesButtonOff.SetActive(!state);

            if (state)
            {
                slidersCanvasGroup.alpha = 1.0f;
                slidersCanvasGroup.interactable = true;
            }
            else
            {
                slidersCanvasGroup.alpha = slidersGroupDisabledAlpha;
                slidersCanvasGroup.interactable = false;
            }
        }

        void ShowInfo()
        {
            if (!infoText)
                return;

            infoText.text = "";
            infoText.text += "RCCP_CarController vehicle = " + vehicle + "\n";
            infoText.text += "Vehicle Position = " + vehicle.transform.position + "\n";
            infoText.text += "RCCP_Input vehicleInput = " + vehicleInput + "\n";
            infoText.text += "overrideInputs = " + overrideInputs + "\n";
            infoText.text += "RCCP_Input overridePlayerInputs = " + vehicleInput.overridePlayerInputs + "\n";
        }
    }
}
