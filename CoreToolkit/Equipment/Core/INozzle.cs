using System;

namespace CoreToolkit.Equipment.Core
{
    /// <summary>
    /// 吸嘴接口抽象（贴片机/固晶机通用）
    /// </summary>
    public interface INozzle : IDisposable
    {
        int NozzleId { get; }
        bool IsPicking { get; }

        void Pick();
        void Place();
        void Blow();
        void SetVacuum(int levelPercent);
        bool CheckVacuumSensor();
    }
}
