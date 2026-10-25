using System.Globalization;
using Tractor.UI;
using UnityEngine;
using Vopere;

namespace Tractor.Net
{
    public class Router_Base : MonoBehaviour
    {
        public bool isPC = true;
        public bool destroyOnWrongPlatform = true;

        PCServer server;
        TabletClient client;

        UITestCanvas testCanvas;
        UIMainCanvas mainCanvas;
        UIExerciseWindow exerciseWindow;
        UITimeOfDaySettingsWindow timeOfDaySettingsWindow;
        UIReportsWindow reportsWindow;

        void Start()
        {
            server = PCServer.Instance;
            client = TabletClient.Instance;
            testCanvas = UITestCanvas.Instance;
            mainCanvas = UIMainCanvas.Instance;
            exerciseWindow = UIExerciseWindow.Instance;
            timeOfDaySettingsWindow = UITimeOfDaySettingsWindow.Instance;
            reportsWindow = UIReportsWindow.Instance;
        }

        public void Route(string command)
        {
            int colonIndex = command.IndexOf(':');
            if (colonIndex > 0)
            {
                string cmd = command.Substring(0, colonIndex);
                string param = command.Substring(colonIndex + 1);
                RouteWithParam(cmd, param);
                return;
            }

            switch (command)
            {
                // Test
                case "TestOpenPC":
                    testCanvas?.TestOpenPC();
                    break;
                case "TestClosePC":
                    testCanvas?.TestClosePC();
                    break;
                case "TestOpenTablet":
                    testCanvas?.TestOpenTablet();
                    break;
                case "TestCloseTablet":
                    testCanvas?.TestCloseTablet();
                    break;

                // Base UI
                case "ExitGame":
                    mainCanvas?.ExitGame();
                    break;
                case "OpenSettingsWindow":
                    mainCanvas?.OpenSettingsWindow();
                    break;
                case "CloseSettingsWindow":
                    mainCanvas?.CloseSettingsWindow();
                    break;
                case "OpenControlsWindow":
                    mainCanvas?.OpenControlsWindow();
                    break;
                case "CloseControlsWindow":
                    mainCanvas?.CloseControlsWindow();
                    break;
                case "StartGame":
                    mainCanvas?.StartGame();
                    break;
                case "EndGame":
                    mainCanvas?.EndGame();
                    break;
                case "OpenExitGameWindow":
                    mainCanvas?.OpenExitGameWindow();
                    break;
                case "CloseExitGameWindow":
                    mainCanvas?.CloseExitGameWindow();
                    break;
                case "OpenTimeOfDaySettingsWindow":
                    mainCanvas?.OpenTimeOfDaySettingsWindow();
                    break;
                case "CloseTimeOfDaySettingsWindow":
                    mainCanvas?.CloseTimeOfDaySettingsWindow();
                    break;
                case "OpenExitToMainMenuWindow":
                    mainCanvas?.OpenExitToMainMenuWindow();
                    break;
                case "CloseExitToMainMenuWindow":
                    mainCanvas?.CloseExitToMainMenuWindow();
                    break;

                // Exercise UI
                case "OpenExerciseForceExitWindow":
                    exerciseWindow?.OpenExerciseForceExitWindow();
                    break;
                case "CloseExerciseForceExitWindow":
                    exerciseWindow?.CloseExerciseForceExitWindow();
                    break;
                case "OpenExercisePreparingWindow":
                    exerciseWindow?.OpenExercisePreparingWindow();
                    break;
                case "CloseExercisePreparingWindow":
                    exerciseWindow?.CloseExercisePreparingWindow();
                    break;
                case "StartExercise":
                    exerciseWindow?.StartExercise();
                    break;
                case "ExerciseForceExit":
                    exerciseWindow?.ExerciseForceExit();
                    break;
                case "CurrentExerciseWindowCollapse":
                    exerciseWindow?.CurrentExerciseWindowCollapse();
                    break;
                case "CurrentExerciseWindowExpand":
                    exerciseWindow?.CurrentExerciseWindowExpand();
                    break;
                case "OpenExerciseCompleteWindow":
                    exerciseWindow?.OpenExerciseCompleteWindow();
                    break;
                case "CloseExerciseCompleteWindow":
                    exerciseWindow?.CloseExerciseCompleteWindow();
                    break;

                // Time of Day UI
                case "SetDayTimeMorning":
                    timeOfDaySettingsWindow?.SetMorning();
                    break;
                case "SetDayTimeDay":
                    timeOfDaySettingsWindow?.SetDay();
                    break;
                case "SetDayTimeEvening":
                    timeOfDaySettingsWindow?.SetEvening();
                    break;
                case "SetDayTimeNight":
                    timeOfDaySettingsWindow?.SetNight();
                    break;
                case "EnableDynamicTime":
                    timeOfDaySettingsWindow?.SetDynamicTime(true);
                    break;
                case "DisableDynamicTime":
                    timeOfDaySettingsWindow?.SetDynamicTime(false);
                    break;

                // Weather UI
                case "SetWeatherCloudy":
                    timeOfDaySettingsWindow?.SetCloudyWeather();
                    break;
                case "SetWeatherRainy":
                    timeOfDaySettingsWindow?.SetRainyWeather();
                    break;
                case "SetWeatherFoggy":
                    timeOfDaySettingsWindow?.SetFoggyWeather();
                    break;
                case "SetWeatherClearSky":
                    timeOfDaySettingsWindow?.SetClearSkyWeather();
                    break;
                case "SetWeatherSnowy":
                    timeOfDaySettingsWindow?.SetSnowyWeather();
                    break;
                case "EnableDynamicWeather":
                    timeOfDaySettingsWindow?.SetDynamicWeather(true);
                    break;
                case "DisableDynamicWeather":
                    timeOfDaySettingsWindow?.SetDynamicWeather(false);
                    break;

                // Seasons UI
                case "SetSeasonSpring":
                    timeOfDaySettingsWindow?.SetSpring();
                    break;
                case "SetSeasonSummer":
                    timeOfDaySettingsWindow?.SetSummer();
                    break;
                case "SetSeasonAutumn":
                    timeOfDaySettingsWindow?.SetAutumn();
                    break;
                case "SetSeasonWinter":
                    timeOfDaySettingsWindow?.SetWinter();
                    break;

                // Reports UI
                case "OpenReportsWindow":
                    mainCanvas?.OpenReportsWindow();
                    break;
                case "CloseReportsWindow":
                    mainCanvas?.CloseReportsWindow();
                    break;
                case "OpenSelectedReportWindow":
                    reportsWindow?.OpenSelectedReportWindow();
                    break;
                case "CloseSelectedReportWindow":
                    reportsWindow?.CloseSelectedReportWindow();
                    break;
                case "REQ_REPORTS":
                    reportsWindow?.LoadAllReports();
                    break;
                case "SaveExerciseReport":
                    reportsWindow?.SaveExerciseReport();
                    break;

                default:
                    Debug.LogWarning($"Неизвестная команда: {command}");
                    break;
            }
        }

