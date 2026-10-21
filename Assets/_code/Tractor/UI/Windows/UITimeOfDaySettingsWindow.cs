using Enviro;
using TMPro;
using Tractor.Net;
using UnityEngine;
using UnityEngine.UI;
using static Tractor.UI.UIMainCanvas;

namespace Tractor.UI
{
    public sealed class UITimeOfDaySettingsWindow : UI_WindowBase
    {
        public static UITimeOfDaySettingsWindow Instance;

        [Header("Общие настройки")]
        [SerializeField] Slider hourSlider;
        [SerializeField] TextMeshProUGUI timeLabel;
        [SerializeField] Toggle dynamicTimeToggle;
        [SerializeField] bool simulateTime = true;
        [SerializeField] bool useCloudyDemoWeather = true;
        [SerializeField, Range(0f, 24f)] float fallbackStartHour = 8f;

        [Header("Общие настройки")]
        [SerializeField] int morningHour = 8;
        [SerializeField] int dayHour = 13;
        [SerializeField] int eveningHour = 19;
        [SerializeField] int nightHour = 0;

        bool isSynchronizing;

        CanvasPlatform platform = CanvasPlatform.Windows;
        RouterPC routerPC;
        RouterTablet routerTablet;

        protected override void OnAwake()
        {
            if (Instance != null)
            {
                Debug.LogWarning("Cannot create UITimeOfDaySettingsWindow");
                Destroy(gameObject);
                return;
            }

            Instance = this;
        }

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

        protected override void OnInit()
        {
            if (!mainCanvas)
                return;

            platform = mainCanvas.platform;

            if (platform == CanvasPlatform.Windows)
                routerPC = RouterPC.Instance;
            else
                routerTablet = RouterTablet.Instance;
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

        // Set Time
        public void OnSetMorning()
        {
            SetMorning();

            if (platform == CanvasPlatform.Windows)
                routerPC?.SetDayTimeMorning();
            else
                routerTablet?.SetDayTimeMorning();
        }

        public void SetMorning() => ApplyHour(morningHour);

        public void OnSetDay()
        {
            SetDay();

            if (platform == CanvasPlatform.Windows)
                routerPC?.SetDayTimeDay();
            else
                routerTablet?.SetDayTimeDay();
        }

        public void SetDay() => ApplyHour(dayHour);

        public void OnSetEvening()
        {
            SetEvening();

            if (platform == CanvasPlatform.Windows)
                routerPC?.SetDayTimeEvening();
            else
                routerTablet?.SetDayTimeEvening();
        }

        public void SetEvening() => ApplyHour(eveningHour);

        public void OnSetNight()
        {
            SetNight();

            if (platform == CanvasPlatform.Windows)
                routerPC?.SetDayTimeNight();
            else
                routerTablet?.SetDayTimeNight();
        }

        public void SetNight() => ApplyHour(nightHour);

        // Set Dynamic Time
        public void OnSetDynamicTime()
        {
            if (!dynamicTimeToggle) return;

            bool state = dynamicTimeToggle.isOn;

            SetDynamicTime(state);

            if (platform == CanvasPlatform.Windows)
            {
                if (state) routerPC?.EnableDynamicTime();
                else routerPC?.DisableDynamicTime();
            }
            else
            {
                if (state) routerTablet?.EnableDynamicTime();
                else routerTablet?.DisableDynamicTime();
            }
        }
        public void SetDynamicTime(bool state)
        {
            if (dynamicTimeToggle)
                dynamicTimeToggle.SetIsOnWithoutNotify(state);

            if (!TryGetEnviroTimeModule(out var timeModule))
                return;

            timeModule.Settings.simulate = state;
        }
    }
}
