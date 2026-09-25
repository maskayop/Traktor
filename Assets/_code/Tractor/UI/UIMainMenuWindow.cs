using UnityEngine;

namespace Tractor.UI
{
    public class UIMainMenuWindow : MonoBehaviour
    {
        [SerializeField] UISettingsWindow settingsWindow;
        [SerializeField] UIControlsWindow controlsWindow;

        void Start()
        {
            Init();
        }

        public void Init()
        {
        }

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
    }
}
