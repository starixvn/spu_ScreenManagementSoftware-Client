namespace SMSInstallWrapper
{
    using Microsoft.Win32;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Xml;

    class Program
    {
        private const string PRODUCTCODE = "{1169316E-B092-48F5-951D-7CF486C35F93}";
        private const string SMSINSTALLER = "SMSClientInstaller.msi";
        private const string ADOBEFLASHINSTALLER = "flashplayer26pp_xa_install.msi";

        static void Main(string[] args)
        {
            InstallPrerequisites();
        }

        private static void InstallPrerequisites()
        {
            using (Process process1 = new Process())
            {
                ProcessStartInfo startInfo = new ProcessStartInfo();

                startInfo.CreateNoWindow = true;
                startInfo.WindowStyle = ProcessWindowStyle.Hidden;
                startInfo.FileName = @"msiexec";
                startInfo.Arguments = @"/i " + ADOBEFLASHINSTALLER + " /passive";
                startInfo.UseShellExecute = false;
                //startInfo.Verb = "runas";
                process1.EnableRaisingEvents = true;
                process1.Exited += new EventHandler(CheckToUninstallSMSClient);
                process1.StartInfo = startInfo;
                process1.Start();
                process1.WaitForExit();
                process1.Close();
            }
        }

        private static void CheckToUninstallSMSClient(object sender, System.EventArgs e)
        {
            try
            {
                using (Process process2 = new Process())
                {
                    ProcessStartInfo startInfo = new ProcessStartInfo();

                    startInfo.CreateNoWindow = true;
                    startInfo.WindowStyle = ProcessWindowStyle.Hidden;
                    startInfo.FileName = @"msiexec";
                    startInfo.Arguments = @"/x " + PRODUCTCODE + " /passive";
                    startInfo.UseShellExecute = false;
                    startInfo.Verb = "runas";
                    process2.EnableRaisingEvents = true;
                    process2.Exited += new EventHandler(InstallSMSClient);
                    process2.StartInfo = startInfo;
                    process2.Start();
                    process2.WaitForExit();
                    process2.Close();
                }
            }
            catch
            {
                InstallSMSClient(null, null);
            }
        }

        private static void InstallSMSClient(object sender, System.EventArgs e)
        {
            using (Process process3 = new Process())
            {
                ProcessStartInfo startInfo = new ProcessStartInfo();

                startInfo.CreateNoWindow = true;
                startInfo.WindowStyle = ProcessWindowStyle.Hidden;
                startInfo.FileName = @"msiexec";
                startInfo.Arguments = @"/i " + SMSINSTALLER + " /passive";
                startInfo.UseShellExecute = false;
                //startInfo.Verb = "runas";

                process3.StartInfo = startInfo;
                process3.Start();
                process3.WaitForExit();
                process3.Close();
            }
        }
    }
}
