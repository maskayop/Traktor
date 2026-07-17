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

        RCCP_CarController vehicle;
        RCCP_Input vehicleInput;

        void Start()
        {
            vehicle = FindAnyObjectByType<RCCP_CarController>();
            vehicleInput = vehicle.Inputs;

            // Tell RCCP_Input to stop reading from keyboard/gamepad
            if (overrideInputs)
                vehicleInput.overridePlayerInputs = true;

            UseOverrides(overrideInputs);
        }

        void Update()
        {
            if (!vehicleInput)
                return;

            if (!overrideInputs)
                return;

            SetInputs();
        }

        void OnDisable()
        {
            // Always restore normal input when done
            if (vehicleInput)
                vehicleInput.overridePlayerInputs = false;
        }

        void SetInputs()
        {
            // Write your desired values every frame
            //vehicleInput.inputs.throttleInput = 0.8f;  // 80% throttle
            //vehicleInput.inputs.steerInput = -0.5f;     // Turn left
            //vehicleInput.inputs.brakeInput = 0f;         // No brake

            vehicleInput.inputs.steerInput = steerSlider.value;
            steerSliderValueText.text = steerSlider.value.ToString("F4");

            vehicleInput.inputs.throttleInput = throttleSlider.value;
            throttleSliderValueText.text = throttleSlider.value.ToString("F4");

            vehicleInput.inputs.brakeInput = brakeSlider.value;
            brakeSliderValueText.text = brakeSlider.value.ToString("F4");
        }

        public void UseOverrides(bool state)
        {
            overrideInputs = state;

            useOverridesButtonOn.SetActive(state);
            useOverridesButtonOff.SetActive(!state);

            if (state)
                slidersCanvasGroup.alpha = 1.0f;
            else
                slidersCanvasGroup.alpha = slidersGroupDisabledAlpha;
        }
    }
}
