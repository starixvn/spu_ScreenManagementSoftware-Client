/*
 * Xibo - Digitial Signage - http://www.xibo.org.uk
 * Copyright (C) 2006-2015 Daniel Garner and the Xibo Developers
 *
 * This file is part of Xibo.
 *
 * Xibo is free software: you can redistribute it and/or modify
 * it under the terms of the GNU Affero General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * any later version. 
 *
 * Xibo is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU Affero General Public License for more details.
 *
 * You should have received a copy of the GNU Affero General Public License
 * along with Xibo.  If not, see <http://www.gnu.org/licenses/>.
 */
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Diagnostics;
using XiboClient.Logic;
using System.Threading.Tasks;
using System.Xml;
using XiboClient.Properties;
using System.IO;
using System.Net.NetworkInformation;
using System.Text;

namespace XiboClient
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static int Main(string[] args)
        {
            //WriteLog("Start ...");
            NativeMethods.SetErrorMode(NativeMethods.SetErrorMode(0) |
                           ErrorModes.SEM_NOGPFAULTERRORBOX |
                           ErrorModes.SEM_FAILCRITICALERRORS |
                           ErrorModes.SEM_NOOPENFILEERRORBOX);


            // Ensure our process has the highest priority
            Process.GetCurrentProcess().PriorityClass = ProcessPriorityClass.RealTime;

            Application.SetCompatibleTextRenderingDefault(false);

#if !DEBUG
            // Catch unhandled exceptions
            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);
            Application.ThreadException += new System.Threading.ThreadExceptionEventHandler(Application_ThreadException);
            TaskScheduler.UnobservedTaskException += TaskScheduler_UnobservedTaskException;
