using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using CoreToolkit.Common.Models;

namespace CoreToolkit.Files.Helpers
{
    /// <summary>
    /// 高性能文件索引管理器 V2 - 支持实时监控、异步预热和状态管理
    /// </summary>
    public class FileIndexManagerV2 : IDisposable
    {
        private readonly string _searchDirectory;
        private readonly string _fileExtension;
        private readonly ReaderWriterLockSlim _lock = new ReaderWriterLockSlim();
        private readonly FileSystemWatcher _watcher;
        private readonly object _warmupLock = new object();
        
        // 索引结构
        private Dictionary<string, List<FileIndexEntry>> _indexByProductModel;
        private Dictionary<string, List<FileIndexEntry>> _indexBySerialPrefix;
        private List<FileIndexEntry> _allFiles;
        
        // 状态管理
        private IndexStatus _status = IndexStatus.NotInitialized;
        private DateTime _lastIndexTime;
        private DateTime _lastFolderModifiedTime;
        private Task _warmupTask;
        private CancellationTokenSource _warmupCts;

        /// <summary>
        /// 索引状态
        /// </summary>
        public enum IndexStatus
        {
            /// <summary>未初始化</summary>
            NotInitialized,
            /// <summary>预热中</summary>
            WarmingUp,
            /// <summary>就绪</summary>
            Ready,
            /// <summary>需要刷新</summary>
            NeedsRefresh,
            /// <summary>错误</summary>
            Error
        }

        /// <summary>
        /// 文件索引条目
        /// </summary>
        public class FileIndexEntry
        {
            public string FullPath { get; set; }
            public string FileName { get; set; }
            public string ProductModel { get; set; }
            public string SerialNumber { get; set; }
            public string SerialPrefix { get; set; }
            public DateTime LastWriteTime { get; set; }
            public long FileSize { get; set; }
        }

        /// <summary>
        /// 初始化文件索引管理器 V2
        /// </summary>
        public FileIndexManagerV2(string searchDirectory, string fileExtension = ".txt", bool autoWarmup = true)
        {
            if (string.IsNullOrWhiteSpace(searchDirectory))
                throw new ArgumentException("搜索目录不能为空", nameof(searchDirectory));

            _searchDirectory = searchDirectory;
            _fileExtension = fileExtension.StartsWith(".") ? fileExtension : "." + fileExtension;

            if (!Directory.Exists(_searchDirectory))
                Directory.CreateDirectory(_searchDirectory);

            // 初始化索引
            _indexByProductModel = new Dictionary<string, List<FileIndexEntry>>(StringComparer.OrdinalIgnoreCase);
            _indexBySerialPrefix = new Dictionary<string, List<FileIndexEntry>>(StringComparer.OrdinalIgnoreCase);
            _allFiles = new List<FileIndexEntry>();

            // 记录文件夹当前修改时间
            _lastFolderModifiedTime = GetFolderLastWriteTime();

            // 设置文件系统监控
            _watcher = new FileSystemWatcher(_searchDirectory, $"*{_fileExtension}")
            {
                NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite | NotifyFilters.CreationTime | NotifyFilters.DirectoryName,
                EnableRaisingEvents = false
            };
            _watcher.Created += OnFileChanged;
            _watcher.Deleted += OnFileChanged;
            _watcher.Renamed += OnFileRenamed;
            _watcher.Changed += OnFileChanged;

            // 自动预热
            if (autoWarmup)
            {
                StartWarmupAsync();
            }
        }

        #region 状态管理

        /// <summary>
        /// 获取当前索引状态（使用独立锁，避免递归）
        /// </summary>
        public IndexStatus Status
        {
            get
            {
                lock (_warmupLock)
                {
                    return _status;
                }
            }
        }

        /// <summary>
        /// 获取状态描述
        /// </summary>
        public string StatusDescription
        {
            get
            {
                var status = Status;
                switch (status)
                {
                    case IndexStatus.NotInitialized: return "未初始化";
                    case IndexStatus.WarmingUp: return "预热中...";
                    case IndexStatus.Ready: return "就绪";
                    case IndexStatus.NeedsRefresh: return "需要刷新";
                    case IndexStatus.Error: return "错误";
                    default: return "未知";
                }
            }
        }

        /// <summary>
        /// 是否就绪
        /// </summary>
        public bool IsReady 
        { 
            get 
            { 
                lock (_warmupLock) 
                { 
                    return _status == IndexStatus.Ready; 
                } 
            } 
        }