        void RouteWithParam(string command, string param)
        {
            switch (command)
            {
                case "SetHour":
                    if (TryParseFloat(param, out float hour))
                        timeOfDaySettingsWindow?.SetHour(hour);
                    else
                        Debug.LogWarning($"Не удалось разобрать SetHour: {param}");
                    break;

                // Exercises
                case "EXERCISE":
                    OnExerciseReceived(param);
                    break;
                case "SELECT_EXERCISE":
                    exerciseWindow?.SelectExerciseByName(param);
                    break;

                // Reports
                case "REPORT":
                    OnReportReceived(param);
                    break;
                case "SELECT_REPORT":
                    reportsWindow?.SelectReportByName(param);
                    break;

                default:
                    Debug.LogWarning($"Неизвестная команда с параметром: {command}");
                    break;
            }
        }

        void SendCommand(string command)
        {
            if (isPC)
                server?.SendCommand(command);
            else
                client?.SendCommand(command);
        }

        protected void SendWithParam(string command, string param)
        {
            SendCommand(command + ":" + param);
        }

        protected void SendWithParam(string command, float param)
        {
            SendWithParam(command, param.ToString(CultureInfo.InvariantCulture));
        }

        protected void SendWithParam(string command, int param)
        {
            SendWithParam(command, param.ToString(CultureInfo.InvariantCulture));
        }

        static bool TryParseFloat(string s, out float value)
        {
            return float.TryParse(s, NumberStyles.Float, CultureInfo.InvariantCulture, out value);
        }

        // Тестовые обёртки
        public void OnTestOpenPC() { SendCommand("TestOpenPC"); }
        public void OnTestClosePC() { SendCommand("TestClosePC"); }
        public void OnTestOpenTablet() { SendCommand("TestOpenTablet"); }
        public void OnTestCloseTablet() { SendCommand("TestCloseTablet"); }

        // Базовые обёртки кнопок UI
        public void OnExitGame() { SendCommand("ExitGame"); }
        public void OnOpenSettingsWindow() { SendCommand("OpenSettingsWindow"); }
        public void OnCloseSettingsWindow() { SendCommand("CloseSettingsWindow"); }
        public void OnOpenControlsWindow() { SendCommand("OpenControlsWindow"); }
        public void OnCloseControlsWindow() { SendCommand("CloseControlsWindow"); }
        public void OnStartGame() { SendCommand("StartGame"); }
        public void OnEndGame() { SendCommand("EndGame"); }
        public void OnOpenExitGameWindow() { SendCommand("OpenExitGameWindow"); }
        public void OnCloseExitGameWindow() { SendCommand("CloseExitGameWindow"); }
        public void OnOpenTimeOfDaySettingsWindow() { SendCommand("OpenTimeOfDaySettingsWindow"); }
        public void OnCloseTimeOfDaySettingsWindow() { SendCommand("CloseTimeOfDaySettingsWindow"); }
        public void OnOpenExitToMainMenuWindow() { SendCommand("OpenExitToMainMenuWindow"); }
        public void OnCloseExitToMainMenuWindow() { SendCommand("CloseExitToMainMenuWindow"); }

