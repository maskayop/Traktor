using System.Collections.Generic;
using TMPro;
using Tractor.Net;
using UnityEngine;
using Vopere.Common;
using static Tractor.GameController;

namespace Tractor.UI
{
    public class UIMainCanvas : MonoBehaviour
    {
        public static UIMainCanvas Instance;

        public enum CanvasPlatform { Windows, Android }
        public CanvasPlatform platform = CanvasPlatform.Windows;

        [Header("Windows")]
        [SerializeField] GameObject HUDWindow;
        [SerializeField] GameObject mainMenuWindow;
        [SerializeField] GameObject mainMenuPanel;
        [SerializeField] UITimeOfDaySettingsWindow timeOfDaySettingsWindow;
        [SerializeField] UIControlsWindow controlsWindow;
        [SerializeField] UISettingsWindow settingsWindow;
        [SerializeField] UIReportsWindow reportsWindow;
        [SerializeField] UIExitWindow exitGameWindow;
        [SerializeField] UIExitWindow exitToMainMenuWindow;

        [Header("Buttons")]
        [SerializeField] List<GameObject> controlsButtons = new List<GameObject>();

        [Header("Test Info Window")]
        public bool showTestInfoWindow = true;
        [SerializeField] UITestInfoWindow testInfoWindow;

        [Header("Version")]
        [SerializeField] TextMeshProUGUI versionText;

        GameController gameController;
        UIExerciseWindow exerciseWindow;

        RouterPC routerPC;
        RouterTablet routerTablet;

        void Awake()
        {
            if (Instance != null)
            {
                Debug.LogWarning("Cannot create UIMainCanvas");
                Destroy(gameObject);
                return;
            }

            Instance = this;
        }

        void Start()
        {
            Init();
        }

        public void Init()
        {
            gameController = GameController.Instance;
            exerciseWindow = UIExerciseWindow.Instance;

            OpenMainMenuWindow();
            ShowTestInfoWindow(showTestInfoWindow);

            if (versionText)
                versionText.text = Application.version;

            if (platform == CanvasPlatform.Windows)
                routerPC = RouterPC.Instance;
            else
                routerTablet = RouterTablet.Instance;
        }

        // Exit Game
        public void OnExitGame()
        {
            ExitGame();

            if (platform == CanvasPlatform.Windows)
                routerPC?.OnExitGame();
            else
                routerTablet?.OnExitGame();
        }

        public void ExitGame()
        {
            App.Instance?.ExitGame();
        }

        // Main Menu Window
        public void OpenMainMenuWindow()
        {
            mainMenuWindow.SetActive(true);
            HUDWindow.SetActive(false);
        }

        public void CloseMainMenuWindow()
        {
            mainMenuWindow.SetActive(false);
            HUDWindow.SetActive(true);
        }

        public void OpenMainMenuPanel()
        {
            mainMenuPanel.SetActive(true);
        }

        public void CloseMainMenuPanel()
        {
            mainMenuPanel.SetActive(false);
        }

        // Start Game
        public void OnStartGame()
        {
            StartGame();

            if (platform == CanvasPlatform.Windows)
                routerPC?.OnStartGame();
            else
                routerTablet?.OnStartGame();
        }

        public void StartGame()
        {
            CloseMainMenuWindow();
            gameController?.SetGameState(GameState.Game);
        }

        // End Game
        public void OnEndGame()
        {
            EndGame();

            if (platform == CanvasPlatform.Windows)
                routerPC?.OnEndGame();
            else
                routerTablet?.OnEndGame();
        }

        public void EndGame()
        {
            OpenMainMenuWindow();
            exerciseWindow?.ExerciseForceExit();
            gameController?.SetGameState(GameState.Menu);
        }

        // ---
        public void ShowTestInfoWindow(bool state)
        {
            showTestInfoWindow = state;
            testInfoWindow.gameObject.SetActive(state);
        }

