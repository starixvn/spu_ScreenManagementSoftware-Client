namespace SMSInstallWrapper
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;

    class Program
    {
        private const string PRODUCTCODE = "{1169316E-B092-48F5-951D-7CF486C35F93}";
        private const string SMSINSTALLER = @"\SMSClientInstaller.msi";
        private const string ADOBEFLASHINSTALLER = @"\Adobe_Flash_Player_26_PPAPI_sib.msi";
        private static string currentDirectory = Directory.GetCurrentDirectory();

        static void Main(string[] args)
        {
            if (!File.Exists(currentDirectory + SMSINSTALLER))
            {
                currentDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + @"\SMS Client Library\SMSClientInstaller";
            }

            System.Console.WriteLine("Duong dan hien tai: " + currentDirectory);

            System.Console.WriteLine("Dong chuong trinh hien tai");
            KillOldProcess();

            Thread.Sleep(1000);

            System.Console.WriteLine("Xoa ban cu");
            UninstallSMSClient();

            System.Console.WriteLine("Cai dat flash");
            InstallFlash();

            Thread.Sleep(1000);

            System.Console.WriteLine("Cai ban moi");
            InstallSMSClient();

            System.Console.WriteLine("Xonggggggggg!");

            Thread.Sleep(1000);

            Restart();
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

        private static void InstallFlash()
        {
            if (File.Exists(currentDirectory + ADOBEFLASHINSTALLER))
            {
                System.Console.WriteLine(currentDirectory + ADOBEFLASHINSTALLER);

                var task = Task.Factory.StartNew(() =>
                {
                    var file = "\"" + currentDirectory + ADOBEFLASHINSTALLER + "\"";

                    using (Process process = new Process())
                    {
                        ProcessStartInfo startInfo = new ProcessStartInfo();
                        startInfo.WorkingDirectory = currentDirectory;
                        startInfo.CreateNoWindow = true;
                        startInfo.WindowStyle = ProcessWindowStyle.Hidden;
                        startInfo.FileName = @"msiexec.exe";
                        startInfo.Arguments = @"/i " + file + " /passive";
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
        }

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
    }
}
