namespace XiboClient.Control
{
    using Microsoft.VisualBasic.Devices;
    using System;
    using System.Diagnostics;
    using System.Timers;

    class ResourceUsageManager
    {
        private static Timer resourceUsageTimer;
        // private static PerformanceCounter performanceCounter;      

        public static void Start()
        {
            resourceUsageTimer = new Timer(ApplicationSettings.Default.ResourceUsageInterval);
            resourceUsageTimer.Elapsed += CollectClientResourceUsage;
            resourceUsageTimer.Start();

            CollectClientResourceUsage(null, null);
        }

        private static int CPUUsage()
        {
            var performanceCounter = new PerformanceCounter()
            {
                CategoryName = "Processor",
                CounterName = "% Processor Time",
                InstanceName = "_Total"
            };

            if (performanceCounter != null)
            {
                var nextValue = performanceCounter.NextValue();
                System.Threading.Thread.Sleep(500);
                nextValue = performanceCounter.NextValue();

                return Convert.ToInt32(nextValue);
            }

            return 0;
        }

        private static int MemoryUsage(int memoryTotal)
        {
            var performanceCounter = new PerformanceCounter()
            {
                CategoryName = "Memory",
                CounterName = "Available MBytes"
            };

            if (performanceCounter != null)
            {
                var nextValue = performanceCounter.NextValue();

                return Convert.ToInt32(nextValue);

                
            }

            return 0;
        }

        private static void CollectClientResourceUsage(object sender, System.Timers.ElapsedEventArgs e)
        {
            var computerInfo = new ComputerInfo();
            var totalPhysicalMemory = computerInfo.TotalPhysicalMemory;
            var memoryTotal = Convert.ToInt32(totalPhysicalMemory / 1024 / 1024);

            var cpuUsage = CPUUsage();
            var memoryUsage = MemoryUsage(memoryTotal);
            var memoryAvailable = Convert.ToInt32(Convert.ToDecimal(memoryTotal - memoryUsage) / memoryTotal * 100);

            if (ApplicationSettings.Default.IsForcedSendResourceUsage || cpuUsage >= ApplicationSettings.Default.BenchmarkCPUUsage || memoryAvailable >= ApplicationSettings.Default.BenchmarkRAMUsage)
            {

                using (xmds.xmds xmds1 = new xmds.xmds())
                {
                    xmds1.StoreClientCpuRamUsageAsync(ApplicationSettings.Default.HardwareKey, cpuUsage, memoryTotal, memoryUsage);
                }

            }
        }
    }
}
