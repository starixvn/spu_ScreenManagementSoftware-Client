using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;

namespace XiboClient.Logic
{
    [Serializable]
    public class Command
    {
        private const string UPGRADEVERSION_FOLDER = @"\SMSClientInstaller";
        private const string UPGRADEVERSION_FILENAME = @"\SMSClientInstaller.zip";
        private const string UPGRADEVERSION_INSTALLATIONFILENAME = @"\SMSInstallWrapper.exe";

        public string Code;
        public string CommandString;
        public string Validation;
        public string AdditionalInfo;

        public bool notifyStatus()
        {
            return !string.IsNullOrEmpty(Validation);
        }

        /// <summary>
        /// Run the Command
        /// </summary>
        /// <returns>true on success</returns>
        public bool Run()
        {
            if (string.IsNullOrEmpty(CommandString))
                throw new ArgumentNullException("Command string is empty, please check your Display Profile " + Code + " command for a valid command string.");

            // Parse the command string to work out how we should run this command.
            if (CommandString.StartsWith("rs232"))
            {
                Rs232Command rs232 = new Rs232Command(this);
                string line = rs232.Run();

                if (notifyStatus())
                {
                    return line == Validation;
                }
                else
                {
                    return true;
                }
            }
            else if (Code == "RestartPlayer")
            {
                return RestartPlayer();
            }
            else if (Code == "UpgradeVersion")
            {
                return GetUpdate();
            }
            else
            {
                // Process with CMD
                using (Process process = new Process())
                {
                    ProcessStartInfo startInfo = new ProcessStartInfo();

                    startInfo.CreateNoWindow = true;
                    startInfo.WindowStyle = ProcessWindowStyle.Hidden;
                    startInfo.FileName = "cmd.exe";
                    startInfo.Arguments = "/C " + CommandString;
                    startInfo.UseShellExecute = false;

                    if (notifyStatus())
                        startInfo.RedirectStandardOutput = true;

                    process.StartInfo = startInfo;
                    process.Start();

                    if (notifyStatus())
                    {
                        string line = "";
                        while (!process.StandardOutput.EndOfStream)
                        {
                            line += process.StandardOutput.ReadLine();
                        }

                        return line == Validation;
                    }
                    else
                        return true;
                }
            }
        }

        /// <summary>
        /// Get a command from Application Settings based on its Command Code
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        public static Command GetByCode(string code)
        {
            foreach (Command command in ApplicationSettings.Default.Commands)
            {
                if (command.Code == code)
                    return command;
            }

            throw new KeyNotFoundException("Command Not Found");
        }

        private bool RestartPlayer()
        {
            Process[] processes = Process.GetProcessesByName("SMSClient");

            Thread.Sleep(500);
            foreach (var process in processes)
            {
                process.Kill();
            }

            return true;
        }

        private bool GetUpdate()
        {
            var upgradeVersionFileName = AdditionalInfo;

            using (xmds.xmds xmds = new xmds.xmds())
            {
                xmds.Credentials = null;
                xmds.Url = ApplicationSettings.Default.XiboClient_xmds_xmds;
                xmds.UseDefaultCredentials = true;

                var installerFile = xmds.CheckAndUpgradeNewVersion(ApplicationSettings.Default.ClientVersion + "." + ApplicationSettings.Default.ClientCodeVersion.ToString(), upgradeVersionFileName, false);

                if (installerFile != null)
                {
                    using (FileStream fileStream = File.Open(ApplicationSettings.Default.LibraryPath + UPGRADEVERSION_FILENAME, FileMode.Create, FileAccess.Write, FileShare.Read))
                    {
                        fileStream.Write(installerFile, 0, installerFile.Length);
                        fileStream.Close();
                    }

                    InstallNewVersion();
                }
            }

            return true;
        }

        private void InstallNewVersion()
        {
            ExtractNewVersionFile();

            Process.Start(ApplicationSettings.Default.LibraryPath + UPGRADEVERSION_FOLDER + UPGRADEVERSION_INSTALLATIONFILENAME);
        }

        private void ExtractNewVersionFile()
        {
            if (Directory.Exists(ApplicationSettings.Default.LibraryPath + UPGRADEVERSION_FOLDER))
            {
                Directory.Delete(ApplicationSettings.Default.LibraryPath + UPGRADEVERSION_FOLDER, true);
            }

            Thread.Sleep(1000);
            System.IO.Compression.ZipFile.ExtractToDirectory(ApplicationSettings.Default.LibraryPath + UPGRADEVERSION_FILENAME, ApplicationSettings.Default.LibraryPath + UPGRADEVERSION_FOLDER);
            Thread.Sleep(1000);
        }
    }
}