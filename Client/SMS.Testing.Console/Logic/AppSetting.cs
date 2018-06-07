using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMS.Testing.Console.Logic
{
    public static class AppSetting
    {
        public static string ServerUri {
            get {
                return ConfigurationManager.AppSettings["server.url"].ToString();
            }
        }

        public static string ServerKey
        {
            get {
                return ConfigurationManager.AppSettings["server.key"].ToString();
            }
        }

        public static string XmrAddress
        {
            get
            {
                return ConfigurationManager.AppSettings["xmr.address"].ToString();
            }
        }


        public static string LibraryPath {
            get {
                return System.Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\SMS Client Library";
            }
        }
    }
}
