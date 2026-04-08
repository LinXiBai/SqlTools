using System;
using System.IO;

namespace CoreToolkit.Common.Models
{
    /// <summary>
    /// 路径配置管理类
    /// <para>统一管理应用程序的所有路径配置，方便维护和修改</para>
    /// <para>使用说明：</para>
    /// <para>1. 在应用程序启动时调用 Initialize() 方法初始化所有目录</para>
    /// <para>2. 通过属性访问各类路径，如：PathConfig.MainDatabase</para>
    /// <para>3. 可以自定义目录路径，如：PathConfig.DataDirectory = @"D:\MyApp\Data"</para>
    /// <para>4. 路径支持相对路径和绝对路径，自动处理路径拼接</para>
    /// </summary>
    /// <example>
    /// <code>
    /// // 初始化所有目录
    /// PathConfig.Initialize();
    /// 
    /// // 访问数据库路径
    /// string mainDb = PathConfig.MainDatabase;
    /// string logDb = PathConfig.LogDatabase;
    /// 
    /// // 自定义目录路径
    /// PathConfig.DataDirectory = @"D:\MyApp\Data";
    /// 
    /// // 获取完整路径（自动处理相对路径）
    /// string fullPath = PathConfig.GetFullPath(@"Data\Main.db");
    /// 
    /// // 检查目录是否存在
    /// if (PathConfig.DirectoryExists(PathConfig.DataDirectory))
    /// {
    ///     // 目录存在，执行操作
    /// }
    /// 
    /// // 清空临时目录
    /// PathConfig.ClearTempDirectory();
    /// </code>
    /// </example>
    public static class PathConfig
    {
        #region 字段

        private static string _baseDirectory;
        private static string _dataDirectory;
        private static string _logsDirectory;
        private static string _recipesDirectory;
        private static string _tempDirectory;
        private static string _configDirectory;

        #endregion

        #region 属性

        /// <summary>
        /// 获取或设置应用程序基础目录
        /// <para>默认值：AppDomain.CurrentDomain.BaseDirectory（应用程序根目录）</para>
        /// <para>说明：这是所有其他路径的基准目录</para>
        /// </summary>
        public static string BaseDirectory
        {
            get => _baseDirectory ?? (_baseDirectory = AppDomain.CurrentDomain.BaseDirectory);
            set => _baseDirectory = value;
        }

        /// <summary>
        /// 获取或设置数据库文件目录
        /// <para>默认值：BaseDirectory\Data</para>
        /// <para>说明：存放所有数据库文件的目录</para>
        /// </summary>
        public static string DataDirectory
        {
            get => _dataDirectory ?? (_dataDirectory = Path.Combine(BaseDirectory, "Data"));
            set => _dataDirectory = value;
        }

        /// <summary>
        /// 获取或设置日志文件目录
        /// <para>默认值：BaseDirectory\Logs</para>
        /// <para>说明：存放应用程序日志文件的目录</para>
        /// </summary>
        public static string LogsDirectory
        {
            get => _logsDirectory ?? (_logsDirectory = Path.Combine(BaseDirectory, "Logs"));
            set => _logsDirectory = value;
        }

        /// <summary>
        /// 获取或设置配方文件目录
        /// <para>默认值：BaseDirectory\Recipes</para>
        /// <para>说明：存放配方文件（Recipe）的目录</para>
        /// </summary>
        public static string RecipesDirectory
        {
            get => _recipesDirectory ?? (_recipesDirectory = Path.Combine(BaseDirectory, "Recipes"));
            set => _recipesDirectory = value;
        }

        /// <summary>
        /// 获取或设置临时文件目录
        /// <para>默认值：BaseDirectory\Temp</para>
        /// <para>说明：存放临时文件的目录，可通过 ClearTempDirectory() 清空</para>
        /// </summary>
        public static string TempDirectory
        {
            get => _tempDirectory ?? (_tempDirectory = Path.Combine(BaseDirectory, "Temp"));
            set => _tempDirectory = value;
        }