        /// <summary>
        /// 是否正在预热
        /// </summary>
        public bool IsWarmingUp 
        { 
            get 
            { 
                lock (_warmupLock) 
                { 
                    return _status == IndexStatus.WarmingUp; 
                } 
            } 
        }

        /// <summary>
        /// 等待预热完成
        /// </summary>
        public bool WaitForReady(TimeSpan? timeout = null)
        {
            var actualTimeout = timeout ?? TimeSpan.FromSeconds(30);
            var sw = System.Diagnostics.Stopwatch.StartNew();
            
            while (sw.Elapsed < actualTimeout)
            {
                if (Status == IndexStatus.Ready)
                    return true;
                
                if (Status == IndexStatus.Error)
                    return false;
                
                Thread.Sleep(10);
            }
            
            return false;
        }

        /// <summary>
        /// 异步等待预热完成
        /// </summary>
        public async Task<bool> WaitForReadyAsync(TimeSpan? timeout = null)
        {
            var actualTimeout = timeout ?? TimeSpan.FromSeconds(30);
            var sw = System.Diagnostics.Stopwatch.StartNew();
            
            while (sw.Elapsed < actualTimeout)
            {
                if (Status == IndexStatus.Ready)
                    return true;
                
                if (Status == IndexStatus.Error)
                    return false;
                
                await Task.Delay(10);
            }
            
            return false;
        }

        #endregion

        #region 预热机制

        /// <summary>
        /// 开始异步预热
        /// </summary>
        public Task StartWarmupAsync(IProgress<int> progress = null)
        {
            lock (_warmupLock)
            {
                // 如果已经在预热中，返回现有任务
                if (_status == IndexStatus.WarmingUp && _warmupTask != null)
                    return _warmupTask;

                // 如果已经就绪，检查是否需要刷新
                if (_status == IndexStatus.Ready)
                {
                    var currentModifiedTime = GetFolderLastWriteTime();
                    if (currentModifiedTime <= _lastFolderModifiedTime)
                        return Task.CompletedTask; // 不需要刷新
                }

                // 取消之前的预热任务
                _warmupCts?.Cancel();
                _warmupCts = new CancellationTokenSource();
                var token = _warmupCts.Token;

                // 设置状态为预热中
                _lock.EnterWriteLock();
                try
                {
                    _status = IndexStatus.WarmingUp;
                }
                finally
                {
                    _lock.ExitWriteLock();
                }

                // 启动异步预热
                _warmupTask = Task.Run(() => DoWarmup(progress, token), token);
                return _warmupTask;
            }
        }

