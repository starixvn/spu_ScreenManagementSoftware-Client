using NetMQ;
using NetMQ.Sockets;
using SMS.Testing.Console.Logic;
using SMS.Testing.Console.Modules;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;


namespace SMS.Testing.Console
{
    class Program
    {
        static DateTime started = DateTime.Now;
        private static readonly object _syncObject = new object();

        static void Main(string[] args)
        {
            string selectedModule;
            while (true)
            {
                System.Console.WriteLine("************* SMS Testing Console *************");
                System.Console.WriteLine("\n** Module lists:");
                System.Console.WriteLine("**   1. Check environment");
                System.Console.WriteLine("**   2. Connect XMR & get messsages");
                System.Console.WriteLine("**   3. Download media files");
                System.Console.WriteLine("**   0. EXIT");
                System.Console.Write("** Please select module: ");
                selectedModule = System.Console.ReadLine();
                switch (selectedModule)
                {
                    case "0":
                        System.Console.WriteLine("** The program is closing...!");
                        DateTime startTime = DateTime.Now;
                        while ((DateTime.Now - startTime).Seconds != 2)
                        {
                            System.Console.Write("*");
                            Thread.Sleep(500);
                        }

                        Environment.Exit(0);
                        break;
                    case "1":
                        var checkEnvir = new SmsEnvironment();
                        checkEnvir.Check();
                        break;
                    case "2":
                        var connectXMR = new ConnectXMR();
                        connectXMR.Run();
                        break;
                    case "3":
                        var downloader = new DownloadMediaFile();
                        downloader.Run();
                        break;
                    default:
                        System.Console.WriteLine("** This module is not available! Please try again!");
                        System.Console.WriteLine("**************************************************");
                        System.Console.WriteLine("**************************************************");
                        break;
                }
            }
        }
    }
}
