using Microsoft.VisualBasic.Devices;
using System;
using System.Diagnostics;
using System.Threading;

namespace XiboClient.Control
{
    public class ResourceLogger
    {
        private static ComputerInfo ramCounter;
        private static ulong totalRam;
        private static PerformanceCounter cpuCounter;
        private static Timer timer;
        private readonly Action<Exception> logger;

        public ResourceLogger(Action<Exception> logger)
        {
            ramCounter = new ComputerInfo();
            totalRam = ramCounter.TotalPhysicalMemory;

            cpuCounter = new PerformanceCounter("Processor Information", "% Processor Time", "_Total");

            cpuCounter.NextValue();

            this.logger = logger;

            timer = new Timer(obj => Run(), null, 1000, 3000);
        }

        public void Run()
        {
            try
            {
                var total = totalRam / 1024 / 1024;
                var raw = ramCounter.AvailablePhysicalMemory;
                var free = raw / 1024 / 1024;
                var cpu = cpuCounter.NextValue();

                using (xmds.xmds xmds1 = new xmds.xmds())
                {
                    xmds1.StoreClientCpuRamUsageAsync(ApplicationSettings.Default.HardwareKey, Convert.ToInt32(cpu), Convert.ToInt32(total), Convert.ToInt32(free));
                }
            }
            catch (Exception ex)
            {
                logger(ex);
            }
        }
    }
}