namespace Tractor.UI
{
    public class UIControlsWindow : UI_WindowBase
    {
        public void OnCloseControlsWindow()
        {
            mainCanvas?.OnCloseControlsWindow();
        }
    }
}
