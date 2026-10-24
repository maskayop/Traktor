using System.Collections.Generic;
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
        [SerializeField] Toggle dynamicWeatherToggle;
        [SerializeField] bool dynamicWeather = true;
        [SerializeField, Range(0f, 24f)] float fallbackStartHour = 8f;

        [Header("Общие настройки")]
        [SerializeField] int morningHour = 8;
        [SerializeField] int dayHour = 13;
        [SerializeField] int eveningHour = 19;
        [SerializeField] int nightHour = 0;

        [Header("Пресеты погоды")]
        [SerializeField] List<string> weatherPresets = new List<string>
        {
            "Cloudy 3",
            "Rain",
            "Foggy",
            "Clear Sky",
            "Snow"
        };

        int weatherIndex;
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

            if (dynamicWeatherToggle)
                dynamicWeatherToggle.SetIsOnWithoutNotify(dynamicWeather);

            if (useCloudyDemoWeather && EnviroManager.instance.Weather)
                SetCloudyWeather();

            var initialHour = timeModule.GetTimeOfDay();
            if (float.IsNaN(initialHour) || initialHour < 0f || initialHour > 24f)
                initialHour = fallbackStartHour;

            ApplyHour(initialHour);
            EnviroManager.instance.OnHourPassed += ChangeWeatherOnHour;
        }

        void OnDestroy()
        {
            // Отписываемся от события изменения времени
            if (EnviroManager.instance != null)
                EnviroManager.instance.OnHourPassed -= ChangeWeatherOnHour;
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
            if (isSynchronizing)
                return;

            ApplyHour(hour);
        }

        public void OnHourSliderReleased()
        {
            if (!hourSlider) return;

            float hour = Mathf.Clamp(hourSlider.value, 0f, 24f);

            if (platform == CanvasPlatform.Windows)
                routerPC?.SendSetHour(hour);
            else
                routerTablet?.SendSetHour(hour);
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

        #region Weather
        #region Weather Utilities
        static bool TryGetEnviroWeatherModule(out EnviroWeatherModule weatherModule)
        {
            weatherModule = null;

            if (EnviroManager.instance == null || EnviroManager.instance.Weather == null)
                return false;

            weatherModule = EnviroManager.instance.Weather;
            return true;
        }


        public void ApplyWeather(string weather, bool useInstant = false)
        {
            if (!TryGetEnviroWeatherModule(out var weatherModule))
                return;

            var weatherTypes = weatherModule.Settings?.weatherTypes;
            if (string.IsNullOrWhiteSpace(weather) || weatherTypes == null) 
                return;

            for (int i = 0; i < weatherTypes.Count; i++) {
                var candidate = weatherTypes[i];
                if (candidate != null && candidate.name == weather) {
                    if (useInstant)
                        weatherModule.ChangeWeatherInstant(candidate);
                    else
                        weatherModule.ChangeWeather(candidate);
                    return;
                }
            }

            Debug.LogWarning($"Weather {weather} not found");
        }

        void ApplyInstantWeatherAt(int index)
        {
            weatherIndex = index;
            ApplyWeather(weatherPresets[index], true);
        }
        #endregion

        #region Weather Events
        void ChangeWeatherOnHour()
        {
            if (dynamicWeatherToggle && !dynamicWeatherToggle.isOn) return;

            weatherIndex = Random.Range(0, weatherPresets.Count);
            var preset = weatherPresets[weatherIndex];

            if (preset == "Rain" || preset == "Snow") {
                if (TryGetEnviroEnvironmentModule(out var environmentModule)) {
                    if (environmentModule.Settings.season == EnviroEnvironment.Seasons.Winter) {
                        preset = "Snow";
                    } else {
                        preset = "Rain";
                    }
                }
            }

            ApplyWeather(preset);
        }

        public void OnSetCloudyWeather()
        {
            SetCloudyWeather();

            if (platform == CanvasPlatform.Windows)
                routerPC?.SetWeatherCloudy();
            else
                routerTablet?.SetWeatherCloudy();
        }  

        public void OnSetRainyWeather()
        {
            SetRainyWeather();

            if (platform == CanvasPlatform.Windows)
                routerPC?.SetWeatherRainy();
            else
                routerTablet?.SetWeatherRainy();
        }

        public void OnSetFoggyWeather()
        {
            SetFoggyWeather();

            if (platform == CanvasPlatform.Windows)
                routerPC?.SetWeatherFoggy();
            else
                routerTablet?.SetWeatherFoggy();
        }

        public void OnSetClearSkyWeather()
        {
            SetClearSkyWeather();

            if (platform == CanvasPlatform.Windows)
                routerPC?.SetWeatherClearSky();
            else
                routerTablet?.SetWeatherClearSky();
        }

        public void OnSetSnowyWeather()
        {
            SetSnowyWeather();

            if (platform == CanvasPlatform.Windows)
                routerPC?.SetWeatherSnowy();
            else
                routerTablet?.SetWeatherSnowy();
        }

        public void OnSetPrecipitationWeather()
        {
            if (!TryGetEnviroEnvironmentModule(out var environmentModule))
                return;
            if (environmentModule.Settings.season == EnviroEnvironment.Seasons.Winter)
                OnSetSnowyWeather();
            else
                OnSetRainyWeather();
        }

        public void OnSetDynamicWeather() {
            if (!dynamicWeatherToggle) return;

            bool state = dynamicWeatherToggle.isOn;
            SetDynamicWeather(state);

            if (platform == CanvasPlatform.Windows) {
                if (state) routerPC?.EnableDynamicWeather();
                else routerPC?.DisableDynamicWeather();
            }
            else {
                if (state) routerTablet?.EnableDynamicWeather();
                else routerTablet?.DisableDynamicWeather();
            }
        }
        #endregion

        public void SetCloudyWeather() => ApplyInstantWeatherAt(0);
        public void SetRainyWeather() => ApplyInstantWeatherAt(1);
        public void SetFoggyWeather() => ApplyInstantWeatherAt(2);
        public void SetClearSkyWeather() => ApplyInstantWeatherAt(3);
        public void SetSnowyWeather() => ApplyInstantWeatherAt(4);
        public void SetDynamicWeather(bool state) {
            if (dynamicWeatherToggle)
                dynamicWeatherToggle.SetIsOnWithoutNotify(state);
        }
        #endregion

        #region Seasons
        #region Seasons Utilities
        static bool TryGetEnviroEnvironmentModule(out EnviroEnvironmentModule environmentModule) {
            environmentModule = null;

            if (EnviroManager.instance == null || EnviroManager.instance.Environment == null)
                return false;

            environmentModule = EnviroManager.instance.Environment;
            return true;
        }

        void ApplySeason(EnviroEnvironment.Seasons season) {
            if (!TryGetEnviroEnvironmentModule(out var environmentModule)) return;

            environmentModule.Settings.changeSeason = false;
            environmentModule.ChangeSeason(season);
            UpdatePrecipitationForSeason();
        }

        void UpdatePrecipitationForSeason()
        {
            if (!TryGetEnviroWeatherModule(out var weatherModule))
                return;
            var current = weatherModule.targetWeatherType;
            if (current == null || (current.name != "Rain" && current.name != "Snow"))
                return;
            OnSetPrecipitationWeather();
        }
        #endregion

        #region Seasons Events
        public void OnSetSpring() {
            SetSpring();

            if (platform == CanvasPlatform.Windows)
                routerPC?.SetSeasonSpring();
            else
                routerTablet?.SetSeasonSpring();
        }

        public void OnSetSummer() {
            SetSummer();

            if (platform == CanvasPlatform.Windows)
                routerPC?.SetSeasonSummer();
            else
                routerTablet?.SetSeasonSummer();
        }
        
        public void OnSetAutumn() {
            SetAutumn();

            if (platform == CanvasPlatform.Windows)
                routerPC?.SetSeasonAutumn();
            else
                routerTablet?.SetSeasonAutumn();
        }

        public void OnSetWinter() {
            SetWinter();

            if (platform == CanvasPlatform.Windows)
                routerPC?.SetSeasonWinter();
            else
                routerTablet?.SetSeasonWinter();
        }
        #endregion

        public void SetSpring() => ApplySeason(EnviroEnvironment.Seasons.Spring);
        public void SetSummer() => ApplySeason(EnviroEnvironment.Seasons.Summer);
        public void SetAutumn() => ApplySeason(EnviroEnvironment.Seasons.Autumn);
        public void SetWinter() => ApplySeason(EnviroEnvironment.Seasons.Winter);
        #endregion
    }
}