        /// <summary>
        /// 获取或设置配置文件目录
        /// <para>默认值：BaseDirectory\Config</para>
        /// <para>说明：存放配置文件的目录</para>
        /// </summary>
        public static string ConfigDirectory
        {
            get => _configDirectory ?? (_configDirectory = Path.Combine(BaseDirectory, "Config"));
            set => _configDirectory = value;
        }

        #endregion

        #region 数据库路径

        /// <summary>
        /// 获取主数据库文件的完整路径
        /// <para>路径：DataDirectory\Main.db</para>
        /// <para>用途：存放应用程序的主要业务数据</para>
        /// </summary>
        public static string MainDatabase => Path.Combine(DataDirectory, "Main.db");

        /// <summary>
        /// 获取日志数据库文件的完整路径
        /// <para>路径：DataDirectory\Log.db</para>
        /// <para>用途：存放应用程序的操作日志和系统日志</para>
        /// </summary>
        public static string LogDatabase => Path.Combine(DataDirectory, "Log.db");

        /// <summary>
        /// 获取用户数据库文件的完整路径
        /// <para>路径：DataDirectory\User.db</para>
        /// <para>用途：存放用户账户和权限信息</para>
        /// </summary>
        public static string UserDatabase => Path.Combine(DataDirectory, "User.db");

        /// <summary>
        /// 获取参数数据库文件的完整路径
        /// <para>路径：DataDirectory\Parameter.db</para>
        /// <para>用途：存放设备参数和系统配置参数</para>
        /// </summary>
        public static string ParameterDatabase => Path.Combine(DataDirectory, "Parameter.db");

        #endregion

        #region 初始化方法

        /// <summary>
        /// 初始化所有默认目录
        /// <para>创建以下目录（如果不存在）：</para>
        /// <para>- DataDirectory（数据库目录）</para>
        /// <para>- LogsDirectory（日志目录）</para>
        /// <para>- RecipesDirectory（配方目录）</para>
        /// <para>- TempDirectory（临时目录）</para>
        /// <para>- ConfigDirectory（配置目录）</para>
        /// </summary>
        /// <example>
        /// <code>
        /// // 在应用程序启动时调用
        /// PathConfig.Initialize();
        /// </code>
        /// </example>
        public static void Initialize()
        {
            CreateDirectoryIfNotExists(DataDirectory);
            CreateDirectoryIfNotExists(LogsDirectory);
            CreateDirectoryIfNotExists(RecipesDirectory);
            CreateDirectoryIfNotExists(TempDirectory);
            CreateDirectoryIfNotExists(ConfigDirectory);
        }

        /// <summary>
        /// 初始化指定的目录列表
        /// <para>创建传入的目录路径（如果不存在）</para>
        /// </summary>
        /// <param name="directories">目录路径数组，可以是相对路径或绝对路径</param>
        /// <example>
        /// <code>
        /// // 初始化单个目录
        /// PathConfig.Initialize(@"D:\MyApp\CustomDir");
        /// 
        /// // 初始化多个目录
        /// PathConfig.Initialize(
        ///     @"D:\MyApp\Data",
        ///     @"D:\MyApp\Logs",
        ///     @"D:\MyApp\Temp"
        /// );
        /// </code>
        /// </example>
        public static void Initialize(params string[] directories)
        {
            foreach (var dir in directories)
            {
                CreateDirectoryIfNotExists(dir);
            }
        }

        /// <summary>
        /// 重新初始化所有目录
        /// <para>1. 调用 Initialize() 重新创建所有目录</para>
        /// <para>2. 清空 TempDirectory（临时目录）中的所有内容</para>
        /// <para>说明：适用于需要重置应用程序状态的场景</para>
        /// </summary>
        /// <example>
        /// <code>
        /// // 重置应用程序状态
        /// PathConfig.Reinitialize();
        /// </code>
        /// </example>
        public static void Reinitialize()
        {
            Initialize();
            ClearTempDirectory();
        }