        /// <summary>
        /// 执行预热
        /// </summary>
        private void DoWarmup(IProgress<int> progress, CancellationToken token)
        {
            try
            {
                _lock.EnterWriteLock();
                try
                {
                    // 清空现有索引
                    _indexByProductModel.Clear();
                    _indexBySerialPrefix.Clear();
                    _allFiles.Clear();

                    if (!Directory.Exists(_searchDirectory))
                    {
                        _status = IndexStatus.Error;
                        return;
                    }

                    // 获取所有文件
                    var files = Directory.EnumerateFiles(_searchDirectory, $"*{_fileExtension}", SearchOption.TopDirectoryOnly).ToList();
                    int total = files.Count;
                    int current = 0;

                    foreach (var filePath in files)
                    {
                        // 检查取消令牌
                        if (token.IsCancellationRequested)
                        {
                            _status = IndexStatus.NeedsRefresh;
                            return;
                        }

                        try
                        {
                            var entry = CreateIndexEntry(filePath);
                            if (entry != null)
                            {
                                _allFiles.Add(entry);

                                // 按产品型号索引
                                if (!string.IsNullOrEmpty(entry.ProductModel))
                                {
                                    if (!_indexByProductModel.ContainsKey(entry.ProductModel))
                                        _indexByProductModel[entry.ProductModel] = new List<FileIndexEntry>();
                                    _indexByProductModel[entry.ProductModel].Add(entry);
                                }

                                // 按序列号前缀索引
                                if (!string.IsNullOrEmpty(entry.SerialPrefix))
                                {
                                    if (!_indexBySerialPrefix.ContainsKey(entry.SerialPrefix))
                                        _indexBySerialPrefix[entry.SerialPrefix] = new List<FileIndexEntry>();
                                    _indexBySerialPrefix[entry.SerialPrefix].Add(entry);
                                }
                            }

                            current++;
                            if (progress != null && current % 1000 == 0)
                                progress.Report(current * 100 / total);
                        }
                        catch { }
                    }

                    // 对每个列表按时间排序（降序）
                    foreach (var list in _indexByProductModel.Values)
                        list.Sort((a, b) => b.LastWriteTime.CompareTo(a.LastWriteTime));

                    foreach (var list in _indexBySerialPrefix.Values)
                        list.Sort((a, b) => b.LastWriteTime.CompareTo(a.LastWriteTime));

                    _allFiles.Sort((a, b) => b.LastWriteTime.CompareTo(a.LastWriteTime));

                    // 更新状态
                    _lastIndexTime = DateTime.Now;
                    _lastFolderModifiedTime = GetFolderLastWriteTime();
                    _status = IndexStatus.Ready;

                    // 启用文件监控
                    if (_watcher != null)
                        _watcher.EnableRaisingEvents = true;

                    progress?.Report(100);
                }
                finally
                {
                    _lock.ExitWriteLock();
                }
            }
            catch (Exception ex)
            {
                _lock.EnterWriteLock();
                try
                {
                    _status = IndexStatus.Error;
                }
                finally
                {
                    _lock.ExitWriteLock();
                }
                System.Diagnostics.Debug.WriteLine($"预热失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 获取文件夹最后修改时间
        /// </summary>
        private DateTime GetFolderLastWriteTime()
        {
            try
            {
                return Directory.GetLastWriteTime(_searchDirectory);
            }
            catch
            {
                return DateTime.MinValue;
            }
        }

        /// <summary>
        /// 检查是否需要刷新
        /// </summary>
        public bool NeedsRefresh()
        {
            try
            {
                var currentModifiedTime = GetFolderLastWriteTime();
                return currentModifiedTime > _lastFolderModifiedTime;
            }
            catch
            {
                return false;
            }
        }

        #endregion

        #region 搜索方法

        /// <summary>
        /// 根据产品型号查找最新文件
        /// </summary>
        public Result<string> FindLatestByProductModel(string productModel)
        {
            if (string.IsNullOrWhiteSpace(productModel))
                return Result<string>.Failure("产品型号不能为空");

            // 检查状态
            if (_status == IndexStatus.WarmingUp)
                return Result<string>.Failure("索引正在预热中，请稍后再试");
            
            if (_status != IndexStatus.Ready)
                return Result<string>.Failure($"索引未就绪，当前状态: {StatusDescription}");

            _lock.EnterReadLock();
            try
            {
                if (_indexByProductModel.TryGetValue(productModel, out var list) && list.Count > 0)
                {
                    return Result<string>.Success(list[0].FullPath, $"找到 {list.Count} 个匹配文件");
                }

                return Result<string>.Failure($"未找到产品型号为 '{productModel}' 的文件");
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        /// <summary>
        /// 根据序列号前缀查找最新文件
        /// </summary>
        public Result<string> FindLatestBySerialPrefix(string serialPrefix)
        {
            if (string.IsNullOrWhiteSpace(serialPrefix))
                return Result<string>.Failure("序列号前缀不能为空");

            // 检查状态
            if (_status == IndexStatus.WarmingUp)
                return Result<string>.Failure("索引正在预热中，请稍后再试");
            
            if (_status != IndexStatus.Ready)
                return Result<string>.Failure($"索引未就绪，当前状态: {StatusDescription}");

            _lock.EnterReadLock();
            try
            {
                var upperPrefix = serialPrefix.ToUpper();
                if (_indexBySerialPrefix.TryGetValue(upperPrefix, out var list) && list.Count > 0)
                {
                    return Result<string>.Success(list[0].FullPath, $"找到 {list.Count} 个匹配文件");
                }

                return Result<string>.Failure($"未找到序列号前缀为 '{serialPrefix}' 的文件");
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        /// <summary>
        /// 通用搜索（兼容模式）
        /// </summary>
        public Result<string> FindLatestFile(string indexValue)
        {
            if (string.IsNullOrWhiteSpace(indexValue))
                return Result<string>.Failure("索引值不能为空");

            // 检查状态
            if (_status == IndexStatus.WarmingUp)
                return Result<string>.Failure("索引正在预热中，请稍后再试");
            
            if (_status != IndexStatus.Ready)
                return Result<string>.Failure($"索引未就绪，当前状态: {StatusDescription}");

            _lock.EnterReadLock();
            try
            {
                // 先尝试精确匹配产品型号
                if (_indexByProductModel.TryGetValue(indexValue, out var pmList) && pmList.Count > 0)
                    return Result<string>.Success(pmList[0].FullPath, $"找到 {pmList.Count} 个匹配文件");

                // 再尝试序列号前缀
                var upperValue = indexValue.ToUpper();
                if (_indexBySerialPrefix.TryGetValue(upperValue, out var spList) && spList.Count > 0)
                    return Result<string>.Success(spList[0].FullPath, $"找到 {spList.Count} 个匹配文件");

                // 最后遍历所有文件（最坏情况）
                var entry = _allFiles.FirstOrDefault(f => 
                    f.FileName.Contains(indexValue) || 
                    f.SerialNumber.Contains(indexValue));

                if (entry != null)
                    return Result<string>.Success(entry.FullPath, "找到 1 个匹配文件");

                return Result<string>.Failure($"未找到包含 '{indexValue}' 的文件");
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        /// <summary>
        /// 查找所有匹配的文件
        /// </summary>
        public Result<List<FileIndexEntry>> FindAllByProductModel(string productModel)
        {
            if (string.IsNullOrWhiteSpace(productModel))
                return Result<List<FileIndexEntry>>.Failure("产品型号不能为空");

            // 检查状态
            if (_status == IndexStatus.WarmingUp)
                return Result<List<FileIndexEntry>>.Failure("索引正在预热中，请稍后再试");
            
            if (_status != IndexStatus.Ready)
                return Result<List<FileIndexEntry>>.Failure($"索引未就绪，当前状态: {StatusDescription}");

            _lock.EnterReadLock();
            try
            {
                if (_indexByProductModel.TryGetValue(productModel, out var list))
                {
                    return Result<List<FileIndexEntry>>.Success(list.ToList(), $"找到 {list.Count} 个匹配文件");
                }

                return Result<List<FileIndexEntry>>.Failure($"未找到产品型号为 '{productModel}' 的文件");
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        #endregion

        #region 辅助方法

        private FileIndexEntry CreateIndexEntry(string filePath)
        {
            try
            {
                var fileInfo = new FileInfo(filePath);
                var fileName = Path.GetFileNameWithoutExtension(filePath);
                
                var parts = fileName.Split('_');
                
                string productModel = parts.Length > 0 ? parts[0] : "";
                string serialNumber = parts.Length > 1 ? parts[1] : "";
                string serialPrefix = "";
                
                if (!string.IsNullOrEmpty(serialNumber))
                {
                    var match = Regex.Match(serialNumber, @"^([A-Za-z]+)");
                    if (match.Success)
                        serialPrefix = match.Groups[1].Value.ToUpper();
                }

                return new FileIndexEntry
                {
                    FullPath = filePath,
                    FileName = fileName,
                    ProductModel = productModel,
                    SerialNumber = serialNumber,
                    SerialPrefix = serialPrefix,
                    LastWriteTime = fileInfo.LastWriteTime,
                    FileSize = fileInfo.Length
                };
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// 获取文件总数
        /// </summary>
        public int GetFileCount()
        {
            _lock.EnterReadLock();
            try
            {
                return _allFiles.Count;
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        /// <summary>
        /// 获取所有产品型号列表
        /// </summary>
        public List<string> GetAllProductModels()
        {
            if (_status != IndexStatus.Ready)
                return new List<string>();

            _lock.EnterReadLock();
            try
            {
                return _indexByProductModel.Keys.ToList();
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        /// <summary>
        /// 获取索引统计信息
        /// </summary>
        public string GetIndexInfo()
        {
            _lock.EnterReadLock();
            try
            {
                return $"状态: {StatusDescription}, 文件数: {_allFiles.Count}, 产品型号: {_indexByProductModel.Count}, 序列号前缀: {_indexBySerialPrefix.Count}, 最后索引: {_lastIndexTime:HH:mm:ss}";
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        #endregion

        #region 文件系统监控

        private void OnFileChanged(object sender, FileSystemEventArgs e)
        {
            // 标记需要刷新
            _lock.EnterWriteLock();
            try
            {
                if (_status == IndexStatus.Ready)
                    _status = IndexStatus.NeedsRefresh;
            }
            finally
            {
                _lock.ExitWriteLock();
            }

            // 延迟自动刷新
            Task.Delay(1000).ContinueWith(_ => StartWarmupAsync());
        }

        private void OnFileRenamed(object sender, RenamedEventArgs e)
        {
            OnFileChanged(sender, null);
        }

        #endregion

        #region IDisposable

        public void Dispose()
        {
            _warmupCts?.Cancel();
            _watcher?.Dispose();
            _lock?.Dispose();
        }

        #endregion
    }
}
