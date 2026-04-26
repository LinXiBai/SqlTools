using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Management;
using System.Text.RegularExpressions;

namespace CoreToolkit.Communication.Helpers
{
    /// <summary>
    /// 串口扫描与信息查询
    /// </summary>
    public static class SerialPortScanner
    {
        /// <summary>
        /// 获取系统当前所有可用串口名称（COM1, COM2...）
        /// </summary>
        public static string[] GetAvailablePortNames()
        {
            return SerialPort.GetPortNames();
        }

        /// <summary>
        /// 获取串口详细信息（名称 + 描述 + PID/VID）
        /// </summary>
        public static List<SerialPortInfo> GetPortDetails()
        {
            var result = new List<SerialPortInfo>();
            var portNames = GetAvailablePortNames();

            try
            {
                using (var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_PnPEntity WHERE Name LIKE '%(COM%')"))
                {
                    foreach (ManagementObject item in searcher.Get())
                    {
                        using (item)
                        {
                            string name = item["Name"]?.ToString() ?? "";
                            foreach (var port in portNames)
                            {
                                if (name.Contains($"({port})"))
                                {
                                    result.Add(new SerialPortInfo
                                    {
                                        PortName = port,
                                        Description = name,
                                        DeviceId = item["DeviceID"]?.ToString() ?? ""
                                    });
                                }
                            }
                        }
                    }
                }
            }
            catch
            {
                // 如果 WMI 查询失败，回退到只返回名称
                result = portNames.Select(p => new SerialPortInfo { PortName = p }).ToList();
            }

            // 补充 WMI 没查到的
            foreach (var port in portNames)
            {
                if (!result.Any(r => StringComparer.OrdinalIgnoreCase.Equals(r.PortName, port)))
                {
                    result.Add(new SerialPortInfo { PortName = port });
                }
            }

            return result.OrderBy(r => ExtractNumber(r.PortName)).ToList();
        }

        /// <summary>
        /// 按 VID/PID 查找特定 USB 转串口设备
        /// </summary>
        public static string FindPortByVidPid(string vid, string pid)
        {
            var details = GetPortDetails();
            var target = details.FirstOrDefault(d =>
                !string.IsNullOrEmpty(d.DeviceId) &&
                d.DeviceId.IndexOf($"VID_{vid}", StringComparison.OrdinalIgnoreCase) >= 0 &&
                d.DeviceId.IndexOf($"PID_{pid}", StringComparison.OrdinalIgnoreCase) >= 0);
            return target?.PortName;
        }

        private static int ExtractNumber(string portName)
        {
            var match = Regex.Match(portName ?? "", @"\d+");
            return match.Success ? int.Parse(match.Value) : 0;
        }
    }

    /// <summary>
    /// 串口详细信息
    /// </summary>
    public class SerialPortInfo
    {
        public string PortName { get; set; }
        public string Description { get; set; }
        public string DeviceId { get; set; }

        public override string ToString()
        {
            return string.IsNullOrEmpty(Description) ? PortName : $"{PortName} - {Description}";
        }
    }
}