        public void ShowControlsButtons(bool state)
        {
            foreach (var cb in controlsButtons)
                cb.SetActive(state);
        }

        // Settings Window
        public void OnOpenSettingsWindow()
        {
            OpenSettingsWindow();

            if (platform == CanvasPlatform.Windows)
                routerPC?.OnOpenSettingsWindow();
            else
                routerTablet?.OnOpenSettingsWindow();
        }

        public void OpenSettingsWindow()
        {
            settingsWindow?.Open();
        }

        public void OnCloseSettingsWindow()
        {
            CloseSettingsWindow();

            if (platform == CanvasPlatform.Windows)
                routerPC?.OnCloseSettingsWindow();
            else
                routerTablet?.OnCloseSettingsWindow();
        }

        public void CloseSettingsWindow()
        {
            settingsWindow?.Close();
        }

        // Controls Window
        public void OnOpenControlsWindow()
        {
            OpenControlsWindow();

            if (platform == CanvasPlatform.Windows)
                routerPC?.OnOpenControlsWindow();
            else
                routerTablet?.OnOpenControlsWindow();
        }

        public void OpenControlsWindow()
        {
            controlsWindow?.Open();
        }

        public void OnCloseControlsWindow()
        {
            CloseControlsWindow();

            if (platform == CanvasPlatform.Windows)
                routerPC?.OnCloseControlsWindow();
            else
                routerTablet?.OnCloseControlsWindow();
        }

        public void CloseControlsWindow()
        {
            controlsWindow?.Close();
        }

        // Reports Window
        public void OnOpenReportsWindow()
        {
            OpenReportsWindow();

            if (platform == CanvasPlatform.Windows)
                routerPC?.OnOpenReportsWindow();
            else
                routerTablet?.OnOpenReportsWindow();
        }

        public void OpenReportsWindow()
        {
            reportsWindow?.Open();
        }

        public void OnCloseReportsWindow()
        {
            CloseReportsWindow();

            if (platform == CanvasPlatform.Windows)
                routerPC?.OnCloseReportsWindow();
            else
                routerTablet?.OnCloseReportsWindow();
        }

        public void CloseReportsWindow()
        {
            reportsWindow?.Close();
        }

        // Exit Game Window
        public void OnOpenExitGameWindow()
        {
            OpenExitGameWindow();

            if (platform == CanvasPlatform.Windows)
                routerPC?.OnOpenExitGameWindow();
            else
                routerTablet?.OnOpenExitGameWindow();
        }

        public void OpenExitGameWindow()
        {
            exitGameWindow?.Open();
        }

        public void OnCloseExitGameWindow()
        {
            CloseExitGameWindow();

            if (platform == CanvasPlatform.Windows)
                routerPC?.OnCloseExitGameWindow();
            else
                routerTablet?.OnCloseExitGameWindow();
        }

        public void CloseExitGameWindow()
        {
            exitGameWindow?.Close();
        }

        // Time Of Day Settings Window
        public void OnOpenTimeOfDaySettingsWindow()
        {
            OpenTimeOfDaySettingsWindow();

            if (platform == CanvasPlatform.Windows)
                routerPC?.OnOpenTimeOfDaySettingsWindow();
            else
                routerTablet?.OnOpenTimeOfDaySettingsWindow();
        }

        public void OpenTimeOfDaySettingsWindow()
        {
            timeOfDaySettingsWindow?.Open();
        }

        public void OnCloseTimeOfDaySettingsWindow()
        {
            CloseTimeOfDaySettingsWindow();

            if (platform == CanvasPlatform.Windows)
                routerPC?.OnCloseTimeOfDaySettingsWindow();
            else
                routerTablet?.OnCloseTimeOfDaySettingsWindow();
        }

        public void CloseTimeOfDaySettingsWindow()
        {
            timeOfDaySettingsWindow?.Close();
        }

        // Exercises
        public void OnExerciseForceExit()
        {
            exerciseWindow?.ExerciseForceExit();
        }
    }
}
