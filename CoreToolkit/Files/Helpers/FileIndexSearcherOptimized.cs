using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using CoreToolkit.Common.Models;

namespace CoreToolkit.Files.Helpers
{
    /// <summary>
    /// 优化的文件索引搜索器 - 使用缓存机制提高大量文件搜索性能
    /// </summary>
    public class FileIndexSearcherOptimized
    {
        private readonly string _searchDirectory;
        private readonly string _fileExtension;
        private readonly ReaderWriterLockSlim _lock = new ReaderWriterLockSlim();
        
        // 缓存
        private List<FileInfo> _fileCache;
        private DateTime _cacheLastUpdate;
        private readonly TimeSpan _cacheExpiration = TimeSpan.FromMinutes(5);

        /// <summary>
        /// 初始化优化的文件索引搜索器
        /// </summary>
        /// <param name="searchDirectory">搜索目录路径</param>
        /// <param name="fileExtension">文件扩展名（默认.txt）</param>
        public FileIndexSearcherOptimized(string searchDirectory, string fileExtension = ".txt")
        {
            if (string.IsNullOrWhiteSpace(searchDirectory))
                throw new ArgumentException("搜索目录不能为空", nameof(searchDirectory));

            _searchDirectory = searchDirectory;
            _fileExtension = fileExtension.StartsWith(".") ? fileExtension : "." + fileExtension;

            if (!Directory.Exists(_searchDirectory))
                Directory.CreateDirectory(_searchDirectory);
        }

        /// <summary>
        /// 搜索目录路径
        /// </summary>
        public string SearchDirectory => _searchDirectory;

        /// <summary>
        /// 文件扩展名
        /// </summary>
        public string FileExtension => _fileExtension;

        /// <summary>
        /// 刷新缓存
        /// </summary>
        public void RefreshCache()
        {
            _lock.EnterWriteLock();
            try
            {
                _fileCache = null;
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        /// <summary>
        /// 获取缓存的文件列表
        /// </summary>
        private List<FileInfo> GetCachedFiles()
        {
            _lock.EnterUpgradeableReadLock();
            try
            {
                // 检查缓存是否有效
                if (_fileCache != null && DateTime.Now - _cacheLastUpdate < _cacheExpiration)
                {
                    return _fileCache;
                }

                // 需要刷新缓存
                _lock.EnterWriteLock();
                try
                {
                    _fileCache = Directory.EnumerateFiles(_searchDirectory, $"*{_fileExtension}", SearchOption.TopDirectoryOnly)
                        .Select(f => new FileInfo(f))
                        .ToList();
                    _cacheLastUpdate = DateTime.Now;
                    return _fileCache;
                }
                finally
                {
                    _lock.ExitWriteLock();
                }
            }
            finally
            {
                _lock.ExitUpgradeableReadLock();
            }
        }

        /// <summary>
        /// 根据索引值查找最新的文件（使用缓存）
        /// </summary>
        /// <param name="indexValue">索引值（如产品型号、序列号等）</param>
        /// <returns>包含最新文件路径的结果</returns>
        public Result<string> FindLatestFile(string indexValue)
        {
            if (string.IsNullOrWhiteSpace(indexValue))
                return Result<string>.Failure("索引值不能为空");

            try
            {
                var files = GetCachedFiles();
                
                var matchingFiles = files.Where(f => 
                    f.Name.Contains(indexValue))
                    .ToList();

                if (matchingFiles.Count == 0)
                    return Result<string>.Failure($"未找到包含索引值 '{indexValue}' 的文件");

                var latestFile = matchingFiles.OrderByDescending(f => f.LastWriteTime).First();
                return Result<string>.Success(latestFile.FullName, $"找到 {matchingFiles.Count} 个匹配文件");
            }
            catch (Exception ex)
            {
                return Result<string>.Failure($"搜索文件时发生错误: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// 根据索引值查找最新的文件（不使用缓存，实时搜索）
        /// </summary>
        /// <param name="indexValue">索引值</param>
        /// <returns>包含最新文件路径的结果</returns>
        public Result<string> FindLatestFileRealtime(string indexValue)
        {
            if (string.IsNullOrWhiteSpace(indexValue))
                return Result<string>.Failure("索引值不能为空");

            try
            {
                _lock.EnterReadLock();
                try
                {
                    var matchingFiles = new List<FileInfo>();

                    foreach (var filePath in Directory.EnumerateFiles(_searchDirectory, $"*{_fileExtension}", SearchOption.TopDirectoryOnly))
                    {
                        var fileName = Path.GetFileNameWithoutExtension(filePath);
                        
                        if (fileName != null && fileName.Contains(indexValue))
                        {
                            matchingFiles.Add(new FileInfo(filePath));
                        }
                    }

                    if (matchingFiles.Count == 0)
                        return Result<string>.Failure($"未找到包含索引值 '{indexValue}' 的文件");

                    var latestFile = matchingFiles.OrderByDescending(f => f.LastWriteTime).First();
                    return Result<string>.Success(latestFile.FullName, $"找到 {matchingFiles.Count} 个匹配文件");
                }
                finally
                {
                    _lock.ExitReadLock();
                }
            }
            catch (Exception ex)
            {
                return Result<string>.Failure($"搜索文件时发生错误: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// 查找所有匹配的文件（使用缓存）
        /// </summary>
        /// <param name="indexValue">索引值</param>
        /// <returns>包含匹配文件列表的结果</returns>
        public Result<List<FileInfo>> FindAllFiles(string indexValue)
        {
            if (string.IsNullOrWhiteSpace(indexValue))
                return Result<List<FileInfo>>.Failure("索引值不能为空");

            try
            {
                var files = GetCachedFiles();
                
                var matchingFiles = files.Where(f => 
                    f.Name.Contains(indexValue))
                    .OrderByDescending(f => f.LastWriteTime)
                    .ToList();

                if (matchingFiles.Count == 0)
                    return Result<List<FileInfo>>.Failure($"未找到包含索引值 '{indexValue}' 的文件");

                return Result<List<FileInfo>>.Success(matchingFiles, $"找到 {matchingFiles.Count} 个匹配文件");
            }
            catch (Exception ex)
            {
                return Result<List<FileInfo>>.Failure($"搜索文件时发生错误: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// 获取目录中所有文件的数量
        /// </summary>
        /// <returns>文件数量</returns>
        public int GetFileCount()
        {
            return GetCachedFiles().Count;
        }

        /// <summary>
        /// 获取缓存信息
        /// </summary>
        public string GetCacheInfo()
        {
            _lock.EnterReadLock();
            try
            {
                if (_fileCache == null)
                    return "缓存未初始化";
                
                var age = DateTime.Now - _cacheLastUpdate;
                return $"缓存文件数: {_fileCache.Count}, 缓存年龄: {age.TotalSeconds:F1}秒";
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }
    }
}
