using System.Collections.Generic;
using TMPro;
using Tractor.Net;
using UnityEngine;
using Vopere;
using static Tractor.UI.UIMainCanvas;

namespace Tractor.UI
{
    public class UIReportsWindow : UI_WindowBase
    {
        public static UIReportsWindow Instance;

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

        CanvasPlatform platform = CanvasPlatform.Windows;
        RouterPC routerPC;
        RouterTablet routerTablet;

        protected override void OnAwake()
        {
            if (Instance != null)
            {
                Debug.LogWarning("Cannot create UIReportsWindow");
                Destroy(gameObject);
                return;
            }

            Instance = this;
        }

        protected override void OnUpdate()
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

            if (mainCanvas)
            {
                platform = mainCanvas.platform;
                if (platform == CanvasPlatform.Windows)
                    routerPC = RouterPC.Instance;
                else
                    routerTablet = RouterTablet.Instance;
            }

            reportButtons.Clear();
            foreach (Transform t in reportButtonsContainer)
                Destroy(t.gameObject);
        }

        protected override void OnOpen()
        {
            currentReport = null;
            selectedReportWindow.SetActive(false);
            mainCanvas?.CloseMainMenuPanel();
            ShowOpenSelectedReportButton();
        }

        protected override void OnClose()
        {
            currentReport = null;
            selectedReportWindow.SetActive(false);
            mainCanvas?.OpenMainMenuPanel();
        }

        public void OnCloseReportsWindow()
        {
            mainCanvas?.OnCloseReportsWindow();
        }

        public void LoadAllReports()
        {
            if (platform == CanvasPlatform.Windows)
            {
                // ПК: локальная загрузка + отправка на планшет
                if (!reportController) return;

                reportController.LoadAllReports();
                allReports.Clear();

                for (int i = 0; i < reportController.GetAllReports().Count; i++)
                    allReports.Add(reportController.GetAllReports()[i]);

                CreateReportButtons();
                ShowOpenSelectedReportButton();

                routerPC?.SendReportsList();
            }
            else
            {
                // Планшет: попросить ПК загрузить и прислать список
                routerTablet?.RequestReports();
            }
        }

        public void SetCurrentReport(Report r)
        {
            currentReport = r;
        }

        public void OnSaveExerciseReport()
        {
            SaveExerciseReport();

            if (platform == CanvasPlatform.Windows)
                routerPC?.OnSaveExerciseReport();
            else
                routerTablet?.OnSaveExerciseReport();
        }

        public void SaveExerciseReport()
        {
            if (!reportController)
                return;

            reportController.SetReport(currentReport);
            reportController.SaveReport();
        }

        public void OpenReportsFolder()
        {
            reportController?.OpenReportsFolder();
        }

        public void OpenSelectedReportWindow()
        {
            selectedReportWindow.SetActive(true);
            window.SetActive(false);

            selectedReportNameText.text = currentReport.fileName;
            CreateExerciseStepTexts();
        }

        public void OnCloseSelectedReportWindow()
        {
            CloseSelectedReportWindow();

            if (platform == CanvasPlatform.Windows)
                routerPC?.OnCloseSelectedReportWindow();
            else
                routerTablet?.OnCloseSelectedReportWindow();
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

        // === Для планшета ===
        public void AddReportButton(string fileName)
        {
            if (!reportButtonPrefab || !reportButtonsContainer) return;

            // Уже есть кнопка с таким именем? — игнорируем
            for (int i = 0; i < reportButtons.Count; i++)
            {
                if (reportButtons[i].ReportName == fileName)
                    return;
            }

            GameObject go = Instantiate(reportButtonPrefab, reportButtonsContainer);
            go.name = fileName;

            UIReportButton rb = go.GetComponent<UIReportButton>();
            rb.Init(this, fileName);
            reportButtons.Add(rb);
        }

        public void SelectReportByName(string fileName)
        {
            if (platform == CanvasPlatform.Windows)
            {
                // ПК-ветка: найти Report, выбрать, открыть
                if (!reportController) return;

                Report found = null;
                for (int i = 0; i < allReports.Count; i++)
                {
                    if (allReports[i].fileName == fileName) { found = allReports[i]; break; }
                }

                if (found == null) return;

                SelectReport(found);
                OpenSelectedReportWindow();
            }
            else
            {
                // Планшет-ветка: подсветить, открыть selectedReportWindow без шагов, отправить на ПК
                for (int i = 0; i < reportButtons.Count; i++)
                    reportButtons[i].Select(reportButtons[i].ReportName == fileName);

                OpenTabletReportWindow(fileName);

                routerTablet?.SendSelectReport(fileName);
            }
        }

        void OpenTabletReportWindow(string fileName)
        {
            selectedReportWindow.SetActive(true);
            window.SetActive(false);
            selectedReportNameText.text = fileName;
        }
    }
}
