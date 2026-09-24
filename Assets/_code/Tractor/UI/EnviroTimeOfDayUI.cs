using Enviro;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace TractorSimulator.TimeOfDay
{
    /// <summary>
    /// Small, reusable UI bridge for Enviro 3.
    /// Attach it to a Canvas, assign a Slider and a TMP label, then call the
    /// preset methods from UI Buttons or SetHour from any gameplay script.
    /// </summary>
    public sealed class EnviroTimeOfDayUI : MonoBehaviour
    {
        [SerializeField] private Slider hourSlider;
        [SerializeField] private TMP_Text timeLabel;
        [SerializeField] private Toggle dynamicTimeToggle;
        [SerializeField] private bool stopAutomaticTime = true;
        [SerializeField] private bool useCloudyDemoWeather = true;
        [SerializeField, Range(0f, 24f)] private float fallbackStartHour = 8f;

        private bool isSynchronizing;

        private void Start()
        {
            if (!TryGetTimeModule(out var timeModule))
                return;

            if (stopAutomaticTime)
                timeModule.Settings.simulate = false;

            if (dynamicTimeToggle != null)
                dynamicTimeToggle.SetIsOnWithoutNotify(timeModule.Settings.simulate);

            // The supplied configuration starts with Clear Sky. For this demo we
            // explicitly select the package's Cloudy 1 preset so volumetric
            // clouds are visible and do not depend on saved PlayerPrefs/weather.
            if (useCloudyDemoWeather && EnviroManager.instance.Weather != null)
                EnviroManager.instance.Weather.ChangeWeatherInstant("Cloudy 1");

            var initialHour = timeModule.GetTimeOfDay();
            if (float.IsNaN(initialHour) || initialHour < 0f || initialHour > 24f)
                initialHour = fallbackStartHour;

            ApplyHour(initialHour);
        }

        private void Update()
        {
            if (!TryGetTimeModule(out var timeModule))
                return;

            UpdateVisuals(timeModule.GetTimeOfDay());
        }

        /// <summary>Sets an exact hour in the range 0..24.</summary>
        public void SetHour(float hour)
        {
            ApplyHour(hour);
        }

        /// <summary>Callback for Slider.onValueChanged.</summary>
        public void OnHourSliderChanged(float hour)
        {
            if (!isSynchronizing)
                ApplyHour(hour);
        }

        public void SetMorning() => ApplyHour(8f);
        public void SetDay() => ApplyHour(13f);
        public void SetEvening() => ApplyHour(19f);
        public void SetNight() => ApplyHour(0f);

        /// <summary>Enables or pauses Enviro's automatic time progression.</summary>
        public void SetDynamicTime(bool enabled)
        {
            if (!TryGetTimeModule(out var timeModule))
                return;

            if (!dynamicTimeToggle)
                return;

            dynamicTimeToggle.SetIsOnWithoutNotify(enabled);
            timeModule.Settings.simulate = dynamicTimeToggle.isOn;
        }

        private void ApplyHour(float hour)
        {
            hour = Mathf.Clamp(hour, 0f, 24f);

            if (!TryGetTimeModule(out var timeModule))
                return;

            timeModule.SetTimeOfDay(hour);
            UpdateVisuals(hour);

            if (dynamicTimeToggle)
                timeModule.Settings.simulate = dynamicTimeToggle.isOn;
        }

        private void UpdateVisuals(float hour)
        {
            isSynchronizing = true;

            if (hourSlider != null)
                hourSlider.SetValueWithoutNotify(hour);

            if (timeLabel != null)
            {
                var wrappedHour = Mathf.Repeat(hour, 24f);
                var h = Mathf.FloorToInt(wrappedHour);
                var m = Mathf.FloorToInt((wrappedHour - h) * 60f);
                timeLabel.text = $"{h:00}:{m:00}";
            }

            isSynchronizing = false;
        }

        private static int GetSelectedPreset(float hour)
        {
            var presets = new[] { 8f, 13f, 19f, 0f };
            for (var i = 0; i < presets.Length; i++)
            {
                var delta = Mathf.Abs(Mathf.DeltaAngle(hour * 15f, presets[i] * 15f)) / 15f;
                if (delta < 0.2f)
                    return i;
            }

            return -1;
        }

        private static bool TryGetTimeModule(out EnviroTimeModule timeModule)
        {
            timeModule = null;

            if (EnviroManager.instance == null || EnviroManager.instance.Time == null)
            {
                Debug.LogWarning("Enviro Time module is not available. Add an Enviro Manager with the Time module to the scene.");
                return false;
            }

            timeModule = EnviroManager.instance.Time;
            return true;
        }
    }
}
