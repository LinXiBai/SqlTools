using System;

namespace CoreToolkit.Equipment.Core
{
    /// <summary>
    /// 供料器接口抽象（飞达/料盒）
    /// </summary>
    public interface IFeeder : IDisposable
    {
        int FeederId { get; }
        bool IsReady { get; }

        void Advance();
        void Reset();
        void Peel();
    }
}
