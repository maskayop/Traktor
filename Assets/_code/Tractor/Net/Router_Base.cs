using System.Globalization;
using Tractor.UI;
using UnityEngine;

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

        void Start()
        {
            server = PCServer.Instance;
            client = TabletClient.Instance;
            testCanvas = UITestCanvas.Instance;
            mainCanvas = UIMainCanvas.Instance;
            exerciseWindow = UIExerciseWindow.Instance;
            timeOfDaySettingsWindow = UITimeOfDaySettingsWindow.Instance;
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
                case "OpenReportsWindow":
                    mainCanvas?.OpenReportsWindow();
                    break;
                case "CloseReportsWindow":
                    mainCanvas?.CloseReportsWindow();
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

                case "EXERCISE":
                    OnExerciseReceived(param);
                    break;
                case "SELECT_EXERCISE":
                    exerciseWindow?.SelectExerciseByName(param);
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
        public void OnOpenReportsWindow() { SendCommand("OpenReportsWindow"); }
        public void OnCloseReportsWindow() { SendCommand("CloseReportsWindow"); }
        public void OnOpenExitToMainMenuWindow() { SendCommand("OpenExitToMainMenuWindow"); }
        public void OnCloseExitToMainMenuWindow() { SendCommand("CloseExitToMainMenuWindow"); }

        // Обёртки Exercise UI
        public void OnOpenExerciseForceExitWindow() { SendCommand("OpenExerciseForceExitWindow"); }
        public void OnCloseExerciseForceExitWindow() { SendCommand("CloseExerciseForceExitWindow"); }
        public void OnOpenExercisePreparingWindow() { SendCommand("OpenExercisePreparingWindow"); }
        public void OnCloseExercisePreparingWindow() { SendCommand("CloseExercisePreparingWindow"); }
        public void OnStartExercise() { SendCommand("StartExercise"); }
        public void OnExerciseForceExit() { SendCommand("ExerciseForceExit"); }

        // Обёртки Time of Day UI
        public void SetDayTimeMorning() { SendCommand("SetDayTimeMorning"); }
        public void SetDayTimeDay() { SendCommand("SetDayTimeDay"); }
        public void SetDayTimeEvening() { SendCommand("SetDayTimeEvening"); }
        public void SetDayTimeNight() { SendCommand("SetDayTimeNight"); }
        public void EnableDynamicTime() { SendCommand("EnableDynamicTime"); }
        public void DisableDynamicTime() { SendCommand("DisableDynamicTime"); }
        public void SendSetHour(float hour) => SendWithParam("SetHour", hour);

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
    }
}
