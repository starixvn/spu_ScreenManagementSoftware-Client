using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace SMS.Testing.Console.Modules
{
    public class DownloadMediaFile
    {
        static DateTime started = DateTime.Now;
        private static readonly object _syncObject = new object();

        public void Download()
        {
            try
            {
                var filenames = new string[] { "20.mp4", "21.mp4", "22.mp4", "23.mp4", "24.mp4", "25.mp4", "26.mp4", "27.mp4", "28.mp4", "29.mp4" };

                var apiUrl = System.Configuration.ConfigurationManager.AppSettings["api.url"];

                foreach (var filename in filenames)
                {
                    var url = new Uri(apiUrl + "/download.php?file=" + filename + "&key=508510b9ffa848cb9ce50fce333aec76");
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

        private void DownloadCompleted(object sender, AsyncCompletedEventArgs e)
        {
            System.Console.WriteLine("Success");

            var elapsedTime = DateTime.Now - started;

            System.Console.WriteLine(elapsedTime.TotalSeconds + "s");
        }

        private void wc_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e, string filename)
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
    }
}
