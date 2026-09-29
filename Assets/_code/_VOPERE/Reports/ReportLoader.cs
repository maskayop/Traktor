using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Vopere
{
    public class ReportLoader : MonoBehaviour
    {
        public bool loadOnStart = false;
        public List<Report> loadedReports = new List<Report>();

        const string FolderName = "Reports";

        public static string ReportsFolder
        {
            get
            {
#if UNITY_EDITOR
                return Path.Combine(Application.dataPath, "..", FolderName);
#else
            return Path.Combine(Path.GetDirectoryName(Application.dataPath), FolderName);
#endif
            }
        }

        void Start()
        {
            if (loadOnStart)
                LoadAll();
        }

        public void LoadAll()
        {
            loadedReports.Clear();

            string folder = ReportsFolder;
            if (!Directory.Exists(folder))
            {
                Debug.Log($"Папка {folder} не найдена.");
                return;
            }

            string[] files = Directory.GetFiles(folder, "*.csv");
            if (files.Length == 0)
            {
                Debug.Log($"В {folder} нет .csv.");
                return;
            }

            foreach (string file in files)
            {
                try
                {
                    string text = File.ReadAllText(file, System.Text.Encoding.UTF8);
                    string name = Path.GetFileNameWithoutExtension(file);

                    Report report = Report.FromCsv(text, name);
                    report.dateSaved = File.GetLastWriteTime(file).ToString("yyyy-MM-dd HH:mm:ss");

                    loadedReports.Add(report);
                }
                catch (Exception e)
                {
                    Debug.LogError($"Не удалось загрузить {file}: {e.Message}");
                }
            }

            Debug.Log($"Загружено отчётов: {loadedReports.Count}");
        }

        public Report GetReport(string fileName)
        {
            foreach (var r in loadedReports)
                if (r.fileName == fileName)
                    return r;

            return null;
        }
    }
}
