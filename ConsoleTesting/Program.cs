using System;
using System.Diagnostics;
using System.Net.Http;

namespace ConsoleTesting
{
    class Program
    {
        static void Main(string[] args)
        {
            var _address = "http://10.70.70.210:8081/xmds/download.php?file=23.mp4&key=508510b9ffa848cb9ce50fce333aec76";

            var result = Downloader.Download(_address, @"D:\TempData\23.mp4", 5);

            Console.WriteLine($"Location: {result.FilePath}");
            Console.WriteLine($"Size: {result.Size}bytes");
            Console.WriteLine($"Time taken: {result.TimeTaken.Milliseconds}ms");
            Console.WriteLine($"Parallel: {result.ParallelDownloads}");

            Console.ReadKey();
        }
    }
}
