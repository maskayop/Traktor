using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Tractor
{
    public class UI_RCCP_InputOverride : MonoBehaviour
    {
        [SerializeField] bool overrideInputs = false;

        [Header("Overrides")]
        [SerializeField] GameObject useOverridesButtonOn;
        [SerializeField] GameObject useOverridesButtonOff;

        [Header("Sliders Group Properties")]
        [SerializeField] CanvasGroup slidersCanvasGroup;
        [SerializeField] float slidersGroupDisabledAlpha = 0.2f;

        [Header("Steer Slider")]
        [SerializeField] string steerInputName;
        [SerializeField] Slider steerSlider;
        [SerializeField] TextMeshProUGUI steerSliderValueText;

        CustomInput steerInput;

        [Header("Throttle Slider")]
        [SerializeField] string throttleInputName;
        [SerializeField] Slider throttleSlider;
        [SerializeField] TextMeshProUGUI throttleSliderValueText;

        CustomInput throttleInput;

        [Header("Brake Slider")]
        [SerializeField] string brakeInputName;
        [SerializeField] Slider brakeSlider;
        [SerializeField] TextMeshProUGUI brakeSliderValueText;

        CustomInput brakeInput;

        [Header("Info")]
        [SerializeField] TextMeshProUGUI infoText;

        InputController inputController;

        void Start()
        {
            inputController = InputController.Instance;

            steerInput = inputController?.GetInputByName(steerInputName);
            throttleInput = inputController?.GetInputByName(throttleInputName);
            brakeInput = inputController?.GetInputByName(brakeInputName);

            overrideInputs = inputController.overrideInputs;

            UseOverrides(overrideInputs);
        }

        void Update()
        {
            SetInputs();
        }

        void SetInputs()
        {
            if (steerInput != null)
                SetSliderValues(steerSlider, steerSliderValueText, steerInput.inputValue);

            if (throttleInput != null)
                SetSliderValues(throttleSlider, throttleSliderValueText, throttleInput.inputValue);

            if (brakeInput != null)
                SetSliderValues(brakeSlider, brakeSliderValueText, brakeInput.inputValue);
        }

        void SetSliderValues(Slider INslider, TextMeshProUGUI INsliderValueText, float INvalue)
        {
            INslider.value = INvalue;
            INsliderValueText.text = INvalue.ToString("F4");
        }

        public void UseOverrides(bool state)
        {
            overrideInputs = state;
            inputController.overrideInputs = overrideInputs;

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
    }
}
