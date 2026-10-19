using Enviro;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Tractor.UI
{
    public sealed class UITimeOfDaySettingsWindow : UI_WindowBase
    {
        [Header("Настройки")]
        [SerializeField] Slider hourSlider;
        [SerializeField] TextMeshProUGUI timeLabel;
        [SerializeField] Toggle dynamicTimeToggle;
        [SerializeField] bool simulateTime = true;
        [SerializeField] bool useCloudyDemoWeather = true;
        [SerializeField, Range(0f, 24f)] float fallbackStartHour = 8f;

        bool isSynchronizing;

        protected override void OnStart()
        {
            if (!TryGetEnviroTimeModule(out var timeModule))
                return;

            timeModule.Settings.simulate = simulateTime;

            if (dynamicTimeToggle)
                dynamicTimeToggle.SetIsOnWithoutNotify(timeModule.Settings.simulate);

            if (useCloudyDemoWeather && EnviroManager.instance.Weather)
                EnviroManager.instance.Weather.ChangeWeatherInstant("Cloudy 1");

            var initialHour = timeModule.GetTimeOfDay();
            if (float.IsNaN(initialHour) || initialHour < 0f || initialHour > 24f)
                initialHour = fallbackStartHour;

            ApplyHour(initialHour);
        }

        protected override void OnUpdate()
        {
            if (!TryGetEnviroTimeModule(out var timeModule))
                return;

            UpdateVisuals(timeModule.GetTimeOfDay());
        }

        public void SetHour(float hour)
        {
            ApplyHour(hour);
        }

        public void OnHourSliderChanged(float hour)
        {
            if (!isSynchronizing)
                ApplyHour(hour);
        }

        public void SetMorning() => ApplyHour(8f);
        public void SetDay() => ApplyHour(13f);
        public void SetEvening() => ApplyHour(19f);
        public void SetNight() => ApplyHour(0f);

        public void SetDynamicTime(bool enabled)
        {
            if (!TryGetEnviroTimeModule(out var timeModule))
                return;

            if (!dynamicTimeToggle)
                return;

            dynamicTimeToggle.SetIsOnWithoutNotify(enabled);
            timeModule.Settings.simulate = dynamicTimeToggle.isOn;
        }

        void ApplyHour(float hour)
        {
            hour = Mathf.Clamp(hour, 0f, 24f);

            if (!TryGetEnviroTimeModule(out var timeModule))
                return;

            timeModule.SetTimeOfDay(hour);
            UpdateVisuals(hour);

            if (dynamicTimeToggle)
                timeModule.Settings.simulate = dynamicTimeToggle.isOn;
        }

        void UpdateVisuals(float hour)
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

        static bool TryGetEnviroTimeModule(out EnviroTimeModule timeModule)
        {
            timeModule = null;

            if (EnviroManager.instance == null || EnviroManager.instance.Time == null)
                return false;

            timeModule = EnviroManager.instance.Time;
            return true;
        }

        public void OnCloseTimeOfDaySettingsWindow()
        {
            mainCanvas?.OnCloseTimeOfDaySettingsWindow();
        }
    }
}
