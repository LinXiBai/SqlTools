using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace CoreToolkit.Files.Helpers
{
    /// <summary>
    /// 轻量级 INI 文件读写辅助类
    /// </summary>
    public class IniHelper
    {
        private readonly string _filePath;
        private readonly Dictionary<string, Dictionary<string, string>> _data;

        public IniHelper(string filePath)
        {
            _filePath = filePath;
            _data = new Dictionary<string, Dictionary<string, string>>(StringComparer.OrdinalIgnoreCase);
            if (File.Exists(filePath))
                Load();
        }

        public string GetValue(string section, string key, string defaultValue = null)
        {
            if (_data.TryGetValue(section, out var sectionData) && sectionData.TryGetValue(key, out var value))
                return value;
            return defaultValue;
        }

        public int GetInt(string section, string key, int defaultValue = 0)
        {
            return int.TryParse(GetValue(section, key), out int val) ? val : defaultValue;
        }

        public double GetDouble(string section, string key, double defaultValue = 0)
        {
            return double.TryParse(GetValue(section, key), out double val) ? val : defaultValue;
        }

        public bool GetBool(string section, string key, bool defaultValue = false)
        {
            return bool.TryParse(GetValue(section, key), out bool val) ? val : defaultValue;
        }

        public void SetValue(string section, string key, object value)
        {
            if (!_data.ContainsKey(section))
                _data[section] = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            _data[section][key] = value?.ToString();
        }

        public void Save()
        {
            using (var writer = new StreamWriter(_filePath, false, System.Text.Encoding.UTF8))
            {
                foreach (var section in _data)
                {
                    writer.WriteLine($"[{section.Key}]");
                    foreach (var kv in section.Value)
                    {
                        writer.WriteLine($"{kv.Key}={kv.Value}");
                    }
                    writer.WriteLine();
                }
            }
        }

        private void Load()
        {
            string currentSection = "";
            foreach (var line in File.ReadAllLines(_filePath))
            {
                var trimmed = line.Trim();
                if (string.IsNullOrEmpty(trimmed) || trimmed.StartsWith(";")) continue;
                if (trimmed.StartsWith("[") && trimmed.EndsWith("]"))
                {
                    currentSection = trimmed.Substring(1, trimmed.Length - 2);
                    if (!_data.ContainsKey(currentSection))
                        _data[currentSection] = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                }
                else if (trimmed.Contains("="))
                {
                    var idx = trimmed.IndexOf('=');
                    var key = trimmed.Substring(0, idx).Trim();
                    var val = trimmed.Substring(idx + 1).Trim();
                    if (_data.ContainsKey(currentSection))
                        _data[currentSection][key] = val;
                }
            }
        }
    }
}
