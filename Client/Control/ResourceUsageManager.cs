namespace XiboClient.Control
{
    using System;
    using System.Diagnostics;

    class ResourceUsageManager
    {
        private const int RESOURCEUSAGE_INTERVAL = 20000;
        private static System.Timers.Timer resourceUsageTimer;

        public static void Start()
        {
            resourceUsageTimer = new System.Timers.Timer(RESOURCEUSAGE_INTERVAL);
            resourceUsageTimer.Elapsed += CollectClientResourceUsage;
            resourceUsageTimer.Start();            
        }

        private static void CollectClientResourceUsage(object sender, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                PerformanceCounter cpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");
                PerformanceCounter ramCounter = new PerformanceCounter("Memory", "Available MBytes");
                int cpuUsage = (int)cpuCounter.NextValue();
                System.Threading.Thread.Sleep(500);
                cpuUsage = (int)cpuCounter.NextValue();
                long ramTotal = (long)(new Microsoft.VisualBasic.Devices.ComputerInfo()).TotalPhysicalMemory / 1024;
                long ramAvailable = (long)ramCounter.NextValue() * 1024;

                using(xmds.xmds xmds1 = new xmds.xmds())
                {
                    var result = xmds1.StoreClientCpuRamUsage(ApplicationSettings.Default.HardwareKey, cpuUsage, ramTotal, ramAvailable);
                    Trace.WriteLine(new LogMessage("Client Resource Collector", "CPU Usage: " + cpuUsage + "%; RAM Available: "+ ramAvailable), LogType.Info.ToString());
                }
            }
            catch (Exception err)
            {
                resourceUsageTimer.Stop();
                Trace.WriteLine(new LogMessage("Client Resource Collector", "Error: " + err.Message), LogType.Error.ToString());
            }
        }
    }
}
