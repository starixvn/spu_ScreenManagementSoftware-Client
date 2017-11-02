using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace XiboClientWatchdog
{
#if debug
    static class Debug
    {
        public static string ChangeProcessPath
        {
            get
            {
                var solutionDir = Directory.GetParent(Environment.CurrentDirectory).Parent.Parent.FullName;

                var path = string.Format("{0}/client/bin/debug/smsclient.exe", solutionDir);

                return path;
            }
        }
    } 
#endif
}
