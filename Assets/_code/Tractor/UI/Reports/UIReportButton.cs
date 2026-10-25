using TMPro;
using UnityEngine;
using Vopere;

namespace Tractor.UI
{
    public class UIReportButton : MonoBehaviour
    {
        [SerializeField] GameObject selected;
        [SerializeField] TextMeshProUGUI nameText;

        bool isSelected = false;
        public bool IsSelected { get { return isSelected; } }

        Report report;
        public Report Report { get { return report; } }

        string reportName;
        public string ReportName { get { return reportName; } }

        UIReportsWindow reportsWindow;

        // Для ПК: из Report
        public void Init(UIReportsWindow INreportsWindow, Report INreport)
        {
            reportsWindow = INreportsWindow;
            report = INreport;
            reportName = INreport.fileName;
            nameText.text = reportName;

            Select(false);
        }

        // Для планшета: из имени файла
        public void Init(UIReportsWindow INreportsWindow, string fileName)
        {
            reportsWindow = INreportsWindow;
            reportName = fileName;
            nameText.text = fileName;

            Select(false);
        }

        public void Select(bool state)
        {
            isSelected = state;
            if (selected) selected.SetActive(state);
        }

        public void SelectReport()
        {
            Select(true);
            reportsWindow?.SelectReportByName(reportName);
        }
    }
}
