using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Vopere
{
    [System.Serializable]
    public class Report
    {
        [Tooltip("Имя файла без расширения.")]
        public string fileName = "Report";

        [Tooltip("Дата последнего изменения файла (при загрузке).")]
        public string dateSaved = "";

        [Tooltip("Колонки таблицы. Первая колонка в CSV — № пункта, генерируется автоматом.")]
        public List<Column> columns = new List<Column>();

        const char Separator = ';';

        public int RowCount
        {
            get
            {
                int max = 0;
                for (int c = 0; c < columns.Count; c++)
                    if (columns[c].values.Count > max)
                        max = columns[c].values.Count;
                return max;
            }
        }

        public List<string> GetColumn(int index)
        {
            if (index < 0 || index >= columns.Count) return null;
            return columns[index].values;
        }

        public string ToCsv()
        {
            var sb = new StringBuilder();

            // Заголовок: "№" + имена колонок
            sb.Append("№");
            for (int c = 0; c < columns.Count; c++)
            {
                sb.Append(Separator);
                sb.Append(EscapeCsv(columns[c].header));
            }
            sb.AppendLine();

            // Строки
            int rowCount = RowCount;
            for (int r = 0; r < rowCount; r++)
            {
                sb.Append(r + 1); // № пункта: 1, 2, 3, ...

                for (int c = 0; c < columns.Count; c++)
                {
                    sb.Append(Separator);
                    string value = r < columns[c].values.Count ? columns[c].values[r] : "";
                    sb.Append(EscapeCsv(value));
                }
                sb.AppendLine();
            }

            return sb.ToString();
        }

        public static Report FromCsv(string csvText, string fileName = "")
        {
            var report = new Report { fileName = fileName };

            string[] rawLines = csvText.Split(
                new[] { '\n', '\r' },
                System.StringSplitOptions.RemoveEmptyEntries);

            if (rawLines.Length < 1) return report;

            // Заголовок: первая ячейка — "№" (игнорируем), остальные — имена колонок
            var headerCells = SplitCsvLine(rawLines[0]);
            int columnCount = headerCells.Count - 1;

            for (int c = 0; c < columnCount; c++)
            {
                report.columns.Add(new Column
                {
                    header = headerCells[c + 1],
                    values = new List<string>()
                });
            }

            // Данные
            for (int i = 1; i < rawLines.Length; i++)
            {
                string line = rawLines[i];
                if (string.IsNullOrWhiteSpace(line)) continue;

                var cells = SplitCsvLine(line);
                if (cells.Count < 1) continue;

                if (!int.TryParse(cells[0], out int index)) continue;
                int rowIndex = index - 1;

                for (int c = 0; c < columnCount; c++)
                {
                    string value = (c + 1) < cells.Count ? cells[c + 1] : "";

                    while (report.columns[c].values.Count <= rowIndex)
                        report.columns[c].values.Add("");

                    report.columns[c].values[rowIndex] = value;
                }
            }

            return report;
        }

        static string EscapeCsv(string value)
        {
            if (string.IsNullOrEmpty(value)) return "";

            bool needQuotes = value.IndexOf(Separator) >= 0
                           || value.Contains("\"")
                           || value.Contains("\n")
                           || value.Contains("\r");

            if (!needQuotes) return value;
            return "\"" + value.Replace("\"", "\"\"") + "\"";
        }

        static List<string> SplitCsvLine(string line)
        {
            var result = new List<string>();
            var current = new StringBuilder();
            bool inQuotes = false;

            for (int i = 0; i < line.Length; i++)
            {
                char ch = line[i];

                if (inQuotes)
                {
                    if (ch == '"')
                    {
                        if (i + 1 < line.Length && line[i + 1] == '"')
                        {
                            current.Append('"');
                            i++;
                        }
                        else inQuotes = false;
                    }
                    else current.Append(ch);
                }
                else
                {
                    if (ch == '"') inQuotes = true;
                    else if (ch == Separator)
                    {
                        result.Add(current.ToString());
                        current.Clear();
                    }
                    else current.Append(ch);
                }
            }

            result.Add(current.ToString());
            return result;
        }
    }
}
