using System;
using System.IO;
using System.Windows;
using CoreToolkit.Common;
using CoreToolkit.Data;
using Microsoft.Win32;
using Newtonsoft.Json;

namespace LicenseManager.WPF
{
    public partial class DetailWindow : Window
    {
        private readonly LicenseRecord _record;
        private MachineCodeInfo _machineCodeInfo;

        public DetailWindow(LicenseRecord record)
        {
            InitializeComponent();
            _record = record ?? throw new ArgumentNullException(nameof(record));
            LoadData();
        }

        private void LoadData()
        {
            IdText.Text = _record.Id.ToString();
            RecordTimeText.Text = _record.RecordTime.ToString("yyyy-MM-dd HH:mm:ss");
            DepartmentText.Text = _record.Department;
            OperatorText.Text = _record.Operator;
            ApplicantText.Text = _record.Applicant;
            ProjectNumberText.Text = _record.ProjectNumber;
            DeviceNumberText.Text = _record.DeviceNumber;
            CreatedAtText.Text = _record.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss");
            UpdatedAtText.Text = _record.UpdatedAt.ToString("yyyy-MM-dd HH:mm:ss");

            MachineCodeJsonText.Text = _record.MachineCode;
            _machineCodeInfo = LicenseSerializer.DeserializeMachineCode(_record.MachineCode);
        }

        private void ViewMachineCodeDetailBtn_Click(object sender, RoutedEventArgs e)
        {
            if (_machineCodeInfo == null)
            {
                MessageBox.Show("无法解析机器码信息", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            string detail = $"机器码详细信息:\n\n" +
                           $"计算机名: {_machineCodeInfo.MachineName}\n" +
                           $"CPU ID: {_machineCodeInfo.CpuId}\n" +
                           $"主板ID: {_machineCodeInfo.MotherboardId}\n" +
                           $"硬盘ID: {_machineCodeInfo.DiskId}\n" +
                           $"MAC地址: {_machineCodeInfo.MacAddress}\n" +
                           $"操作系统: {_machineCodeInfo.OsVersion}\n" +
                           $"系统安装日期: {_machineCodeInfo.InstallDate?.ToString("yyyy-MM-dd") ?? "未知"}\n" +
                           $"生成时间: {_machineCodeInfo.GeneratedAt:yyyy-MM-dd HH:mm:ss}\n" +
                           $"扩展信息: {_machineCodeInfo.ExtraInfo ?? "无"}\n\n" +
                           $"指纹: {LicenseSerializer.GenerateMachineFingerprint(_machineCodeInfo)}";

            MessageBox.Show(detail, "机器码详情", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void CopyMachineCodeBtn_Click(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(_record.MachineCode);
            MessageBox.Show("机器码已复制到剪贴板", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void ExportBtn_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new SaveFileDialog
            {
                FileName = $"License_{_record.ProjectNumber}_{DateTime.Now:yyyyMMdd}",
                Filter = "JSON文件|*.json|文本文件|*.txt|所有文件|*.*",
                Title = "导出授权记录"
            };

            if (dialog.ShowDialog() == true)
            {
                try
                {
                    var exportData = new
                    {
                        _record.Id,
                        _record.RecordTime,
                        _record.Department,
                        _record.Operator,
                        _record.Applicant,
                        _record.ProjectNumber,
                        _record.DeviceNumber,
                        _record.CreatedAt,
                        _record.UpdatedAt,
                        MachineCode = _machineCodeInfo
                    };

                    string json = JsonConvert.SerializeObject(exportData, Formatting.Indented);
                    File.WriteAllText(dialog.FileName, json);
                    MessageBox.Show("导出成功！", "成功", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"导出失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void CloseBtn_Click(object sender, RoutedEventArgs e) => Close();
    }
}
