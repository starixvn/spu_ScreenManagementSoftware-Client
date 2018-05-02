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

        static void Main(string[] args)
        {
            try
            {
                System.Console.Write("Input filename: ");
                var filename = System.Console.ReadLine();
                   
                var apiUrl = System.Configuration.ConfigurationManager.AppSettings["api.url"];
                //var rsa = new SMS.Core.RSAtoPHPCryptography();
                //var publicKey = GetPublicKey();
                //rsa.LoadCertificateFromString(publicKey);
                //var encodeMessage = rsa.Encrypt(filename);
                var url = new System.Uri(apiUrl + "/download.php?file=" + filename);
                var webClient = new WebClient();
                webClient.DownloadProgressChanged += new DownloadProgressChangedEventHandler(wc_DownloadProgressChanged);
                webClient.DownloadFileCompleted += DownloadCompleted;
                webClient.DownloadFileAsync(url, @"D:\Demo\" + filename);

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

        static void wc_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            double bytesIn = double.Parse(e.BytesReceived.ToString());
            double totalBytes = double.Parse(e.TotalBytesToReceive.ToString());
            double percentage = bytesIn / totalBytes * 100;

            System.Console.WriteLine("Downloaded " + e.BytesReceived + " of " + e.TotalBytesToReceive + ". " + percentage + "% complete...");

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
