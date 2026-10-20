namespace Tractor.UI
{
    public class UIExitWindow : UI_WindowBase
    {
        public void OnExitGame()
        {
            mainCanvas?.OnExitGame();
        }

        public void OnEndGame()
        {
            mainCanvas?.OnEndGame();
            Close();
        }

        public void OnExerciseForceExit()
        {
            mainCanvas?.OnExerciseForceExit();
            Close();
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
            mainCanvas?.OnCloseExerciseForceExitWindow();
        }
    }
}
