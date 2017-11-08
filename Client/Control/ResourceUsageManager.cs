namespace XiboClient.Control
{
    using System;
    using System.Diagnostics;

    class ResourceUsageManager
    {
        private static System.Timers.Timer resourceUsageTimer;

        public static void Start()
        {
            resourceUsageTimer = new System.Timers.Timer(ApplicationSettings.Default.ResourceUsageInterval);
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
                int ramTotal = (int)(new Microsoft.VisualBasic.Devices.ComputerInfo()).TotalPhysicalMemory / 1024 / 1024;
                int ramAvailable = (int)ramCounter.NextValue();
                int ramUsage = ramTotal == 0 ? 0 : (int)((float)(ramTotal - ramAvailable) / ramTotal * 100);

                if (ApplicationSettings.Default.IsForcedSendResourceUsage || cpuUsage >= ApplicationSettings.Default.BenchmarkCPUUsage || ramUsage >= ApplicationSettings.Default.BenchmarkRAMUsage)
                {
                    using (xmds.xmds xmds1 = new xmds.xmds())
                    {
                        var result = xmds1.StoreClientCpuRamUsage(ApplicationSettings.Default.HardwareKey, cpuUsage, ramTotal, ramAvailable);
                        Trace.WriteLine(new LogMessage("Client Resource Collector", "CPU Usage: " + cpuUsage + "%; RAM Available: " + ramAvailable), LogType.Info.ToString());
                    }
                }
            }
            catch (Exception err)
            {
                //resourceUsageTimer.Stop();
                Trace.WriteLine(new LogMessage("Client Resource Collector", "Error: " + err.Message), LogType.Error.ToString());
            }
        }
    }
}