        #endregion

        #region 辅助方法

        /// <summary>
        /// 创建目录（如果不存在）
        /// <para>内部方法，用于确保目录存在</para>
        /// </summary>
        /// <param name="path">目录路径</param>
        /// <exception cref="InvalidOperationException">当目录创建失败时抛出</exception>
        private static void CreateDirectoryIfNotExists(string path)
        {
            try
            {
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"无法创建目录: {path}", ex);
            }
        }

        /// <summary>
        /// 清空临时目录中的所有内容
        /// <para>删除 TempDirectory 目录下的所有文件和子目录</para>
        /// <para>说明：此操作不可恢复，请谨慎使用</para>
        /// </summary>
        /// <exception cref="InvalidOperationException">当清空目录失败时抛出</exception>
        public static void ClearTempDirectory()
        {
            if (Directory.Exists(TempDirectory))
            {
                try
                {
                    var di = new DirectoryInfo(TempDirectory);
                    foreach (var file in di.GetFiles())
                    {
                        file.Delete();
                    }
                    foreach (var dir in di.GetDirectories())
                    {
                        dir.Delete(true);
                    }
                }
                catch (Exception ex)
                {
                    throw new InvalidOperationException($"无法清空临时目录: {TempDirectory}", ex);
                }
            }
        }

        /// <summary>
        /// 获取文件的完整路径（自动处理相对路径）
        /// <para>如果传入的是绝对路径，直接返回</para>
        /// <para>如果传入的是相对路径，会基于 baseDir 参数或 BaseDirectory 拼接</para>
        /// </summary>
        /// <param name="path">文件路径（可以是相对路径或绝对路径）</param>
        /// <param name="baseDir">基准目录，默认使用 BaseDirectory</param>
        /// <returns>完整的文件路径</returns>
        /// <example>
        /// &lt;code&gt;
        /// // 相对路径
        /// string fullPath1 = PathConfig.GetFullPath(@"Data\Main.db");
        /// // 结果：C:\MyApp\Data\Main.db
        /// 
        /// // 绝对路径
        /// string fullPath2 = PathConfig.GetFullPath(@"D:\Data\Main.db");
        /// // 结果：D:\Data\Main.db
        /// 
        /// // 指定基准目录
        /// string fullPath3 = PathConfig.GetFullPath("config.ini", @"D:\MyApp");
        /// // 结果：D:\MyApp\config.ini
        /// </code>
        /// </example>
        public static string GetFullPath(string path, string baseDir = null)
        {
            if (Path.IsPathRooted(path))
            {
                return path;
            }
            return Path.Combine(baseDir ?? BaseDirectory, path);
        }

        /// <summary>
        /// 检查目录是否存在
        /// </summary>
        /// <param name="path">目录路径</param>
        /// <returns>如果目录存在返回 true，否则返回 false</returns>
        /// <example>
        /// <code>
        /// if (PathConfig.DirectoryExists(PathConfig.DataDirectory))
        /// {
        ///     Console.WriteLine("数据目录存在");
        /// }
        /// else
        /// {
        ///     Console.WriteLine("数据目录不存在");
        /// }
        /// </code>
        /// </example>
        public static bool DirectoryExists(string path)
        {
            return Directory.Exists(path);
        }

        /// <summary>
        /// 检查文件是否存在
        /// </summary>
        /// <param name="path">文件路径</param>
        /// <returns>如果文件存在返回 true，否则返回 false</returns>
        /// <example>
        /// <code>
        /// if (PathConfig.FileExists(PathConfig.MainDatabase))
        /// {
        ///     Console.WriteLine("数据库文件存在");
        /// }
        /// else
        /// {
        ///     Console.WriteLine("数据库文件不存在");
        /// }
        /// </code>
        /// </example>
        public static bool FileExists(string path)
        {
            return File.Exists(path);
        }

        #endregion
    }
}
