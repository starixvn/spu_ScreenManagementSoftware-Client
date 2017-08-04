namespace XiboClient.Control
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Windows.Forms;

    class ResourceUsageManager
    {
        private const int RESOURCEUSAGE_INTERVAL = 10000;
        private static Timer resourceUsageTimer;

        public static void Start()
        {
            resourceUsageTimer = new Timer();
            resourceUsageTimer.Interval = RESOURCEUSAGE_INTERVAL;
            resourceUsageTimer.Tick += new EventHandler(CollectClientResourceUsage);
            resourceUsageTimer.Start();            
        }

        private static void CollectClientResourceUsage(object sender, EventArgs e)
        {
            try
            {
                PerformanceCounter cpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");
                PerformanceCounter ramCounter = new PerformanceCounter("Memory", "Available MBytes");
                int cpuUsage = int.Parse(cpuCounter.NextValue().ToString());
                System.Threading.Thread.Sleep(500);
                cpuUsage = int.Parse(cpuCounter.NextValue().ToString());
                int ramAvailable = int.Parse(ramCounter.NextValue().ToString());

                using(xmds.xmds xmds1 = new xmds.xmds())
                {
                    xmds1.StoreClientCpuRamUsage(ApplicationSettings.Default.HardwareKey, 1, 1, 1);
                }
            }
            catch
            {
                resourceUsageTimer.Stop();
            }
        }
    }
}
