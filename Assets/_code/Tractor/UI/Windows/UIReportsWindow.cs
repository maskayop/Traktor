using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Vopere;

namespace Tractor.UI
{
    public class UIReportsWindow : UI_WindowBase
    {
        [Header("Окно отчётов")]
        [SerializeField] GameObject reportButtonPrefab;
        [SerializeField] RectTransform reportButtonsContainer;
        [SerializeField] GameObject openSelectedReportButton;
        List<UIReportButton> reportButtons = new List<UIReportButton>();

        [Header("Выбранный отчёт")]
        [SerializeField] GameObject selectedReportWindow;
        [SerializeField] GameObject reportStepTextPrefab;
        [SerializeField] RectTransform reportStepTextsContainer;
        [SerializeField] TextMeshProUGUI selectedReportNameText;
        List<UIReportStepText> reportStepTexts = new List<UIReportStepText>();

        Report currentReport;
        ReportController reportController;
        ExercisesController exercisesController;

        List<Report> allReports = new List<Report>();

        bool isExercise = false;

        void Update()
        {
            if (!reportController || !exercisesController)
                return;

            if (isExercise != exercisesController.IsExercise())
                SetCurrentReport(exercisesController.CurrentReport);

            isExercise = exercisesController.IsExercise();
        }

        protected override void OnInit()
        {
            base.OnInit();

            reportController = ReportController.Instance;
            exercisesController = ExercisesController.Instance;

            reportButtons.Clear();

            foreach (Transform t in reportButtonsContainer)
                Destroy(t.gameObject);
        }

        protected override void OnOpen()
        {
            selectedReportWindow.SetActive(false);
            currentReport = null;
            ShowOpenSelectedReportButton();
        }

        protected override void OnClose()
        {
            UIMainMenuWindow.Instance?.OnCloseReportsWindow();
            currentReport = null;
            selectedReportWindow.SetActive(false);
        }

        public void LoadAllReports()
        {
            if (!reportController)
                return;

            reportController.LoadAllReports();
            allReports.Clear();

            for (int i = 0; i < reportController.GetAllReports().Count; i++)
                allReports.Add(reportController.GetAllReports()[i]);

            CreateReportButtons();
            ShowOpenSelectedReportButton();
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

        public void OpenReportsFolder()
        {
            reportController.OpenReportsFolder();
        }

        public void OpenSelectedReportWindow()
        {
            selectedReportWindow.SetActive(true);
            window.SetActive(false);

            selectedReportNameText.text = currentReport.fileName;
            CreateExerciseStepTexts();
        }

        public void CloseSelectedReportWindow()
        {
            selectedReportWindow.SetActive(false);
            window.SetActive(true);
        }

        void ShowOpenSelectedReportButton()
        {
            if (currentReport == null)
                openSelectedReportButton.SetActive(false);
            else
                openSelectedReportButton.SetActive(true);
        }

        void CreateReportButtons()
        {
            reportButtons.Clear();
            currentReport = null;

            foreach (Transform t in reportButtonsContainer)
                Destroy(t.gameObject);

            for (int i = 0; i < allReports.Count; i++)
            {
                GameObject go = Instantiate(reportButtonPrefab, reportButtonsContainer);
                go.name = allReports[i].fileName;

                UIReportButton rb = go.GetComponent<UIReportButton>();
                rb.Init(this, allReports[i]);
                reportButtons.Add(rb);
            }
        }

        public void SelectReport(Report INreport)
        {
            if (INreport == null)
            {
                foreach (var rb in reportButtons)
                    rb.Select(false);

                return;
            }

            currentReport = INreport;

            for (int i = 0; i < reportButtons.Count; i++)
            {
                if (reportButtons[i].Report == INreport)
                    reportButtons[i].Select(true);
                else
                    reportButtons[i].Select(false);
            }

            ShowOpenSelectedReportButton();
        }

        void CreateExerciseStepTexts()
        {
            reportStepTexts.Clear();

            foreach (Transform t in reportStepTextsContainer)
                Destroy(t.gameObject);

            if (currentReport == null)
                return;

            for (int i = 0; i < currentReport.columns[0].values.Count; i++)
            {
                GameObject go = Instantiate(reportStepTextPrefab, reportStepTextsContainer);
                UIReportStepText rst = go.GetComponent<UIReportStepText>();
                rst.Init(currentReport.columns[0].values[i], currentReport.columns[1].values[i], currentReport.columns[2].values[i]);
                reportStepTexts.Add(rst);
            }
        }
    }
}
