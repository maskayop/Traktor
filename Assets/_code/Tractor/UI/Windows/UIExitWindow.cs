namespace Tractor.UI
{
    public class UIExitWindow : UI_WindowBase
    {
        UIExerciseWindow exerciseWindow;

        protected override void OnInit()
        {
            exerciseWindow = UIExerciseWindow.Instance;
        }

        public void OnExitGame()
        {
            mainCanvas?.OnExitGame();
        }

        public void OnEndGame()
        {
            mainCanvas?.OnEndGame();
        }

        public void OnExerciseForceExit()
        {
            exerciseWindow?.OnExerciseForceExit();
        }

        public void OnCloseExitGameWindow()
        {
            mainCanvas?.OnCloseExitGameWindow();
        }

        public void OnCloseExitToMainMenuWindow()
        {
            mainCanvas?.OnCloseExitToMainMenuWindow();
        }

        public void OnCloseExerciseForceExitWindow()
        {
            exerciseWindow?.OnCloseExerciseForceExitWindow();
        }
    }
}
