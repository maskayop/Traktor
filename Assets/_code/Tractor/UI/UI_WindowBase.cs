using Tractor.Net;
using UnityEngine;
using Vopere;
using Vopere.Common;
using static Tractor.UI.UIMainCanvas;

namespace Tractor.UI
{
    public abstract class UI_WindowBase : MonoBehaviour
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
        protected RouterPC routerPC;
        protected RouterTablet routerTablet;
        protected CanvasPlatform platform = CanvasPlatform.Windows;

        bool isOpen = false;
        public bool IsOpen { get { return isOpen; } }

        void Awake()
        {
            if (closeOnAwake)
                Close();

            OnAwake();
        }

        protected virtual void OnAwake() { }

        void Start()
        {
            Init();
            OnStart();
        }

        protected virtual void OnStart() { }

        void Update()
        {
            OnUpdate();
        }

        protected virtual void OnUpdate() { }

        public void Init()
        {
            app = App.Instance;
            dataSaveLoad = DataSaveLoad.Instance;
            scenesManager = ScenesManager.Instance;
            gameController = GameController.Instance;
            audioController = AudioController.Instance;
            lightController = LightController.Instance;
            mainCanvas = UIMainCanvas.Instance;
            routerPC = RouterPC.Instance;
            routerTablet = RouterTablet.Instance;

            if (mainCanvas)
            {
                platform = mainCanvas.platform;

                if (platform == CanvasPlatform.Windows)
                    routerPC = RouterPC.Instance;
                else
                    routerTablet = RouterTablet.Instance;
            }

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
