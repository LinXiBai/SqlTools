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
    /// 高性能文件索引管理器 - 使用内存索引和字典实现毫秒级搜索
    /// </summary>
    public class FileIndexManager : IDisposable
    {
        private readonly string _searchDirectory;
        private readonly string _fileExtension;
        private readonly ReaderWriterLockSlim _lock = new ReaderWriterLockSlim();
        private readonly FileSystemWatcher _watcher;
        
        // 索引结构
        private Dictionary<string, List<FileIndexEntry>> _indexByProductModel;  // 按产品型号索引
        private Dictionary<string, List<FileIndexEntry>> _indexBySerialPrefix;  // 按序列号前缀索引
        private List<FileIndexEntry> _allFiles;  // 所有文件列表
        private bool _isIndexBuilt = false;
        private DateTime _lastIndexTime;

        /// <summary>
        /// 文件索引条目
        /// </summary>
        public class FileIndexEntry
        {
            public string FullPath { get; set; }
            public string FileName { get; set; }
            public string ProductModel { get; set; }  // 产品型号（第一个下划线前的部分）
            public string SerialNumber { get; set; }  // 完整序列号
            public string SerialPrefix { get; set; }  // 序列号前缀（如SZCCL）
            public DateTime LastWriteTime { get; set; }
            public long FileSize { get; set; }
        }

        /// <summary>
        /// 初始化文件索引管理器
        /// </summary>
        public FileIndexManager(string searchDirectory, string fileExtension = ".txt", bool enableAutoRefresh = true)
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

            // 设置文件系统监控（自动刷新索引）
            if (enableAutoRefresh)
            {
                _watcher = new FileSystemWatcher(_searchDirectory, $"*{_fileExtension}")
                {
                    NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite | NotifyFilters.CreationTime,
                    EnableRaisingEvents = false  // 先不启用，等索引构建完成后再启用
                };
                _watcher.Created += OnFileChanged;
                _watcher.Deleted += OnFileChanged;
                _watcher.Renamed += OnFileRenamed;
                _watcher.Changed += OnFileChanged;
            }
        }

        /// <summary>
        /// 构建索引
        /// </summary>
        public void BuildIndex()
        {
            _lock.EnterWriteLock();
            try
            {
                _indexByProductModel.Clear();
                _indexBySerialPrefix.Clear();
                _allFiles.Clear();

                if (!Directory.Exists(_searchDirectory))
                    return;

                var files = Directory.EnumerateFiles(_searchDirectory, $"*{_fileExtension}", SearchOption.TopDirectoryOnly);

                foreach (var filePath in files)
                {
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
                    }
                    catch { /* 忽略单个文件的错误 */ }
                }

                // 对每个列表按时间排序（降序）
                foreach (var list in _indexByProductModel.Values)
                    list.Sort((a, b) => b.LastWriteTime.CompareTo(a.LastWriteTime));

                foreach (var list in _indexBySerialPrefix.Values)
                    list.Sort((a, b) => b.LastWriteTime.CompareTo(a.LastWriteTime));

                _allFiles.Sort((a, b) => b.LastWriteTime.CompareTo(a.LastWriteTime));

                _isIndexBuilt = true;
                _lastIndexTime = DateTime.Now;

                // 启用文件监控
                if (_watcher != null)
                    _watcher.EnableRaisingEvents = true;
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        /// <summary>
        /// 异步构建索引
        /// </summary>
        public async Task BuildIndexAsync(IProgress<int> progress = null)
        {
            await Task.Run(() =>
            {
                _lock.EnterWriteLock();
                try
                {
                    _indexByProductModel.Clear();
                    _indexBySerialPrefix.Clear();
                    _allFiles.Clear();

                    if (!Directory.Exists(_searchDirectory))
                        return;

                    var files = Directory.EnumerateFiles(_searchDirectory, $"*{_fileExtension}", SearchOption.TopDirectoryOnly).ToList();
                    int total = files.Count;
                    int current = 0;

                    foreach (var filePath in files)
                    {
                        try
                        {
                            var entry = CreateIndexEntry(filePath);
                            if (entry != null)
                            {
                                _allFiles.Add(entry);

                                if (!string.IsNullOrEmpty(entry.ProductModel))
                                {
                                    if (!_indexByProductModel.ContainsKey(entry.ProductModel))
                                        _indexByProductModel[entry.ProductModel] = new List<FileIndexEntry>();
                                    _indexByProductModel[entry.ProductModel].Add(entry);
                                }

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

                    // 排序
                    foreach (var list in _indexByProductModel.Values)
                        list.Sort((a, b) => b.LastWriteTime.CompareTo(a.LastWriteTime));

                    foreach (var list in _indexBySerialPrefix.Values)
                        list.Sort((a, b) => b.LastWriteTime.CompareTo(a.LastWriteTime));

                    _allFiles.Sort((a, b) => b.LastWriteTime.CompareTo(a.LastWriteTime));

                    _isIndexBuilt = true;
                    _lastIndexTime = DateTime.Now;

                    if (_watcher != null)
                        _watcher.EnableRaisingEvents = true;

                    progress?.Report(100);
                }
                finally
                {
                    _lock.ExitWriteLock();
                }
            });
        }

        /// <summary>
        /// 创建索引条目
        /// </summary>
        private FileIndexEntry CreateIndexEntry(string filePath)
        {
            try
            {
                var fileInfo = new FileInfo(filePath);
                var fileName = Path.GetFileNameWithoutExtension(filePath);
                
                // 解析文件名：产品型号_序列号_时间戳_索引.txt
                var parts = fileName.Split('_');
                
                string productModel = parts.Length > 0 ? parts[0] : "";
                string serialNumber = parts.Length > 1 ? parts[1] : "";
                string serialPrefix = "";
                
                // 提取序列号前缀（字母部分）
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
        /// 根据产品型号查找最新文件（毫秒级）
        /// </summary>
        public Result<string> FindLatestByProductModel(string productModel)
        {
            if (string.IsNullOrWhiteSpace(productModel))
                return Result<string>.Failure("产品型号不能为空");

            if (!_isIndexBuilt)
                BuildIndex();

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
        /// 根据序列号前缀查找最新文件（毫秒级）
        /// </summary>
        public Result<string> FindLatestBySerialPrefix(string serialPrefix)
        {
            if (string.IsNullOrWhiteSpace(serialPrefix))
                return Result<string>.Failure("序列号前缀不能为空");

            if (!_isIndexBuilt)
                BuildIndex();

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
        /// 根据任意索引值搜索（兼容模式，稍慢但通用）
        /// </summary>
        public Result<string> FindLatestFile(string indexValue)
        {
            if (string.IsNullOrWhiteSpace(indexValue))
                return Result<string>.Failure("索引值不能为空");

            if (!_isIndexBuilt)
                BuildIndex();

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

            if (!_isIndexBuilt)
                BuildIndex();

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

        /// <summary>
        /// 获取所有产品型号列表
        /// </summary>
        public List<string> GetAllProductModels()
        {
            if (!_isIndexBuilt)
                BuildIndex();

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
        /// 获取所有序列号前缀列表
        /// </summary>
        public List<string> GetAllSerialPrefixes()
        {
            if (!_isIndexBuilt)
                BuildIndex();

            _lock.EnterReadLock();
            try
            {
                return _indexBySerialPrefix.Keys.ToList();
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        /// <summary>
        /// 获取文件总数
        /// </summary>
        public int GetFileCount()
        {
            if (!_isIndexBuilt)
                BuildIndex();

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
        /// 获取索引统计信息
        /// </summary>
        public string GetIndexInfo()
        {
            _lock.EnterReadLock();
            try
            {
                return $"文件总数: {_allFiles.Count}, 产品型号数: {_indexByProductModel.Count}, 序列号前缀数: {_indexBySerialPrefix.Count}, 索引时间: {_lastIndexTime:HH:mm:ss}";
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        /// <summary>
        /// 刷新索引
        /// </summary>
        public void RefreshIndex()
        {
            BuildIndex();
        }

        #region 文件系统监控事件

        private void OnFileChanged(object sender, FileSystemEventArgs e)
        {
            // 文件变化时延迟刷新索引（避免频繁刷新）
            Task.Delay(1000).ContinueWith(_ => BuildIndex());
        }

        private void OnFileRenamed(object sender, RenamedEventArgs e)
        {
            Task.Delay(1000).ContinueWith(_ => BuildIndex());
        }

        #endregion

        #region IDisposable

        public void Dispose()
        {
            _watcher?.Dispose();
            _lock?.Dispose();
        }

        #endregion
    }
}
