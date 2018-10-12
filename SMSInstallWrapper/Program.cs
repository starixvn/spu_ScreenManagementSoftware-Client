namespace SMSInstallWrapper
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Net;
    using System.Threading;
    using System.Threading.Tasks;

    internal class Program
    {
        private const string PRODUCTCODE = "{1169316E-B092-48F5-951D-7CF486C35F93}";
        private const string SMSINSTALLER = @"\SMSClientInstaller.msi";

        //private const string ADOBEFLASHINSTALLER = @"\Adobe_Flash_Player_26_PPAPI_sib.msi";
        private static string currentDirectory = Directory.GetCurrentDirectory();

        private static void Main(string[] args)
        {
            try
            {
                NormalizeRegistry();

                if (!File.Exists(currentDirectory + SMSINSTALLER))
                {
                    currentDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + @"\SMS Client Library\SMSClientInstaller";
                }

                System.Console.WriteLine("Current Directory: " + currentDirectory);

                System.Console.WriteLine("Current App's Directory");
                KillOldProcess();

                Thread.Sleep(500);

                System.Console.WriteLine("Remove the old version");
                UninstallSMSClient();

#if !UPGRADE
                System.Console.WriteLine("Remove old cached files");
                DeleteCachedFiles();
#endif

                //System.Console.WriteLine("Install Adobe Flash Player");
                //InstallFlash();

                Thread.Sleep(500);

                System.Console.WriteLine("Install the new version");
                InstallSMSClient();

                System.Console.WriteLine("Done!");

                Thread.Sleep(500);

                Restart();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error while running wrapper");

                Console.WriteLine($"Error type: {ex.GetType().FullName}");

                Console.WriteLine($"stack:\n {ex.StackTrace}");

                Console.ReadLine();
            }
        }

        private static void KillOldProcess()
        {
            var task = Task.Factory.StartNew(() =>
            {
                var processes1 = Process.GetProcessesByName("SMSClientWatchdog");
                var processes2 = Process.GetProcessesByName("SMSClient");

                foreach (var process in processes1)
                {
                    process.Kill();
                }

                foreach (var process in processes2)
                {
                    process.Kill();
                }
            });

            task.Wait();
        }

        //private static void InstallFlash()
        //{
        //    if (File.Exists(currentDirectory + ADOBEFLASHINSTALLER))
        //    {
        //        System.Console.WriteLine(currentDirectory + ADOBEFLASHINSTALLER);

        //        var task = Task.Factory.StartNew(() =>
        //        {
        //            var file = "\"" + currentDirectory + ADOBEFLASHINSTALLER + "\"";

        //            using (Process process = new Process())
        //            {
        //                ProcessStartInfo startInfo = new ProcessStartInfo();
        //                startInfo.WorkingDirectory = currentDirectory;
        //                startInfo.CreateNoWindow = true;
        //                startInfo.WindowStyle = ProcessWindowStyle.Hidden;
        //                startInfo.FileName = @"msiexec.exe";
        //                startInfo.Arguments = @"/i " + file + " /passive";
        //                startInfo.UseShellExecute = false;
        //                startInfo.Verb = "runas";
        //                process.EnableRaisingEvents = true;
        //                process.StartInfo = startInfo;
        //                process.Start();
        //                process.WaitForExit();
        //                process.Dispose();
        //            }
        //        });

        //        task.Wait();
        //    }
        //}

        private static void UninstallSMSClient()
        {
            var task = Task.Factory.StartNew(() =>
            {
                using (Process process = new Process())
                {
                    ProcessStartInfo startInfo = new ProcessStartInfo();

                    startInfo.CreateNoWindow = true;
                    startInfo.WindowStyle = ProcessWindowStyle.Hidden;
                    startInfo.FileName = @"msiexec";
                    startInfo.Arguments = @"/x " + PRODUCTCODE + " /passive";
                    startInfo.UseShellExecute = false;
                    startInfo.Verb = "runas";
                    process.EnableRaisingEvents = true;
                    process.StartInfo = startInfo;
                    process.Start();
                    process.WaitForExit();
                    process.Dispose();
                }
            });

            task.Wait();
        }

        private static void DeleteCachedFiles()
        {
            try
            {
                var appdata = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                var document = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

                var xmlPath = appdata + "/SMSClient.xml";
                var smsFolder = document + "/SMS Client Library";

                if (File.Exists(xmlPath)) File.Delete(xmlPath);
                if (Directory.Exists(smsFolder))
                {
                    foreach (var file in Directory.GetFiles(smsFolder))
                    {
                        if (!file.Contains("msi"))
                        {
                            File.Delete(file);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("{0} - {1}", e.GetType().Name, e.Message);
            }
        }

        private static void InstallSMSClient()
        {
            if (File.Exists(currentDirectory + SMSINSTALLER))
            {
                System.Console.WriteLine(currentDirectory + SMSINSTALLER);

                var task = Task.Factory.StartNew(() =>
                {
                    var file = "\"" + currentDirectory + SMSINSTALLER + "\"";

                    using (Process process = new Process())
                    {
                        ProcessStartInfo startInfo = new ProcessStartInfo();

                        startInfo.CreateNoWindow = true;
                        startInfo.WindowStyle = ProcessWindowStyle.Hidden;
                        startInfo.FileName = @"msiexec";
                        startInfo.Arguments = @"/i " + file + " /passive";
                        startInfo.UseShellExecute = false;
                        startInfo.Verb = "runas";

                        process.StartInfo = startInfo;
                        process.Start();
                        process.WaitForExit();
                        process.Dispose();
                    }
                });

                task.Wait();
            }
        }

        private static void Restart()
        {
            var proc = new ProcessStartInfo();
            proc.WindowStyle = ProcessWindowStyle.Hidden;
            proc.FileName = "cmd";
            proc.Arguments = "/C shutdown -f -r -t 0";
            Process.Start(proc);
        }

        private static void DownloadDebuggingTool()
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;

            var url = "https://github.com/0xd4d/dnSpy/releases/download/v5.0.0/dnSpy.zip";

            var file = $"{currentDirectory}/dnspy.zip";

            Console.WriteLine($"Downloading debugger from {url}");

            Console.WriteLine($"Saving to {file}");

            using (var client = new WebClient())
            {
                try
                {
                    client.DownloadProgressChanged += (object sender, DownloadProgressChangedEventArgs e) => Console.Write($"\rProgress: {e.ProgressPercentage}%");
                    var tsk = client.DownloadFileTaskAsync(url, file);

                    Task.WaitAll(tsk);

                    Console.Write("\n");
                }
                catch (Exception e)
                {
                    Console.WriteLine($"\nDownloading failed, error: {e.Message}");
                    Console.WriteLine("Download aborted");
                }
            }
        }

        private static void NormalizeRegistry()
        {
            var cmd = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    WindowStyle = ProcessWindowStyle.Hidden,
                    FileName = "cmd",
                    Arguments = "/C lodctr /r",
                    UseShellExecute = false
                }
            };

            Console.WriteLine("Normalizing registry");

            cmd.Start();

            cmd.WaitForExit();
        }
    }
}