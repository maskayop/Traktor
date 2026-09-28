using Vopere;

namespace Tractor.UI
{
    public class UIReportsWindow : UI_WindowBase
    {
        Report currentReport;
        ReportController reportController;
        ExercisesController exercisesController;

        bool isExercise = false;

        void Update()
        {
            if (!reportController || !exercisesController)
                return;

            if (isExercise != exercisesController.IsExercise())
                SetCurrentReport(exercisesController.currentReport);

            isExercise = exercisesController.IsExercise();
        }

        protected override void OnInit()
        {
            base.OnInit();

            reportController = ReportController.Instance;
            exercisesController = ExercisesController.Instance;
        }

        protected override void OnClose()
        {
            UIMainMenuWindow.Instance?.OnCloseReportsWindow();
        }

        public void LoadAllReports()
        {
            if (reportController)
                reportController.LoadAllReports();
        }

        public void SetCurrentReport(Report r)
        {
            currentReport = r;
        }

        public void SaveExerciseReport()
        {
            if (reportController)
            {
                reportController.SetReport(currentReport);
                reportController.SaveReport();
            }
        }
    }
}
