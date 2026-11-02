namespace Tractor.UI
{
    public class UICamerasWindow : UI_WindowBase
    {
        TractorController tractorController;
        TractorCameraController tractorCameraController;

        protected override void OnStart()
        {
            base.OnStart();

            tractorController = FindAnyObjectByType<TractorController>();
            tractorCameraController = tractorController.tractorCameraController;
        }

        public void SetGameCameraContainer(int id)
        {
            tractorCameraController?.SetGameCameraContainer(id);
        }
    }
}