#endif
            Trace.Listeners.Add(new XiboTraceListener());
            
            try
            {
                // Check for any passed arguments
                if (args.Length > 0)
                {
                    if (args[0].ToString() == "RegisterNewPlayer")
                    {
                        RunSettings();
                        //WriteLog("Register player ...");
                        //RegisterXibo();
                    }
                    else
                    {
                        WriteLog("There is some args: " + args[0].ToLower().Trim().Substring(0, 2));
                        switch (args[0].ToLower().Trim().Substring(0, 2))
                        {
                            // Preview the screen saver
                            case "/p":
                                // args[1] is the handle to the preview window
                                KeyInterceptor.SetHook();
                                MouseInterceptor.SetHook();
                                RunClient(new IntPtr(long.Parse(args[1])));
                                KeyInterceptor.UnsetHook();
                                MouseInterceptor.UnsetHook();
                                break;

                            // Show the screen saver
                            case "/s":
                                KeyInterceptor.SetHook();
                                MouseInterceptor.SetHook();
                                RunClient(true);
                                KeyInterceptor.UnsetHook();
                                MouseInterceptor.UnsetHook();
                                break;

                            // Configure the screesaver's settings
                            case "/c":
                                // Show the settings form
                                RunSettings();
                                break;

                            // Show the screen saver
                            default:
                                KeyInterceptor.SetHook();
                                MouseInterceptor.SetHook();
                                RunClient(true);
                                KeyInterceptor.UnsetHook();
                                MouseInterceptor.UnsetHook();
                                break;
                        }
                    }
                }
                else
                {
                    // Add a message filter
                    Application.AddMessageFilter(KeyStore.Instance);

                    // No arguments were passed - we run the usual client
                    //WriteLog("Run Client");
                    RunClient();
                }
            }
            catch (Exception ex)
            {
                HandleUnhandledException(ex);
            }

            // Always flush at the end
            Trace.WriteLine(new LogMessage("Main", "Application Finished"), LogType.Info.ToString());
            Trace.Flush();

            return 0;
        }

        private static void RunClient()
        {
            Trace.WriteLine(new LogMessage("Main", "Client Started"), LogType.Info.ToString());
            Application.Run(new MainForm());
        }

        private static void RunClient(bool screenSaver)
        {
            Trace.WriteLine(new LogMessage("Main", "Client Started"), LogType.Info.ToString());
            Application.Run(new MainForm(screenSaver));
        }

        private static void RunClient(IntPtr previewWindow)
        {
            Trace.WriteLine(new LogMessage("Main", "Client Started"), LogType.Info.ToString());
            Application.Run(new MainForm(previewWindow));
        }

        private static void RegisterXibo()
        {
            // Also tweak the address of the xmds1
            var xmds1 = new xmds.xmds();
            xmds1.Url = ApplicationSettings.Default.XiboClient_xmds_xmds;

            // Commit these changes back to the user settings
            var optionForm = new OptionForm();
            var hardwareKey = new HardwareKey();
            optionForm.buttonSaveSettings.PerformClick();

            // Call register
            xmds1.RegisterDisplayAsync(
                ApplicationSettings.Default.ServerKey,
                ApplicationSettings.Default.HardwareKey,
                ApplicationSettings.Default.DisplayName,
                //"JohnTest",
                //ApplicationSettings.Default.HardwareKey,
                //"LongLuuDesktop",
                "windows",
                ApplicationSettings.Default.ClientVersion,
                ApplicationSettings.Default.ClientCodeVersion,
                Environment.OSVersion.ToString(),
                hardwareKey.MacAddress,
                hardwareKey.Channel,
                hardwareKey.getXmrPublicKey());

            WriteLog("Register completed ...");

            XmrSubscriber a = new XmrSubscriber();
            a.Run();

            XiboClient.XmdsAgents.RegisterAgent b = new XiboClient.XmdsAgents.RegisterAgent();
            b.Run();

            // reload the Xibo exe
            //Process.Start(Application.ExecutablePath);
        }

        private static void WriteLog(string content)
        {
            using (var sw = new StreamWriter(File.Open(@"C:\XiboLog.txt", FileMode.Append), Encoding.GetEncoding("iso-8859-1")))
            {
                sw.WriteLine(DateTime.Now.ToString() + ": " + content);
            }
        }

        private static void RunSettings()
        {
            // If we are showing the options form, enable visual styles
            Application.EnableVisualStyles(); // donot wanna show the form

            Trace.WriteLine(new LogMessage("Main", "Options Started"), LogType.Info.ToString());
            Application.Run(new OptionForm());
        }

        private static void RunSettingsByLong()
        {
            Trace.WriteLine(new LogMessage("Main", "Options Started"), LogType.Info.ToString());

            var optionForm = new OptionForm();

            optionForm.textBoxXmdsUri.Text = ApplicationSettings.Default.ServerUri;
            optionForm.textBoxServerKey.Text = ApplicationSettings.Default.ServerKey;
            optionForm.textBoxLibraryPath.Text = ApplicationSettings.Default.LibraryPath;
            string systemDriveLetter = Path.GetPathRoot(Environment.SystemDirectory);

            // Calculate the Hardware key from the CPUID and Volume Serial
            optionForm.tbHardwareKey.Text = Hashes.MD5(GetCPUId() + GetVolumeSerial(systemDriveLetter[0].ToString()) + GetMacAddress());

            //Application.Run(optionForm);
            
            optionForm.buttonSaveSettings.PerformClick();
            //System.Threading.Thread.Sleep(1000);
            //optionForm.button1.PerformClick();
        }

        private static string GetCPUId()
        {
            Debug.WriteLine("[IN]", "GetCPUId");

            string cpuInfo = String.Empty;
            string temp = String.Empty;
            System.Management.ManagementClass mc = new System.Management.ManagementClass("Win32_Processor");
            System.Management.ManagementObjectCollection moc = mc.GetInstances();
            foreach (System.Management.ManagementObject mo in moc)
            {
                if (cpuInfo == String.Empty)
                {   // only return cpuInfo from first CPU
                    cpuInfo = mo.Properties["ProcessorId"].Value.ToString();
                }
            }

            Debug.WriteLine("[OUT]", "GetCPUId");

            return cpuInfo;
        }
        private static string GetVolumeSerial(string strDriveLetter)
        {
            Debug.WriteLine("[IN]", "GetVolumeSerial");

            if (strDriveLetter == "" || strDriveLetter == null) strDriveLetter = "C";
            System.Management.ManagementObject disk = new System.Management.ManagementObject("win32_logicaldisk.deviceid=\"" + strDriveLetter + ":\"");
            disk.Get();

            System.Diagnostics.Debug.WriteLine("[OUT]", "GetVolumeSerial");

            return disk["VolumeSerialNumber"].ToString();
        }
        private static string GetMacAddress()
        {
            string macAddresses = string.Empty;

            try
            {
                foreach (NetworkInterface nic in NetworkInterface.GetAllNetworkInterfaces())
                {
                    if (nic.OperationalStatus == OperationalStatus.Up)
                    {
                        macAddresses += BitConverter.ToString(nic.GetPhysicalAddress().GetAddressBytes()).Replace('-', ':');
                        break;
                    }
                }
            }
            catch
            {
                macAddresses = "00:00:00:00:00:00";
            }

            return macAddresses;
        }

        static void Application_ThreadException(object sender, System.Threading.ThreadExceptionEventArgs e)
        {
            HandleUnhandledException(e.Exception);
        }

        static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            HandleUnhandledException(e.ExceptionObject);
        }

        static void TaskScheduler_UnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs e)
        {
            HandleUnhandledException(e.Exception);
        }

        /// <summary>
        /// Event for unhandled exceptions
        /// </summary>
        /// <param name="o"></param>
        static void HandleUnhandledException(Object o)
        {
            Exception e = o as Exception;

            // What happens if we cannot start?
            Trace.WriteLine(new LogMessage("Main", "Unhandled Exception: " + e.Message), LogType.Error.ToString());
            Trace.WriteLine(new LogMessage("Main", "Stack Trace: " + e.StackTrace), LogType.Audit.ToString());

            try
            {
                // Also write to the event log
                try
                {
                    if (!EventLog.SourceExists(Application.ProductName))
                        EventLog.CreateEventSource(Application.ProductName, "Xibo");
                    EventLog.WriteEntry(Application.ProductName, e.ToString(), EventLogEntryType.Error);
                }
                catch (Exception ex)
                {
                    Trace.WriteLine(new LogMessage("Main", "Couldn't write to event log: " + ex.Message), LogType.Info.ToString());
                }

                Trace.Flush();
            }
            catch (Exception ex)
            {
                Trace.WriteLine(new LogMessage("Main", "Unable to write to event log " + ex.Message), LogType.Info.ToString());
            }

            // Exit the application and allow it to be restarted by the Watchdog.
            Application.Exit();
        }

        [DllImport("User32.dll")]
        public static extern int ShowWindowAsync(IntPtr hWnd , int swCommand);

        internal static class NativeMethods
        {
            [DllImport("kernel32.dll")]
            internal static extern ErrorModes SetErrorMode(ErrorModes mode);
        }

        [Flags]
        internal enum ErrorModes : uint
        {
            SYSTEM_DEFAULT = 0x0,
            SEM_FAILCRITICALERRORS = 0x0001,
            SEM_NOALIGNMENTFAULTEXCEPT = 0x0004,
            SEM_NOGPFAULTERRORBOX = 0x0002,
            SEM_NOOPENFILEERRORBOX = 0x8000
        }
    }
}