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

        Report report;
        public Report Report { get { return report; } }

        UIReportsWindow reportsWindow;

        public void Init(UIReportsWindow INreportsWindow, Report INreport)
        {
            reportsWindow = INreportsWindow;
            report = INreport;
            nameText.text = report.fileName;

            Select(false);
        }

        public void Select(bool state)
        {
            isSelected = state;
            selected.SetActive(isSelected);
        }

        public void SelectReport()
        {
            Select(true);
            reportsWindow?.SelectReport(report);
        }
    }
}
