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
    /// 文件索引搜索器 - 用于在大量文件中根据索引值快速查找最新文件
    /// </summary>
    /// <remarks>
    /// 适用于包含大量文本文件（如20万+）的文件夹，根据索引值匹配文件名，返回时间最新的文件路径
    /// 文件名格式示例：TUM126316D020_SZCCL249161010_20260408.txt
    /// 其中索引值可以是：TUM126316D020（产品型号）或 SZCCL249161010（序列号）
    /// </remarks>
    public class FileIndexSearcher
    {
        private readonly string _searchDirectory;
        private readonly string _fileExtension;
        private readonly ReaderWriterLockSlim _lock = new ReaderWriterLockSlim();

        /// <summary>
        /// 初始化文件索引搜索器
        /// </summary>
        /// <param name="searchDirectory">搜索目录路径</param>
        /// <param name="fileExtension">文件扩展名（默认.txt）</param>
        public FileIndexSearcher(string searchDirectory, string fileExtension = ".txt")
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
        /// 根据索引值查找最新的文件
        /// </summary>
        /// <param name="indexValue">索引值（如产品型号、序列号等）</param>
        /// <returns>包含最新文件路径的结果，如果未找到则返回失败结果</returns>
        public Result<string> FindLatestFile(string indexValue)
        {
            if (string.IsNullOrWhiteSpace(indexValue))
                return Result<string>.Failure("索引值不能为空");

            try
            {
                _lock.EnterReadLock();
                try
                {
                    // 使用EnumerateFiles而不是GetFiles，避免一次性加载所有文件名到内存
                    var matchingFiles = new List<FileInfo>();

                    foreach (var filePath in Directory.EnumerateFiles(_searchDirectory, $"*{_fileExtension}", SearchOption.TopDirectoryOnly))
                    {
                        var fileName = Path.GetFileNameWithoutExtension(filePath);
                        
                        // 检查文件名是否包含索引值
                        if (fileName != null && fileName.Contains(indexValue))
                        {
                            matchingFiles.Add(new FileInfo(filePath));
                        }
                    }

                    if (matchingFiles.Count == 0)
                        return Result<string>.Failure($"未找到包含索引值 '{indexValue}' 的文件");

                    // 按最后写入时间排序，取最新的
                    var latestFile = matchingFiles.OrderByDescending(f => f.LastWriteTime).First();

                    return Result<string>.Success(latestFile.FullName, $"找到 {matchingFiles.Count} 个匹配文件，返回最新文件");
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
        /// 根据索引值查找最新的文件（异步版本）
        /// </summary>
        /// <param name="indexValue">索引值（如产品型号、序列号等）</param>
        /// <returns>包含最新文件路径的结果</returns>
        public async System.Threading.Tasks.Task<Result<string>> FindLatestFileAsync(string indexValue)
        {
            return await System.Threading.Tasks.Task.Run(() => FindLatestFile(indexValue));
        }

        /// <summary>
        /// 根据索引值查找所有匹配的文件，按时间倒序排列
        /// </summary>
        /// <param name="indexValue">索引值</param>
        /// <returns>包含匹配文件列表的结果</returns>
        public Result<List<FileInfo>> FindAllFiles(string indexValue)
        {
            if (string.IsNullOrWhiteSpace(indexValue))
                return Result<List<FileInfo>>.Failure("索引值不能为空");

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
                        return Result<List<FileInfo>>.Failure($"未找到包含索引值 '{indexValue}' 的文件");

                    // 按最后写入时间倒序排列
                    var sortedFiles = matchingFiles.OrderByDescending(f => f.LastWriteTime).ToList();

                    return Result<List<FileInfo>>.Success(sortedFiles, $"找到 {sortedFiles.Count} 个匹配文件");
                }
                finally
                {
                    _lock.ExitReadLock();
                }
            }
            catch (Exception ex)
            {
                return Result<List<FileInfo>>.Failure($"搜索文件时发生错误: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// 根据多个索引值查找最新的文件（AND条件）
        /// </summary>
        /// <param name="indexValues">索引值数组</param>
        /// <returns>包含最新文件路径的结果</returns>
        public Result<string> FindLatestFileByMultipleIndexes(params string[] indexValues)
        {
            if (indexValues == null || indexValues.Length == 0)
                return Result<string>.Failure("索引值数组不能为空");

            if (indexValues.Any(string.IsNullOrWhiteSpace))
                return Result<string>.Failure("索引值不能包含空值");

            try
            {
                _lock.EnterReadLock();
                try
                {
                    var matchingFiles = new List<FileInfo>();

                    foreach (var filePath in Directory.EnumerateFiles(_searchDirectory, $"*{_fileExtension}", SearchOption.TopDirectoryOnly))
                    {
                        var fileName = Path.GetFileNameWithoutExtension(filePath);
                        
                        if (fileName != null && indexValues.All(idx => fileName.Contains(idx)))
                        {
                            matchingFiles.Add(new FileInfo(filePath));
                        }
                    }

                    if (matchingFiles.Count == 0)
                        return Result<string>.Failure($"未找到同时包含所有索引值的文件");

                    var latestFile = matchingFiles.OrderByDescending(f => f.LastWriteTime).First();

                    return Result<string>.Success(latestFile.FullName, $"找到 {matchingFiles.Count} 个匹配文件，返回最新文件");
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
        /// 使用正则表达式查找最新的文件
        /// </summary>
        /// <param name="pattern">正则表达式模式</param>
        /// <returns>包含最新文件路径的结果</returns>
        public Result<string> FindLatestFileByRegex(string pattern)
        {
            if (string.IsNullOrWhiteSpace(pattern))
                return Result<string>.Failure("正则表达式不能为空");

            try
            {
                var regex = new Regex(pattern, RegexOptions.IgnoreCase);
                _lock.EnterReadLock();
                try
                {
                    var matchingFiles = new List<FileInfo>();

                    foreach (var filePath in Directory.EnumerateFiles(_searchDirectory, $"*{_fileExtension}", SearchOption.TopDirectoryOnly))
                    {
                        var fileName = Path.GetFileName(filePath);
                        
                        if (fileName != null && regex.IsMatch(fileName))
                        {
                            matchingFiles.Add(new FileInfo(filePath));
                        }
                    }

                    if (matchingFiles.Count == 0)
                        return Result<string>.Failure($"未找到匹配正则表达式 '{pattern}' 的文件");

                    var latestFile = matchingFiles.OrderByDescending(f => f.LastWriteTime).First();

                    return Result<string>.Success(latestFile.FullName, $"找到 {matchingFiles.Count} 个匹配文件，返回最新文件");
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
        /// 获取目录中所有文件的数量
        /// </summary>
        /// <returns>文件数量</returns>
        public int GetFileCount()
        {
            _lock.EnterReadLock();
            try
            {
                return Directory.EnumerateFiles(_searchDirectory, $"*{_fileExtension}", SearchOption.TopDirectoryOnly).Count();
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        /// <summary>
        /// 获取目录中所有索引值的列表（根据文件名中的分隔符提取）
        /// </summary>
        /// <param name="separator">分隔符（默认下划线）</param>
        /// <param name="indexPosition">索引值在文件名中的位置（0-based，默认0）</param>
        /// <returns>索引值列表</returns>
        public Result<List<string>> GetAllIndexValues(char separator = '_', int indexPosition = 0)
        {
            try
            {
                _lock.EnterReadLock();
                try
                {
                    var indexValues = new HashSet<string>();

                    foreach (var filePath in Directory.EnumerateFiles(_searchDirectory, $"*{_fileExtension}", SearchOption.TopDirectoryOnly))
                    {
                        var fileName = Path.GetFileNameWithoutExtension(filePath);
                        
                        if (string.IsNullOrEmpty(fileName))
                            continue;

                        var parts = fileName.Split(separator);
                        if (parts.Length > indexPosition)
                        {
                            indexValues.Add(parts[indexPosition]);
                        }
                    }

                    return Result<List<string>>.Success(indexValues.ToList(), $"找到 {indexValues.Count} 个不同的索引值");
                }
                finally
                {
                    _lock.ExitReadLock();
                }
            }
            catch (Exception ex)
            {
                return Result<List<string>>.Failure($"获取索引值列表时发生错误: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// 清理旧文件，只保留最新的N个文件
        /// </summary>
        /// <param name="indexValue">索引值</param>
        /// <param name="keepCount">保留的文件数量</param>
        /// <returns>清理结果</returns>
        public Result<int> CleanupOldFiles(string indexValue, int keepCount)
        {
            if (string.IsNullOrWhiteSpace(indexValue))
                return Result<int>.Failure("索引值不能为空");

            if (keepCount < 1)
                return Result<int>.Failure("保留数量必须大于0");

            try
            {
                _lock.EnterWriteLock();
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

                    if (matchingFiles.Count <= keepCount)
                        return Result<int>.Success(0, "文件数量未超过限制，无需清理");

                    // 按时间倒序排列，删除多余的旧文件
                    var filesToDelete = matchingFiles.OrderByDescending(f => f.LastWriteTime).Skip(keepCount).ToList();
                    int deletedCount = 0;

                    foreach (var file in filesToDelete)
                    {
                        try
                        {
                            file.Delete();
                            deletedCount++;
                        }
                        catch (Exception ex)
                        {
                            // 记录错误但继续处理其他文件
                            System.Diagnostics.Debug.WriteLine($"删除文件失败 {file.FullName}: {ex.Message}");
                        }
                    }

                    return Result<int>.Success(deletedCount, $"成功清理 {deletedCount} 个旧文件");
                }
                finally
                {
                    _lock.ExitWriteLock();
                }
            }
            catch (Exception ex)
            {
                return Result<int>.Failure($"清理文件时发生错误: {ex.Message}", ex);
            }
        }
    }
}
