using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;

namespace Logger
{
    public static class ExceptionSerializer
    {
        public static string RootDir { get; set; }

        public static void Serialize(Exception ex, Action finishCallback)
        {
            ThreadPool.QueueUserWorkItem(e =>
            {
                SerializeException((Exception)e);
                finishCallback();
            }, ex);
        }

        public static Exception Deserialize(string path)
        {
            using (var file = File.Open(path, FileMode.Open))
            {
                var formatter = new BinaryFormatter();

                var obj = formatter.Deserialize(file);

                return (Exception)obj;
            }
        }

        private static void SerializeException(Exception ex)
        {
            using (var stream = CreateStream())
            {
                var binary = new BinaryFormatter();

                binary.Serialize(stream, ex);
            }
        }

        private static Stream CreateStream()
        {
            var clientName = Environment.MachineName;

            var directoryPath = string.Format("{0}/{1}", RootDir, clientName);

            if (!Directory.Exists(directoryPath)) Directory.CreateDirectory(directoryPath);

            var fileName = string.Format("{0}_{1}.bin", DateTime.Now.ToString("dd-MMMM-yyyy H-mm-ss"), Guid.NewGuid());

            return File.Create(string.Format("{0}/{1}", directoryPath, fileName));
        }
    }
}