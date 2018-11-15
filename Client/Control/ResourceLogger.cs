using Microsoft.VisualBasic.Devices;
using System;
using System.Diagnostics;
using System.Threading;

namespace XiboClient.Control
{
    public static class ResourceLogger
    {
        private static ComputerInfo ramCounter;
        private static ulong totalRam;
        private static PerformanceCounter cpuCounter;
        private static Timer timer;

        public static void Start()
        {
            ramCounter = new ComputerInfo();
            totalRam = ramCounter.TotalPhysicalMemory;

            cpuCounter = new PerformanceCounter("Processor Information", "% Processor Time", "_Total");

            cpuCounter.NextValue();

            timer = new Timer(obj => Run(), null, 1000, 6000);
        }

        public static void Run()
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
    }
}