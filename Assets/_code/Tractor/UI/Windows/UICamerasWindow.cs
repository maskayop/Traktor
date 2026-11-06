using UnityEngine;
using static Tractor.UI.UIMainCanvas;

namespace Tractor.UI
{
    public class UICamerasWindow : UI_WindowBase
    {
        public static UICamerasWindow Instance;

        TractorController tractorController;
        TractorCameraController tractorCameraController;

        protected override void OnAwake()
        {
            if (Instance != null)
            {
                Debug.LogWarning("Cannot create UICamerasWindow");
                Destroy(gameObject);
                return;
            }

            Instance = this;
        }

        protected override void OnStart()
        {
            base.OnStart();

            tractorController = FindAnyObjectByType<TractorController>();

            if (tractorController)
                tractorCameraController = tractorController.tractorCameraController;
        }

        public void SetGameCameraContainer(int id)
        {
            tractorCameraController?.SetGameCameraContainer(id);
        }

        public void OnSetGameCameraInFront()
        {
            SetGameCameraContainer(0);

            if (platform == CanvasPlatform.Windows)
                routerPC?.OnSetGameCameraInFront();
            else
                routerTablet?.OnSetGameCameraInFront();
        }

        public void OnSetGameCameraInBack()
        {
            SetGameCameraContainer(1);

            if (platform == CanvasPlatform.Windows)
                routerPC?.OnSetGameCameraInBack();
            else
                routerTablet?.OnSetGameCameraInBack();
        }

        public void OnSetGameCameraOutFront()
        {
            SetGameCameraContainer(2);

            if (platform == CanvasPlatform.Windows)
                routerPC?.OnSetGameCameraOutFront();
            else
                routerTablet?.OnSetGameCameraOutFront();
        }

        public void OnSetGameCameraOutBack()
        {
            SetGameCameraContainer(3);

            if (platform == CanvasPlatform.Windows)
                routerPC?.OnSetGameCameraOutBack();
            else
                routerTablet?.OnSetGameCameraOutBack();
        }
    }
}
