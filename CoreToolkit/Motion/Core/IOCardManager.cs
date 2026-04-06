using System;
using System.Collections.Generic;
using System.Linq;
using CoreToolkit.Motion.Interfaces;

namespace CoreToolkit.Motion.Core
{
    /// <summary>
    /// IO扩展卡管理器
    /// 用于统一管理多个IO扩展卡
    /// </summary>
    public class IOCardManager : IDisposable
    {
        private readonly Dictionary<int, IIOCard> _ioCards = new Dictionary<int, IIOCard>();
        private readonly object _lockObj = new object();
        private bool _disposed = false;

        #region 单例模式

        private static IOCardManager _instance;
        private static readonly object _instanceLock = new object();

        /// <summary>
        /// 获取IO卡管理器实例
        /// </summary>
        public static IOCardManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_instanceLock)
                    {
                        if (_instance == null)
                        {
                            _instance = new IOCardManager();
                        }
                    }
                }
                return _instance;
            }
        }

        #endregion

        #region 构造函数

        private IOCardManager() { }

        #endregion

        #region IO卡管理

        /// <summary>
        /// 添加IO卡
        /// </summary>
        /// <param name="cardId">卡号</param>
        /// <param name="ioCard">IO卡实例</param>
        public void AddIOCard(int cardId, IIOCard ioCard)
        {
            lock (_lockObj)
            {
                if (_ioCards.ContainsKey(cardId))
                {
                    throw new MotionException($"IO卡号 {cardId} 已存在", -200);
                }

                _ioCards.Add(cardId, ioCard);
            }
        }

        /// <summary>
        /// 移除IO卡
        /// </summary>
        /// <param name="cardId">卡号</param>
        public void RemoveIOCard(int cardId)
        {
            lock (_lockObj)
            {
                if (_ioCards.TryGetValue(cardId, out var ioCard))
                {
                    ioCard?.Dispose();
                    _ioCards.Remove(cardId);
                }
            }
        }

        /// <summary>
        /// 获取IO卡
        /// </summary>
        /// <param name="cardId">卡号</param>
        /// <returns>IO卡实例</returns>
        public IIOCard GetIOCard(int cardId)
        {
            lock (_lockObj)
            {
                if (_ioCards.TryGetValue(cardId, out var ioCard))
                {
                    return ioCard;
                }
                throw new MotionException($"IO卡号 {cardId} 不存在", -201);
            }
        }

        /// <summary>
        /// 检查IO卡是否存在
        /// </summary>
        public bool ContainsIOCard(int cardId)
        {
            lock (_lockObj)
            {
                return _ioCards.ContainsKey(cardId);
            }
        }

        /// <summary>
        /// 获取所有IO卡
        /// </summary>
        public IEnumerable<IIOCard> GetAllIOCards()
        {
            lock (_lockObj)
            {
                return _ioCards.Values.ToList();
            }
        }

        /// <summary>
        /// 获取IO卡数量
        /// </summary>
        public int Count
        {
            get
            {
                lock (_lockObj)
                {
                    return _ioCards.Count;
                }
            }
        }

        /// <summary>
        /// 清除所有IO卡
        /// </summary>
        public void Clear()
        {
            lock (_lockObj)
            {
                foreach (var ioCard in _ioCards.Values)
                {
                    ioCard?.Dispose();
                }
                _ioCards.Clear();
            }
        }

        #endregion

        #region 批量操作

        /// <summary>
        /// 初始化所有IO卡
        /// </summary>
        public void InitializeAll()
        {
            lock (_lockObj)
            {
                foreach (var kvp in _ioCards)
                {
                    try
                    {
                        if (!kvp.Value.IsInitialized)
                        {
                            kvp.Value.Initialize(kvp.Key);
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"初始化IO卡 {kvp.Key} 失败: {ex.Message}");
                    }
                }
            }
        }

        /// <summary>
        /// 打开所有IO卡
        /// </summary>
        public void OpenAll()
        {
            lock (_lockObj)
            {
                foreach (var kvp in _ioCards)
                {
                    try
                    {
                        if (!kvp.Value.IsOpen)
                        {
                            kvp.Value.Open();
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"打开IO卡 {kvp.Key} 失败: {ex.Message}");
                    }
                }
            }
        }

        /// <summary>
        /// 关闭所有IO卡
        /// </summary>
        public void CloseAll()
        {
            lock (_lockObj)
            {
                foreach (var kvp in _ioCards)
                {
                    try
                    {
                        if (kvp.Value.IsOpen)
                        {
                            kvp.Value.Close();
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"关闭IO卡 {kvp.Key} 失败: {ex.Message}");
                    }
                }
            }
        }

        #endregion

        #region 便捷IO操作

        /// <summary>
        /// 读取输入（通过全局索引）
        /// </summary>
        /// <param name="globalIndex">全局输入索引</param>
        /// <returns>输入状态</returns>
        public bool ReadInput(int globalIndex)
        {
            var (cardId, localIndex) = ParseGlobalIndex(globalIndex);
            var ioCard = GetIOCard(cardId);
            return ioCard.ReadInput(localIndex);
        }

        /// <summary>
        /// 设置输出（通过全局索引）
        /// </summary>
        /// <param name="globalIndex">全局输出索引</param>
        /// <param name="value">输出值</param>
        public void WriteOutput(int globalIndex, bool value)
        {
            var (cardId, localIndex) = ParseGlobalIndex(globalIndex);
            var ioCard = GetIOCard(cardId);
            ioCard.WriteOutput(localIndex, value);
        }

        /// <summary>
        /// 翻转输出（通过全局索引）
        /// </summary>
        /// <param name="globalIndex">全局输出索引</param>
        public void ToggleOutput(int globalIndex)
        {
            var (cardId, localIndex) = ParseGlobalIndex(globalIndex);
            var ioCard = GetIOCard(cardId);
            ioCard.ToggleOutput(localIndex);
        }

        #endregion

        #region 索引计算

        /// <summary>
        /// 解析全局索引为卡号和本地索引
        /// </summary>
        /// <param name="globalIndex">全局索引</param>
        /// <param name="ioPointsPerCard">每张卡的IO点数（默认32）</param>
        /// <returns>(卡号, 本地索引)</returns>
        private (int cardId, int localIndex) ParseGlobalIndex(int globalIndex, int ioPointsPerCard = 32)
        {
            if (globalIndex < 0)
            {
                throw new MotionException($"全局索引 {globalIndex} 不能为负数", -202);
            }

            int cardId = globalIndex / ioPointsPerCard;
            int localIndex = globalIndex % ioPointsPerCard;

            return (cardId, localIndex);
        }

        /// <summary>
        /// 计算全局索引
        /// </summary>
        /// <param name="cardId">卡号</param>
        /// <param name="localIndex">本地索引</param>
        /// <param name="ioPointsPerCard">每张卡的IO点数（默认32）</param>
        /// <returns>全局索引</returns>
        public int CalculateGlobalIndex(int cardId, int localIndex, int ioPointsPerCard = 32)
        {
            return cardId * ioPointsPerCard + localIndex;
        }

        #endregion

        #region 状态监控

        /// <summary>
        /// 获取所有IO卡状态
        /// </summary>
        public Dictionary<int, IOCardStatus> GetAllStatus()
        {
            lock (_lockObj)
            {
                var status = new Dictionary<int, IOCardStatus>();
                foreach (var kvp in _ioCards)
                {
                    status[kvp.Key] = new IOCardStatus
                    {
                        CardId = kvp.Key,
                        IsInitialized = kvp.Value.IsInitialized,
                        IsOpen = kvp.Value.IsOpen,
                        InputCount = kvp.Value.InputCount,
                        OutputCount = kvp.Value.OutputCount,
                        CardName = kvp.Value.CardName,
                        Model = kvp.Value.Model
                    };
                }
                return status;
            }
        }

        #endregion

        #region IDisposable

        /// <summary>
        /// 释放资源
        /// </summary>
        public void Dispose()
        {
            if (_disposed) return;

            Clear();
            _disposed = true;
        }

        #endregion
    }

    /// <summary>
    /// IO卡状态信息
    /// </summary>
    public class IOCardStatus
    {
        /// <summary>
        /// 卡号
        /// </summary>
        public int CardId { get; set; }

        /// <summary>
        /// 是否已初始化
        /// </summary>
        public bool IsInitialized { get; set; }

        /// <summary>
        /// 是否已打开
        /// </summary>
        public bool IsOpen { get; set; }

        /// <summary>
        /// 输入点数量
        /// </summary>
        public int InputCount { get; set; }

        /// <summary>
        /// 输出点数量
        /// </summary>
        public int OutputCount { get; set; }

        /// <summary>
        /// 卡名称
        /// </summary>
        public string CardName { get; set; }

        /// <summary>
        /// 型号
        /// </summary>
        public string Model { get; set; }
    }
}
