using System;

namespace CoreToolkit.Equipment.Core
{
    /// <summary>
    /// 加热/温控接口抽象（固晶机加热台、贴片机温区等）
    /// </summary>
    public interface IHeater : IDisposable
    {
        int ZoneId { get; }
        double CurrentTemperature { get; }
        double TargetTemperature { get; set; }
        bool IsReached { get; }

        void StartHeat();
        void StopHeat();
    }
}
