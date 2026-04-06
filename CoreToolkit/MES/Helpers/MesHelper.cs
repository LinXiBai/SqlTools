using System;
using System.Collections.Generic;
using System.Linq;
using CoreToolkit.MES.Models;

namespace CoreToolkit.MES.Helpers
{
    /// <summary>
    /// MES 交互辅助类
    /// </summary>
    public static class MesHelper
    {
        private static readonly Random _rnd = new Random();

        /// <summary>
        /// 生成 MES 交易流水号（时间戳+随机数）
        /// </summary>
        public static string GenerateTransactionId()
        {
            return $"TXN{DateTime.Now:yyyyMMddHHmmss}{_rnd.Next(1000, 9999)}";
        }

        /// <summary>
        /// 构建过站结果（PASS / FAIL）
        /// </summary>
        public static string BuildResultCode(bool isPass, string failureCode = null)
        {
            return isPass ? "PASS" : (failureCode ?? "FAIL");
        }

        /// <summary>
        /// 从贴装结果快速构建 TrackOut 请求
        /// </summary>
        public static TrackOutRequest BuildTrackOutFromPlacement(
            string workOrderNumber,
            string serialNumber,
            string equipmentId,
            string processStep,
            IEnumerable<Equipment.Models.PlacementResult> placements,
            string operatorId = null)
        {
            var list = placements?.ToList() ?? new List<Equipment.Models.PlacementResult>();
            var passCount = list.Count(p => p.IsSuccess);
            var failCount = list.Count - passCount;
            var firstFail = list.FirstOrDefault(p => !p.IsSuccess);

            return new TrackOutRequest
            {
                WorkOrderNumber = workOrderNumber,
                SerialNumber = serialNumber,
                EquipmentId = equipmentId,
                ProcessStep = processStep,
                OperatorId = operatorId,
                PassCount = passCount,
                FailCount = failCount,
                Result = failCount == 0 ? "PASS" : "FAIL",
                FailureCode = firstFail?.ErrorMessage,
                Parameters = list.Select(p => new ProcessParameter
                {
                    ParameterName = $"Placement_{p.StationId}_DeltaX",
                    ParameterValue = p.DeltaX.ToString("F4")
                }).Concat(list.Select(p => new ProcessParameter
                {
                    ParameterName = $"Placement_{p.StationId}_DeltaY",
                    ParameterValue = p.DeltaY.ToString("F4")
                })).ToList()
            };
        }

        /// <summary>
        /// 从设备状态枚举转为 MES 状态字符串
        /// </summary>
        public static string MapEquipmentStatus(EquipmentStatus status)
        {
            switch (status)
            {
                case EquipmentStatus.Running: return "RUNNING";
                case EquipmentStatus.Idle: return "IDLE";
                case EquipmentStatus.Down: return "DOWN";
                case EquipmentStatus.Maintenance: return "MAINTENANCE";
                case EquipmentStatus.Alarm: return "ALARM";
                default: return "UNKNOWN";
            }
        }
    }

    /// <summary>
    /// 设备状态枚举
    /// </summary>
    public enum EquipmentStatus
    {
        Running,
        Idle,
        Down,
        Maintenance,
        Alarm
    }
}