        // Обёртки Exercise UI
        public void OnOpenExerciseForceExitWindow() { SendCommand("OpenExerciseForceExitWindow"); }
        public void OnCloseExerciseForceExitWindow() { SendCommand("CloseExerciseForceExitWindow"); }
        public void OnOpenExercisePreparingWindow() { SendCommand("OpenExercisePreparingWindow"); }
        public void OnCloseExercisePreparingWindow() { SendCommand("CloseExercisePreparingWindow"); }
        public void OnStartExercise() { SendCommand("StartExercise"); }
        public void OnExerciseForceExit() { SendCommand("ExerciseForceExit"); }
        public void OnCurrentExerciseWindowCollapse() { SendCommand("CurrentExerciseWindowCollapse"); }
        public void OnCurrentExerciseWindowExpand() { SendCommand("CurrentExerciseWindowExpand"); }
        public void OnOpenExerciseCompleteWindow() { SendCommand("OpenExerciseCompleteWindow"); }
        public void OnCloseExerciseCompleteWindow() { SendCommand("CloseExerciseCompleteWindow"); }

        // Обёртки Reports UI
        public void OnOpenReportsWindow() { SendCommand("OpenReportsWindow"); }
        public void OnCloseReportsWindow() { SendCommand("CloseReportsWindow"); }
        public void OnOpenSelectedReportWindow() { SendCommand("OpenSelectedReportWindow"); }
        public void OnCloseSelectedReportWindow() { SendCommand("CloseSelectedReportWindow"); }
        public void OnSaveExerciseReport() { SendCommand("SaveExerciseReport"); }

        // Обёртки Time of Day UI
        public void SetDayTimeMorning() { SendCommand("SetDayTimeMorning"); }
        public void SetDayTimeDay() { SendCommand("SetDayTimeDay"); }
        public void SetDayTimeEvening() { SendCommand("SetDayTimeEvening"); }
        public void SetDayTimeNight() { SendCommand("SetDayTimeNight"); }
        public void EnableDynamicTime() { SendCommand("EnableDynamicTime"); }
        public void DisableDynamicTime() { SendCommand("DisableDynamicTime"); }
        public void SendSetHour(float hour) => SendWithParam("SetHour", hour);

        // Обёртки Weather UI
        public void SetWeatherCloudy() { SendCommand("SetWeatherCloudy"); }
        public void SetWeatherRainy() { SendCommand("SetWeatherRainy"); }
        public void SetWeatherFoggy() { SendCommand("SetWeatherFoggy"); }
        public void SetWeatherClearSky() { SendCommand("SetWeatherClearSky"); }
        public void SetWeatherSnowy() { SendCommand("SetWeatherSnowy"); }
        public void EnableDynamicWeather() { SendCommand("EnableDynamicWeather"); }
        public void DisableDynamicWeather() { SendCommand("DisableDynamicWeather"); }

        // Обёртки Seasons UI
        public void SetSeasonSpring() { SendCommand("SetSeasonSpring"); }
        public void SetSeasonSummer() { SendCommand("SetSeasonSummer"); }
        public void SetSeasonAutumn() { SendCommand("SetSeasonAutumn"); }
        public void SetSeasonWinter() { SendCommand("SetSeasonWinter"); }

        // Обёртки Exercises
        public void SendExercisesList()
        {
            var list = ExercisesController.Instance?.exercises;
            if (list == null || list.Count == 0) return;

            var pcServer = PCServer.Instance;
            if (!pcServer) return;

            foreach (var ex in list)
            {
                if (!ex) continue;
                pcServer.SendCommand("EXERCISE:" + ex.exerciseName.Replace(":", "-"));
            }
        }

        void OnExerciseReceived(string name)
        {
            if (isPC) return;
            exerciseWindow?.AddExerciseButton(name);
        }

        public void SendSelectExercise(string name)
        {
            SendWithParam("SELECT_EXERCISE", name);
        }

        // Reports
        public void RequestReports()
        {
            SendCommand("REQ_REPORTS");
        }

        public void SendReportsList()
        {
            var list = ReportController.Instance?.GetAllReports();
            if (list == null || list.Count == 0) return;

            var pcServer = PCServer.Instance;
            if (!pcServer) return;

            foreach (var r in list)
            {
                if (r == null || string.IsNullOrEmpty(r.fileName)) continue;
                pcServer.SendCommand("REPORT:" + r.fileName.Replace(":", "-"));
            }
        }

        void OnReportReceived(string fileName)
        {
            if (isPC) return;
            reportsWindow?.AddReportButton(fileName);
        }

        public void SendSelectReport(string fileName)
        {
            SendWithParam("SELECT_REPORT", fileName);
        }
    }
}
