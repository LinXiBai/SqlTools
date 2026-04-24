using System;
using CoreToolkit.Data;
using CoreToolkit.Data.Models;
using CoreToolkit.Desktop.MVVM;

namespace LicenseManager.WPF.ViewModels
{
    /// <summary>
    /// 授权记录视图模型
    /// </summary>
    public class LicenseRecordViewModel : ObservableObject
    {
        private long _id;
        private DateTime _recordTime;
        private string _department;
        private string _operatorName;
        private string _applicant;
        private string _projectNumber;
        private string _deviceNumber;
        private string _deviceType;
        private string _machineCodeShort;
        private bool _isSelected;
        private bool _isEditing;
        private LicenseRecord _fullRecord;

        public long Id
        {
            get => _id;
            set => SetProperty(ref _id, value);
        }

        public DateTime RecordTime
        {
            get => _recordTime;
            set => SetProperty(ref _recordTime, value);
        }

        public string Department
        {
            get => _department;
            set => SetProperty(ref _department, value);
        }

        public string OperatorName
        {
            get => _operatorName;
            set => SetProperty(ref _operatorName, value);
        }

        public string Applicant
        {
            get => _applicant;
            set => SetProperty(ref _applicant, value);
        }

        public string ProjectNumber
        {
            get => _projectNumber;
            set => SetProperty(ref _projectNumber, value);
        }

        public string DeviceNumber
        {
            get => _deviceNumber;
            set => SetProperty(ref _deviceNumber, value);
        }

        public string DeviceType
        {
            get => _deviceType;
            set => SetProperty(ref _deviceType, value);
        }

        public string MachineCodeShort
        {
            get => _machineCodeShort;
            set => SetProperty(ref _machineCodeShort, value);
        }

        public bool IsSelected
        {
            get => _isSelected;
            set => SetProperty(ref _isSelected, value);
        }

        public bool IsEditing
        {
            get => _isEditing;
            set => SetProperty(ref _isEditing, value);
        }

        public LicenseRecord FullRecord
        {
            get => _fullRecord;
            set => SetProperty(ref _fullRecord, value);
        }

        public string RecordTimeText => RecordTime.ToString("yyyy-MM-dd HH:mm");

        /// <summary>
        /// 从实体创建视图模型
        /// </summary>
        public static LicenseRecordViewModel FromEntity(LicenseRecord record)
        {
            if (record == null) return null;

            var vm = new LicenseRecordViewModel
            {
                Id = record.Id,
                RecordTime = record.RecordTime,
                Department = record.Department,
                OperatorName = record.Operator,
                Applicant = record.Applicant,
                ProjectNumber = record.ProjectNumber,
                DeviceNumber = record.DeviceNumber,
                DeviceType = record.DeviceType,
                FullRecord = record
            };

            // 机器码缩略显示
            if (!string.IsNullOrEmpty(record.MachineCode))
            {
                vm.MachineCodeShort = record.MachineCode.Length > 30
                    ? record.MachineCode.Substring(0, 30) + "..."
                    : record.MachineCode;
            }

            return vm;
        }

        /// <summary>
        /// 更新实体（用于编辑保存）
        /// </summary>
        public void UpdateEntity(LicenseRecord record)
        {
            if (record == null) return;
            record.Department = Department;
            record.Operator = OperatorName;
            record.Applicant = Applicant;
            record.ProjectNumber = ProjectNumber;
            record.DeviceNumber = DeviceNumber;
            record.DeviceType = DeviceType;
            record.UpdatedAt = DateTime.Now;
        }
    }
}
