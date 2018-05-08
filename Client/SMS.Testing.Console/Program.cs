using SMS.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace SMS.Testing.Console
{
    class Program
    {
        static DateTime started = DateTime.Now;
        private static readonly object _syncObject = new object();
        
        static void Main(string[] args)
        {
            try
            {
                //System.Console.Write("Input filename: ");
                //var filename = System.Console.ReadLine();
                var filenames = new string[] { "20.mp4", "21.mp4", "22.mp4", "23.mp4", "24.mp4", "25.mp4", "26.mp4", "27.mp4", "28.mp4", "29.mp4" };

                var apiUrl = System.Configuration.ConfigurationManager.AppSettings["api.url"];
                
                foreach (var filename in filenames)
                {
                    var url = new System.Uri(apiUrl + "/download.php?file=" + filename + "&key=508510b9ffa848cb9ce50fce333aec76");
                    var webClient = new WebClient();
                    webClient.DownloadProgressChanged += new DownloadProgressChangedEventHandler((sender, e) => wc_DownloadProgressChanged(sender, e, filename));
                    webClient.DownloadFileCompleted += DownloadCompleted;
                    webClient.DownloadFileAsync(url, @"D:\Demo\" + filename);
                }
                System.Console.ReadLine();
            }
            catch (Exception ex)
            {
                System.Console.WriteLine(ex.Message);
            }

        }
        public static void DownloadCompleted(object sender, AsyncCompletedEventArgs e)
        {           
            System.Console.WriteLine("Success");

            var elapsedTime = DateTime.Now - started;

            System.Console.WriteLine(elapsedTime.TotalSeconds + "s");
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


        static void wc_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e, string filename)
        {
            double bytesIn = double.Parse(e.BytesReceived.ToString());
            double totalBytes = double.Parse(e.TotalBytesToReceive.ToString());
            double percentage = bytesIn / totalBytes * 100;

            lock (_syncObject)
            {
                File.AppendAllText(@"D:\Demo\log.txt", filename + " is downloading on " + string.Format("{0:0.00}", percentage) + "% complete...\n");
            }

            System.Console.WriteLine(filename + " is downloading on " + string.Format("{0:0.00}", percentage) + "% complete...");

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
