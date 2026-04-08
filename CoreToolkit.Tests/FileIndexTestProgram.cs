using System;

namespace CoreToolkit.Tests
{
    /// <summary>
    /// 文件索引搜索器测试程序入口
    /// </summary>
    class FileIndexTestProgram
    {
        [STAThread]
        static void Main(string[] args)
        {
            Console.WriteLine("=================================================");
            Console.WriteLine("    FileIndexSearcher 20万文件测试程序");
            Console.WriteLine("=================================================");
            Console.WriteLine();
            
            FileIndexSearcherDirectTest.Run();
        }
    }
}
