using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace CoreToolkit.Common
{
    /// <summary>
    /// 进程启动结果
    /// </summary>
    public class ProcessStartResult
    {
        /// <summary>
        /// 是否成功启动
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// 启动的进程对象（成功时）
        /// </summary>
        public Process Process { get; set; }

        /// <summary>
        /// 错误信息（失败时）
        /// </summary>
        public string ErrorMessage { get; set; }

        /// <summary>
        /// 进程ID（成功时）
        /// </summary>
        public int ProcessId { get; set; }

        /// <summary>
        /// 隐式转换为 bool
        /// </summary>
        public static implicit operator bool(ProcessStartResult result) => result?.Success ?? false;
    }

    /// <summary>
    /// 进程执行结果（等待退出时使用）
    /// </summary>
    public class ProcessExecuteResult : ProcessStartResult
    {
        /// <summary>
        /// 退出代码
        /// </summary>
        public int ExitCode { get; set; }

        /// <summary>
        /// 标准输出内容
        /// </summary>
        public string StandardOutput { get; set; }

        /// <summary>
        /// 标准错误内容
        /// </summary>
        public string StandardError { get; set; }

        /// <summary>
        /// 执行耗时（毫秒）
        /// </summary>
        public long ElapsedMilliseconds { get; set; }
    }

    /// <summary>
    /// 外部程序启动辅助类
    /// </summary>
    public static class ProcessHelper
    {
        #region 路径解析

        /// <summary>
        /// 解析可执行文件路径（支持相对路径和绝对路径）
        /// </summary>
        /// <param name="path">相对路径或绝对路径</param>
        /// <param name="baseDirectory">相对路径的基准目录（默认为当前应用程序目录）</param>
        /// <returns>绝对路径，如果找不到则返回 null</returns>
        public static string ResolveExecutablePath(string path, string baseDirectory = null)
        {
            if (string.IsNullOrWhiteSpace(path))
                return null;

            // 1. 检查是否为绝对路径
            if (Path.IsPathRooted(path))
            {
                return File.Exists(path) ? path : null;
            }

            // 2. 确定基准目录
            string baseDir = baseDirectory ?? AppDomain.CurrentDomain.BaseDirectory;

            // 3. 尝试在基准目录下查找
            string fullPath = Path.Combine(baseDir, path);
            if (File.Exists(fullPath))
            {
                return Path.GetFullPath(fullPath);
            }

            // 4. 尝试添加 .exe 扩展名查找
            if (!path.EndsWith(".exe", StringComparison.OrdinalIgnoreCase))
            {
                fullPath = Path.Combine(baseDir, path + ".exe");
                if (File.Exists(fullPath))
                {
                    return Path.GetFullPath(fullPath);
                }
            }

            // 5. 尝试在系统 PATH 环境变量中查找
            string pathEnv = Environment.GetEnvironmentVariable("PATH");
            if (!string.IsNullOrEmpty(pathEnv))
            {
                foreach (var searchPath in pathEnv.Split(Path.PathSeparator))
                {
                    if (string.IsNullOrWhiteSpace(searchPath))
                        continue;

                    fullPath = Path.Combine(searchPath, path);
                    if (File.Exists(fullPath))
                    {
                        return Path.GetFullPath(fullPath);
                    }

                    // 尝试添加 .exe 扩展名
                    if (!path.EndsWith(".exe", StringComparison.OrdinalIgnoreCase))
                    {
                        fullPath = Path.Combine(searchPath, path + ".exe");
                        if (File.Exists(fullPath))
                        {
                            return Path.GetFullPath(fullPath);
                        }
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// 检查可执行文件是否存在
        /// </summary>
        public static bool ExecutableExists(string path, string baseDirectory = null)
        {
            return ResolveExecutablePath(path, baseDirectory) != null;
        }

        #endregion

        #region 同步启动

        /// <summary>
        /// 启动外部程序（不等待退出）
        /// </summary>
        /// <param name="executablePath">可执行文件路径（相对或绝对）</param>
        /// <param name="arguments">命令行参数</param>
        /// <param name="workingDirectory">工作目录</param>
        /// <param name="baseDirectory">相对路径的基准目录</param>
        /// <returns>启动结果</returns>
        public static ProcessStartResult Start(
            string executablePath,
            string arguments = null,
            string workingDirectory = null,
            string baseDirectory = null)
        {
            var result = new ProcessStartResult();

            try
            {
                // 解析路径
                string resolvedPath = ResolveExecutablePath(executablePath, baseDirectory);
                if (resolvedPath == null)
                {
                    result.ErrorMessage = $"找不到可执行文件: {executablePath}";
                    return result;
                }

                // 创建工作目录
                string workDir = workingDirectory ?? Path.GetDirectoryName(resolvedPath);

                // 配置启动信息
                var startInfo = new ProcessStartInfo
                {
                    FileName = resolvedPath,
                    Arguments = arguments ?? string.Empty,
                    WorkingDirectory = workDir,
                    UseShellExecute = true,  // 使用外壳程序启动（支持文件关联）
                    CreateNoWindow = false   // 创建窗口
                };

                // 启动进程
                var process = Process.Start(startInfo);
                if (process == null)
                {
                    result.ErrorMessage = "进程启动失败";
                    return result;
                }

                result.Success = true;
                result.Process = process;
                result.ProcessId = process.Id;

                return result;
            }
            catch (Win32Exception ex)
            {
                result.ErrorMessage = $"启动失败: {ex.Message} (错误码: {ex.NativeErrorCode})";
                return result;
            }
            catch (Exception ex)
            {
                result.ErrorMessage = $"启动异常: {ex.Message}";
                return result;
            }
        }

        /// <summary>
        /// 启动外部程序并等待退出
        /// </summary>
        /// <param name="executablePath">可执行文件路径</param>
        /// <param name="arguments">命令行参数</param>
        /// <param name="workingDirectory">工作目录</param>
        /// <param name="baseDirectory">相对路径的基准目录</param>
        /// <param name="timeout">超时时间（毫秒，null 表示无限等待）</param>
        /// <param name="captureOutput">是否捕获输出</param>
        /// <returns>执行结果</returns>
        public static ProcessExecuteResult Execute(
            string executablePath,
            string arguments = null,
            string workingDirectory = null,
            string baseDirectory = null,
            int? timeout = null,
            bool captureOutput = true)
        {
            var result = new ProcessExecuteResult();
            var stopwatch = Stopwatch.StartNew();

            try
            {
                // 解析路径
                string resolvedPath = ResolveExecutablePath(executablePath, baseDirectory);
                if (resolvedPath == null)
                {
                    result.ErrorMessage = $"找不到可执行文件: {executablePath}";
                    return result;
                }

                // 创建工作目录
                string workDir = workingDirectory ?? Path.GetDirectoryName(resolvedPath);

                // 配置启动信息
                var startInfo = new ProcessStartInfo
                {
                    FileName = resolvedPath,
                    Arguments = arguments ?? string.Empty,
                    WorkingDirectory = workDir,
                    UseShellExecute = !captureOutput,  // 捕获输出时必须为 false
                    CreateNoWindow = captureOutput,    // 捕获输出时隐藏窗口
                    RedirectStandardOutput = captureOutput,
                    RedirectStandardError = captureOutput,
                    StandardOutputEncoding = System.Text.Encoding.UTF8,
                    StandardErrorEncoding = System.Text.Encoding.UTF8
                };

                // 启动进程
                using (var process = new Process { StartInfo = startInfo })
                {
                    var outputBuilder = new System.Text.StringBuilder();
                    var errorBuilder = new System.Text.StringBuilder();

                    if (captureOutput)
                    {
                        process.OutputDataReceived += (sender, e) =>
                        {
                            if (e.Data != null)
                                outputBuilder.AppendLine(e.Data);
                        };

                        process.ErrorDataReceived += (sender, e) =>
                        {
                            if (e.Data != null)
                                errorBuilder.AppendLine(e.Data);
                        };
                    }

                    if (!process.Start())
                    {
                        result.ErrorMessage = "进程启动失败";
                        return result;
                    }

                    if (captureOutput)
                    {
                        process.BeginOutputReadLine();
                        process.BeginErrorReadLine();
                    }

                    // 等待进程退出
                    bool exited;
                    if (timeout.HasValue)
                    {
                        exited = process.WaitForExit(timeout.Value);
                        if (!exited)
                        {
                            try
                            {
                                process.Kill();
                            }
                            catch { }
                            result.ErrorMessage = $"进程执行超时（{timeout.Value}ms）";
                            return result;
                        }
                    }
                    else
                    {
                        process.WaitForExit();
                    }

                    // 等待异步读取完成
                    if (captureOutput)
                    {
                        process.WaitForExit();
                    }

                    stopwatch.Stop();

                    result.Success = true;
                    result.ProcessId = process.Id;
                    result.ExitCode = process.ExitCode;
                    result.StandardOutput = captureOutput ? outputBuilder.ToString() : null;
                    result.StandardError = captureOutput ? errorBuilder.ToString() : null;
                    result.ElapsedMilliseconds = stopwatch.ElapsedMilliseconds;

                    return result;
                }
            }
            catch (Win32Exception ex)
            {
                stopwatch.Stop();
                result.ErrorMessage = $"启动失败: {ex.Message} (错误码: {ex.NativeErrorCode})";
                result.ElapsedMilliseconds = stopwatch.ElapsedMilliseconds;
                return result;
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                result.ErrorMessage = $"执行异常: {ex.Message}";
                result.ElapsedMilliseconds = stopwatch.ElapsedMilliseconds;
                return result;
            }
        }

        #endregion

        #region 异步启动

        /// <summary>
        /// 异步启动外部程序（不等待退出）
        /// </summary>
        public static Task<ProcessStartResult> StartAsync(
            string executablePath,
            string arguments = null,
            string workingDirectory = null,
            string baseDirectory = null)
        {
            return Task.Run(() => Start(executablePath, arguments, workingDirectory, baseDirectory));
        }

        /// <summary>
        /// 异步启动外部程序并等待退出
        /// </summary>
        public static Task<ProcessExecuteResult> ExecuteAsync(
            string executablePath,
            string arguments = null,
            string workingDirectory = null,
            string baseDirectory = null,
            int? timeout = null,
            bool captureOutput = true)
        {
            return Task.Run(() => Execute(executablePath, arguments, workingDirectory, baseDirectory, timeout, captureOutput));
        }

        #endregion

        #region 便捷方法

        /// <summary>
        /// 打开文件（使用系统默认程序）
        /// </summary>
        /// <param name="filePath">文件路径</param>
        /// <param name="baseDirectory">相对路径的基准目录</param>
        /// <returns>启动结果</returns>
        public static ProcessStartResult OpenFile(string filePath, string baseDirectory = null)
        {
            var result = new ProcessStartResult();

            try
            {
                // 解析路径
                string resolvedPath;
                if (Path.IsPathRooted(filePath))
                {
                    resolvedPath = filePath;
                }
                else
                {
                    string baseDir = baseDirectory ?? AppDomain.CurrentDomain.BaseDirectory;
                    resolvedPath = Path.Combine(baseDir, filePath);
                }

                if (!File.Exists(resolvedPath))
                {
                    result.ErrorMessage = $"找不到文件: {filePath}";
                    return result;
                }

                // 使用系统默认程序打开
                var startInfo = new ProcessStartInfo
                {
                    FileName = resolvedPath,
                    UseShellExecute = true
                };

                var process = Process.Start(startInfo);
                if (process == null)
                {
                    result.ErrorMessage = "打开文件失败";
                    return result;
                }

                result.Success = true;
                result.Process = process;
                result.ProcessId = process.Id;

                return result;
            }
            catch (Exception ex)
            {
                result.ErrorMessage = $"打开文件异常: {ex.Message}";
                return result;
            }
        }

        /// <summary>
        /// 打开 URL（使用默认浏览器）
        /// </summary>
        public static ProcessStartResult OpenUrl(string url)
        {
            var result = new ProcessStartResult();

            try
            {
                if (string.IsNullOrWhiteSpace(url))
                {
                    result.ErrorMessage = "URL 不能为空";
                    return result;
                }

                // 确保 URL 格式正确
                if (!url.StartsWith("http://", StringComparison.OrdinalIgnoreCase) &&
                    !url.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
                {
                    url = "https://" + url;
                }

                var startInfo = new ProcessStartInfo
                {
                    FileName = url,
                    UseShellExecute = true
                };

                var process = Process.Start(startInfo);
                if (process == null)
                {
                    result.ErrorMessage = "打开 URL 失败";
                    return result;
                }

                result.Success = true;
                result.Process = process;
                result.ProcessId = process.Id;

                return result;
            }
            catch (Exception ex)
            {
                result.ErrorMessage = $"打开 URL 异常: {ex.Message}";
                return result;
            }
        }

        /// <summary>
        /// 打开文件夹
        /// </summary>
        public static ProcessStartResult OpenFolder(string folderPath, string baseDirectory = null)
        {
            var result = new ProcessStartResult();

            try
            {
                // 解析路径
                string resolvedPath;
                if (Path.IsPathRooted(folderPath))
                {
                    resolvedPath = folderPath;
                }
                else
                {
                    string baseDir = baseDirectory ?? AppDomain.CurrentDomain.BaseDirectory;
                    resolvedPath = Path.Combine(baseDir, folderPath);
                }

                if (!Directory.Exists(resolvedPath))
                {
                    result.ErrorMessage = $"找不到文件夹: {folderPath}";
                    return result;
                }

                // 使用资源管理器打开
                var startInfo = new ProcessStartInfo
                {
                    FileName = "explorer.exe",
                    Arguments = $"\"{resolvedPath}\"",
                    UseShellExecute = false
                };

                var process = Process.Start(startInfo);
                if (process == null)
                {
                    result.ErrorMessage = "打开文件夹失败";
                    return result;
                }

                result.Success = true;
                result.Process = process;
                result.ProcessId = process.Id;

                return result;
            }
            catch (Exception ex)
            {
                result.ErrorMessage = $"打开文件夹异常: {ex.Message}";
                return result;
            }
        }

        #endregion

        #region 进程管理

        /// <summary>
        /// 检查进程是否正在运行
        /// </summary>
        public static bool IsProcessRunning(int processId)
        {
            try
            {
                var process = Process.GetProcessById(processId);
                return process != null && !process.HasExited;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 检查进程是否正在运行
        /// </summary>
        public static bool IsProcessRunning(string processName)
        {
            try
            {
                var processes = Process.GetProcessesByName(processName);
                return processes.Length > 0;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 终止进程
        /// </summary>
        public static bool KillProcess(int processId)
        {
            try
            {
                var process = Process.GetProcessById(processId);
                process.Kill();
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 终止进程
        /// </summary>
        public static bool KillProcess(string processName)
        {
            try
            {
                var processes = Process.GetProcessesByName(processName);
                foreach (var process in processes)
                {
                    process.Kill();
                }
                return processes.Length > 0;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 获取进程信息
        /// </summary>
        public static Process GetProcess(int processId)
        {
            try
            {
                return Process.GetProcessById(processId);
            }
            catch
            {
                return null;
            }
        }

        #endregion
    }
}
