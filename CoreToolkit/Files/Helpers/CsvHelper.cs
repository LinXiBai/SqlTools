using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace CoreToolkit.Files.Helpers
{
    /// <summary>
    /// 通用 CSV 读写辅助类
    /// </summary>
    public static class CsvHelper
    {
        /// <summary>
        /// 将对象列表写入 CSV 字符串
        /// </summary>
        public static string Write<T>(IEnumerable<T> items)
        {
            var sb = new StringBuilder();
            var properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);
            sb.AppendLine(string.Join(",", properties.Select(p => Escape(p.Name))));

            foreach (var item in items)
            {
                var values = properties.Select(p => Escape(p.GetValue(item)?.ToString()));
                sb.AppendLine(string.Join(",", values));
            }
            return sb.ToString();
        }

        /// <summary>
        /// 将 CSV 字符串解析为对象列表（简单映射）
        /// </summary>
        public static List<T> Read<T>(string csv) where T : new()
        {
            var result = new List<T>();
            var lines = csv.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries);
            if (lines.Length < 2) return result;

            var headers = ParseLine(lines[0]);
            var properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .ToDictionary(p => p.Name, StringComparer.OrdinalIgnoreCase);

            for (int i = 1; i < lines.Length; i++)
            {
                var values = ParseLine(lines[i]);
                var item = new T();
                for (int j = 0; j < Math.Min(headers.Count, values.Count); j++)
                {
                    if (properties.TryGetValue(headers[j], out var prop))
                    {
                        try
                        {
                            var val = Convert.ChangeType(values[j], prop.PropertyType);
                            prop.SetValue(item, val);
                        }
                        catch { }
                    }
                }
                result.Add(item);
            }
            return result;
        }

        private static string Escape(string value)
        {
            if (string.IsNullOrEmpty(value)) return "";
            if (value.Contains(",") || value.Contains("\"") || value.Contains("\n") || value.Contains("\r"))
                return "\"" + value.Replace("\"", "\"\"") + "\"";
            return value;
        }

        private static List<string> ParseLine(string line)
        {
            var result = new List<string>();
            var sb = new StringBuilder();
            bool inQuotes = false;
            for (int i = 0; i < line.Length; i++)
            {
                char c = line[i];
                if (c == '"')
                {
                    if (inQuotes && i + 1 < line.Length && line[i + 1] == '"')
                    {
                        sb.Append('"'); i++;
                    }
                    else
                    {
                        inQuotes = !inQuotes;
                    }
                }
                else if (c == ',' && !inQuotes)
                {
                    result.Add(sb.ToString()); sb.Clear();
                }
                else
                {
                    sb.Append(c);
                }
            }
            result.Add(sb.ToString());
            return result;
        }
    }
}
