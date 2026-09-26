using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System;

public class ReportSaver : MonoBehaviour
{
    [Tooltip("Список строк для сохранения. Заполняется из другого скрипта.")]
    public List<string> lines = new List<string>();

    private const string FolderName = "Reports";

    // Папка Reports: в редакторе — Project/Reports, в билде — рядом с .exe
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

    // Метод для кнопки в UI
    public void Save()
    {
        string folder = ReportsFolder;

        // Создаём папку, если её нет
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

        // Имя файла с датой — чтобы сохранения не перезаписывали друг друга
        string fileName = $"Report_{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.csv";
        string path = Path.Combine(folder, fileName);

        // Формируем CSV
        var sb = new StringBuilder();
        sb.AppendLine("Index,Text");

        for (int i = 0; i < lines.Count; i++)
        {
            sb.AppendLine($"{i + 1},{EscapeCsv(lines[i])}");
        }

        File.WriteAllText(path, sb.ToString(), Encoding.UTF8);
        Debug.Log($"Сохранено: {path}");
    }

    // Экранирование ячейки: оборачиваем в кавычки, если внутри есть запятая/кавычка/перенос
    private string EscapeCsv(string value)
    {
        if (string.IsNullOrEmpty(value)) return "";
        bool needQuotes = value.Contains(",") || value.Contains("\"")
                       || value.Contains("\n") || value.Contains("\r");
        if (!needQuotes) return value;
        return "\"" + value.Replace("\"", "\"\"") + "\"";
    }

    // Бонус: открыть папку Reports в проводнике
    public void OpenReportsFolder()
    {
        string folder = ReportsFolder;
        if (!Directory.Exists(folder))
            Directory.CreateDirectory(folder);

        Application.OpenURL("file://" + folder);
    }
}
