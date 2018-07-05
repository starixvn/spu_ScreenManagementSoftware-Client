using SMS.Testing.Console.Logic;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
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
        private HardwareKey _hardwareKey;
        private static readonly object _syncObject = new object();

        private static string mediaPath = @"D:\SMS_Media";

        public void Run()
        {
            _hardwareKey = new HardwareKey();
            try
            {
                var filenames = ConfigurationManager.AppSettings["media.Name"].Split(';');

                var apiUrl = ConfigurationManager.AppSettings["api.url"];
                
                if (!Directory.Exists(mediaPath))
                {
                    Directory.CreateDirectory(mediaPath);
                }

                foreach (var filename in filenames)
                {
                    var url = new Uri(apiUrl + "/download.php?file=" + filename + "&key=" + _hardwareKey.Key);
                    var webClient = new WebClient();
                    webClient.DownloadProgressChanged += new DownloadProgressChangedEventHandler((sender, e) => wc_DownloadProgressChanged(sender, e, filename));
                    webClient.DownloadFileCompleted += DownloadCompleted;
                    webClient.DownloadFileAsync(url, Path.Combine(mediaPath, filename));
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
                File.AppendAllText(Path.Combine(mediaPath, "log.txt"), filename + " is downloading on " + string.Format("{0:0.00}", percentage) + "% complete...\n");
            }

            System.Console.WriteLine(filename + " is downloading on " + string.Format("{0:0.00}", percentage) + "% complete...");
        }
    }
}
