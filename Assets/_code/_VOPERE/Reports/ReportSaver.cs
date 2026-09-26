using System;
using System.IO;
using System.Text;
using UnityEngine;

namespace Vopere
{
    public class ReportSaver : MonoBehaviour
    {
        public Report report = new Report();

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

        public void Save()
        {
            string folder = ReportsFolder;
            try
            {
                if (!Directory.Exists(folder))
                    Directory.CreateDirectory(folder);
            }
            catch (Exception e)
            {
                Debug.LogError($"Не удалось создать папку {folder}: {e.Message}");
                return;
            }

            // Базовое имя: либо заданное, либо "Report"
            string baseName = string.IsNullOrWhiteSpace(report.fileName)
                ? "Report"
                : report.fileName;

            string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss");
            string fileName = $"{baseName} {timestamp}.csv";
            string path = Path.Combine(folder, fileName);

            report.fileName = Path.GetFileNameWithoutExtension(fileName);
            report.dateSaved = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

            File.WriteAllText(path, report.ToCsv(), Encoding.UTF8);
            Debug.Log($"Сохранено: {path}");
        }

        // Открыть папку Reports в проводнике
        public void OpenReportsFolder()
        {
            string folder = ReportsFolder;
            if (!Directory.Exists(folder))
                Directory.CreateDirectory(folder);
            Application.OpenURL("file://" + folder);
        }
    }
}
