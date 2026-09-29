using System.Collections.Generic;
using UnityEngine;

namespace Vopere
{
    public class ReportController : MonoBehaviour
    {
        public static ReportController Instance;

        [SerializeField] ReportSaver reportSaver;
        [SerializeField] ReportLoader reportLoader;

        List<Report> allReports = new List<Report>();

        void Awake()
        {
            if (Instance != null)
            {
                Debug.LogWarning("Cannot create ReportController");
                Destroy(gameObject);
                return;
            }

            Instance = this;
        }

        void Start()
        {
            Init();
        }

        public void Init() { }

        public void LoadAllReports()
        {
            reportLoader.LoadAll();
            allReports.Clear();

            for (int i = 0; i < reportLoader.loadedReports.Count; i++)
                allReports.Add(reportLoader.loadedReports[i]);
        }

        public void SaveReport()
        {
            reportSaver.Save();
        }

        public void SetReport(Report r)
        {
            reportSaver.report = r;
        }

        public void OpenReportsFolder()
        {
            reportSaver.OpenReportsFolder();
        }
        public List<Report> GetAllReports()
        {
            return allReports;
        }
    }
}
