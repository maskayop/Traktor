using Tractor.Net;
using UnityEngine;
using static Tractor.UI.UIMainCanvas;

namespace Tractor.UI
{
    public class UIMainMenuWindow : MonoBehaviour
    {
        public static UIMainMenuWindow Instance;

        [SerializeField] GameObject menuWindow;

        [Header("Окна")]
        [SerializeField] UISettingsWindow settingsWindow;
        [SerializeField] UIControlsWindow controlsWindow;
        [SerializeField] UIReportsWindow reportsWindow;

        [Header("Кнопки")]
        [SerializeField] GameObject controlsButton;

        UIMainCanvas mainCanvas;
        CanvasPlatform platform = CanvasPlatform.Windows;
        RouterPC routerPC;
        RouterTablet routerTablet;

        void Awake()
        {
            if (Instance != null)
            {
                Debug.LogWarning("Cannot create UIMainMenuWindow");
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
            mainCanvas = UIMainCanvas.Instance;

            if (!mainCanvas)
                return;

            platform = mainCanvas.platform;

            if (platform == CanvasPlatform.Windows)
                routerPC = RouterPC.Instance;
            else
                routerTablet = RouterTablet.Instance;
        }

        public void OpenMenuWindow()
        {
            menuWindow.SetActive(true);
        }

        public void CloseMenuWindow()
        {
            menuWindow.SetActive(false);
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
            settingsWindow.Open();
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
            settingsWindow.Close();
        }

        // Controls Window
        public void OpenControlsWindow()
        {
            controlsWindow.Open();
        }

        public void CloseControlsWindow()
        {
            controlsWindow.Close();
        }

        public void ShowControlsButton(bool state)
        {
            controlsButton.SetActive(state);
        }

        public void OpenReportsWindow()
        {
            reportsWindow.Open();
            CloseMenuWindow();
        }

        public void CloseReportsWindow()
        {
            reportsWindow.Close();
            OnCloseReportsWindow();
        }

        public void OnCloseReportsWindow()
        {
            OpenMenuWindow();
        }
    }
}
