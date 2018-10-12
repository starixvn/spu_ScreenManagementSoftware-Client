using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using XiboClient.Control;

namespace XiboClientWatchdog
{
    static class Program
    {
        static Mutex mutex;

        private static ResourceLogger resourceLogger;

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            string mutexName = Path.GetFileNameWithoutExtension(Application.ExecutablePath);

            mutex = new Mutex(true, mutexName);

            if (mutex.WaitOne(TimeSpan.Zero, true))
            {
                var tray = new Tray();

                resourceLogger = new ResourceLogger(tray.LogException);

                Application.EnableVisualStyles();
                //Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(tray);

                // Release when we've stopped
                mutex.ReleaseMutex();
            }
        }
    }
}
