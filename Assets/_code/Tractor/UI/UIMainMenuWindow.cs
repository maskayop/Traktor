using UnityEngine;

namespace Tractor.UI
{
    public class UIMainMenuWindow : MonoBehaviour
    {
        public static UIMainMenuWindow Instance;

        [Header("Окна")]
        [SerializeField] UISettingsWindow settingsWindow;
        [SerializeField] UIControlsWindow controlsWindow;

        [Header("Кнопки")]
        [SerializeField] GameObject controlsButton;

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

        public void Init() { }

        public void OpenSettingsWindow()
        {
            settingsWindow.Open();
        }

        public void CloseSettingsWindow()
        {
            settingsWindow.Close();
        }

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
    }
}
