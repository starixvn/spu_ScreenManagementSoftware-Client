using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Management;
using SMS.Testing.Console.Utils;

namespace SMS.Testing.Console.Modules
{
    public class SmsEnvironment
    {
        public void Check()
        {
            var osName = GetOSFriendlyName();
            var verNetFramework = Helpers.DetectNetFramework();
            var freeSpaceInBytes = DriveInfo.GetDrives().Where(x => x.Name == "C:\\").FirstOrDefault().AvailableFreeSpace;
            var freeSpace = Helpers.CaculateBytes(freeSpaceInBytes);

            System.Console.WriteLine("");
            System.Console.WriteLine("\t Operation system: \t\t" + osName);
            System.Console.WriteLine("\t Framework version: \t\t" + verNetFramework);
            System.Console.WriteLine("\t Available space in C disk: \t" + freeSpace);
            System.Console.WriteLine("");
        }

        private string GetOSFriendlyName()
        {
            string result = string.Empty;
            ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT Caption FROM Win32_OperatingSystem");
            foreach (ManagementObject os in searcher.Get())
            {
                result = os["Caption"].ToString();
                break;
            }
            return result;
        }
    }
}
