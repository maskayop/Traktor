using UnityEngine;
using Vopere;
using Vopere.Common;

namespace Tractor.UI
{
    public abstract class UI_SettingsWindowBase : MonoBehaviour
    {
        [SerializeField] bool closeOnAwake = true;
        [SerializeField] protected GameObject window;

        protected App app;
        protected DataSaveLoad dataSaveLoad;
        protected ScenesManager scenesManager;
        protected GameController gameController;
        protected AudioController audioController;
        protected LightController lightController;

        protected UIMainCanvas mainCanvas;

        bool isOpen = false;
        public bool IsOpen { get { return isOpen; } }

        void Awake()
        {
            if (closeOnAwake)
                Close();
        }

        void Start()
        {
            Init();
        }

        public void Init()
        {
            app = App.Instance;
            dataSaveLoad = DataSaveLoad.Instance;
            scenesManager = ScenesManager.Instance;
            gameController = GameController.Instance;
            audioController = AudioController.Instance;
            lightController = LightController.Instance;
            mainCanvas = UIMainCanvas.Instance;

            OnInit();
        }

        protected virtual void OnInit() { }

        public void Open()
        {
            isOpen = true;
            window.SetActive(true);

            OnOpen();
        }

        protected virtual void OnOpen() { }

        public void Close()
        {
            isOpen = false;
            window.SetActive(false);

            OnClose();
        }

        protected virtual void OnClose() { }
    }
}
