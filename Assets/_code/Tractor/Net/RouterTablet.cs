using TMPro;
using Tractor.UI;
using UnityEngine;

namespace Tractor.Net
{
    public class RouterTablet : MonoBehaviour
    {
        public static RouterTablet Instance;

        public bool destroyOnWrongPlatform = true;

        [Header("UI, которым управляет роутер")]
        public GameObject additionalMenu;
        public TextMeshProUGUI displayText;

        TabletClient client;

        UIMainCanvas mainCanvas;
        UIMainMenuWindow mainMenuWindow;

#if UNITY_ANDROID
        void Awake()
        {
            if (Instance != null)
            {
                Debug.LogWarning("Cannot create RouterTablet");
                Destroy(gameObject);
                return;
            }

            Instance = this;
        }
#else
        void Awake()
        {
            if (destroyOnWrongPlatform)
                DestroyImmediate(gameObject);
        }
#endif

        void Start()
        {
            client = TabletClient.Instance;
            mainCanvas = UIMainCanvas.Instance;
            mainMenuWindow = UIMainMenuWindow.Instance;
        }

        // Вызывается из TabletClient в главном потоке (через очередь)
        public void Route(string command)
        {
            switch (command)
            {
                /*
                case "TRACTOR_STARTED":
                    ShowMessage("Трактор запущен");
                    break;

                case "TRACTOR_STOPPED":
                    ShowMessage("Трактор остановлен");
                    break;

                case "EMERGENCY_STOPPED":
                    ShowMessage("АВАРИЙНАЯ ОСТАНОВКА!");
                    break;

                case "ShowMenu":
                    OnShowMenu();
                    break;

                case "HideMenu":
                    OnHideMenu();
                    break;
                */
                case "ExitGame":
                    mainCanvas.ExitGame();
                    break;
                case "OpenSettingsWindow":
                    mainMenuWindow.OpenSettingsWindow();
                    break;
                case "CloseSettingsWindow":
                    mainMenuWindow.CloseSettingsWindow();
                    break;
                default:
                    // Всё остальное — просто текст из InputField на ПК
                    ShowMessage(command);
                    break;
            }
        }

        public void ShowMessage(string text)
        {
            if (displayText)
                displayText.text = text;
        }
        /*
        public void OnShowMenu()
        {
            Debug.Log("Показать меню");
            if (additionalMenu)
                additionalMenu.SetActive(true);
        }

        public void OnHideMenu()
        {
            Debug.Log("Скрыть меню");
            if (additionalMenu)
                additionalMenu.SetActive(false);
        }

        // Обёртки для кнопок
        public void OnStartButton() { client?.SendCommand("START"); }
        public void OnStopButton() { client?.SendCommand("STOP"); }
        public void OnEmergencyButton() { client?.SendCommand("EMERGENCY"); }
        */
        public void OnExitGame() { client?.SendCommand("ExitGame"); }
        public void OnOpenSettingsWindow() { client?.SendCommand("OpenSettingsWindow"); }
        public void OnCloseSettingsWindow() { client?.SendCommand("CloseSettingsWindow"); }
    }
}
