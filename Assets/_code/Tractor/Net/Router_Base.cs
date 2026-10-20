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

        void Start()
        {
            server = PCServer.Instance;
            client = TabletClient.Instance;
            testCanvas = UITestCanvas.Instance;
            mainCanvas = UIMainCanvas.Instance;
        }

        public void Route(string command)
        {
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
                //
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
                case "OpenExerciseForceExitWindow":
                    mainCanvas?.OpenExerciseForceExitWindow();
                    break;
                case "CloseExerciseForceExitWindow":
                    mainCanvas?.CloseExerciseForceExitWindow();
                    break;
                default:
                    Debug.LogWarning($"Неизвестная команда: {command}");
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

        // Тестовые обёртки
        public void OnTestOpenPC() { SendCommand("TestOpenPC"); }
        public void OnTestClosePC() { SendCommand("TestClosePC"); }
        public void OnTestOpenTablet() { SendCommand("TestOpenTablet"); }
        public void OnTestCloseTablet() { SendCommand("TestCloseTablet"); }

        // Рабочие обёртки для кнопок
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
        public void OnOpenExerciseForceExitWindow() { SendCommand("OpenExerciseForceExitWindow"); }
        public void OnCloseExerciseForceExitWindow() { SendCommand("CloseExerciseForceExitWindow"); }
    }
}
