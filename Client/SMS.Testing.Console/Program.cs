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
                System.Console.WriteLine("**   1. Connect XMR & get messsages");
                System.Console.WriteLine("**   2. Download media files");
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
                        var connectXMR = new ConnectXMR();
                        connectXMR.SubscribeQueue();
                        break;
                    case "2":
                        break;
                    default:
                        System.Console.WriteLine("** This module is not available! Please try again!");
                        System.Console.WriteLine("**************************************************");
                        System.Console.WriteLine("**************************************************");
                        break;
                }
            }
            //DownloadMediaFileFromXMDS();
        }

        

        public static void DownloadMediaFileFromXMDS()
        {
            using (WebClient wc = new WebClient())
            {
                wc.DownloadFile("https://ha.nexdev.net:7710/xmds.php?file=lVUsvZmBtu7sy5wJX5PRPn85513EBBjW&displayId=26&type=M&itemId=11", @"D:\Demo\11.ttf");
            }
        }

        public static void Log(string logMessage, TextWriter w)
        {
            // only one thread can own this lock, so other threads
            // entering this method will wait here until lock is
            // available.
            lock (_syncObject)
            {
                w.WriteLine("{0} {1}", DateTime.Now.ToLongTimeString(),
                    DateTime.Now.ToLongDateString());
                w.WriteLine("  :");
                w.WriteLine("  :{0}", logMessage);
                w.WriteLine("-------------------------------");
                // Update the underlying file.
                w.Flush();
            }
        }

        static string GetPublicKey()
        {
            var apiUrl = System.Configuration.ConfigurationManager.AppSettings["api.url"];

            return Post(apiUrl + "/GetFile.php", "getkey=y");
        }

        static string Post(string url, string data)
        {
            try
            {
                byte[] buffer = Encoding.ASCII.GetBytes(data);
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);

                // Send request
                request.Method = "POST";
                request.ContentType = "application/x-www-form-urlencoded";
                request.ContentLength = buffer.Length;
                Stream postData = request.GetRequestStream();
                postData.Write(buffer, 0, buffer.Length);
                postData.Close();

                // Get and return response
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                Stream Answer = response.GetResponseStream();
                StreamReader answer = new StreamReader(Answer);

                return answer.ReadToEnd();
            }
            catch (Exception ex)
            {
                return "ERROR: " + ex.Message;
            }
        }
    }
}
