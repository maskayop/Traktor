using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System;

public class ReportLoader : MonoBehaviour
{
    [Tooltip("Сюда попадут загруженные строки. Индекс = № пункта − 1.")]
    public List<string> loadedLines = new List<string>();

    private const string FolderName = "Reports";

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

    // Автозагрузка при старте
    void Start()
    {
        LoadAll();
    }

    // Метод для кнопки (если понадобится перезагрузить вручную)
    public void LoadAll()
    {
        loadedLines.Clear();

        string folder = ReportsFolder;

        if (!Directory.Exists(folder))
        {
            Debug.Log($"Папка {folder} не найдена — грузить нечего.");
            return;
        }

        string[] files = Directory.GetFiles(folder, "*.csv");

        if (files.Length == 0)
        {
            Debug.Log($"В {folder} нет .csv файлов.");
            return;
        }

        foreach (string file in files)
            ParseCsv(file);

        Debug.Log($"Загружено файлов: {files.Length}, строк: {loadedLines.Count}.");
    }

    private void ParseCsv(string path)
    {
        if (!File.Exists(path)) return;

        string[] rawLines = File.ReadAllLines(path, Encoding.UTF8);

        // Пропускаем заголовок (первую строку)
        for (int i = 1; i < rawLines.Length; i++)
        {
            string line = rawLines[i];
            if (string.IsNullOrWhiteSpace(line)) continue;

            var cells = SplitCsvLine(line);
            if (cells.Count < 2) continue;

            if (!int.TryParse(cells[0], out int index)) continue;

            int listIndex = index - 1;
            while (loadedLines.Count <= listIndex)
                loadedLines.Add("");

            loadedLines[listIndex] = cells[1];
        }
    }

    // Парсер строки CSV с поддержкой кавычек
    private List<string> SplitCsvLine(string line)
    {
        var result = new List<string>();
        var current = new StringBuilder();
        bool inQuotes = false;

        for (int i = 0; i < line.Length; i++)
        {
            char c = line[i];

            if (inQuotes)
            {
                if (c == '"')
                {
                    if (i + 1 < line.Length && line[i + 1] == '"')
                    {
                        current.Append('"');
                        i++;
                    }
                    else inQuotes = false;
                }
                else current.Append(c);
            }
            else
            {
                if (c == '"') inQuotes = true;
                else if (c == ',')
                {
                    result.Add(current.ToString());
                    current.Clear();
                }
                else current.Append(c);
            }
        }

        result.Add(current.ToString());
        return result;
    }
}
