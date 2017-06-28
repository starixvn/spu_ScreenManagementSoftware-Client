using NetMQ;
using NetMQ.Sockets;
using Newtonsoft.Json;
using Org.BouncyCastle.Crypto;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using XiboClient.Action;
using XiboClient.Log;
using XiboClient.XmdsAgents;

namespace XiboClient.Logic
{
    class XmrSubscriber
    {
        private const string UPGRADEVERSION_FOLDER = @"C:\XiboClient_NewVersion";
        private const string UPGRADEVERSION_FILENAME = "XiboClient_NewVersion.zip";
        private const string UPGRADEVERSION_INSTALLATIONFILENAME = "setup.exe";

        public static object _locker = new object();

        // Members to stop the thread
        private bool _forceStop = false;

        /// <summary>
        /// Last Heartbeat packet received
        /// </summary>
        public DateTime LastHeartBeat = DateTime.MinValue;

        // Events
        public delegate void OnActionDelegate(PlayerActionInterface action);
        public event OnActionDelegate OnAction;

        /// <summary>
        /// Client Hardware key
        /// </summary>
        public HardwareKey HardwareKey
        {
            set
            {
                _hardwareKey = value;
            }
        }
        private HardwareKey _hardwareKey;

        /// <summary>
        /// Client Info Form
        /// </summary>
        public ClientInfo ClientInfoForm
        {
            set
            {
                _clientInfoForm = value;
            }
        }
        private ClientInfo _clientInfoForm;

        /// <summary>
        /// Runs the agent
        /// </summary>
        public void Run()
        {
            try
            {
                // Check we have an address to connect to.
                if (string.IsNullOrEmpty(ApplicationSettings.Default.XmrNetworkAddress))
                    throw new Exception("Empty XMR Network Address");

                // Get the Private Key
                AsymmetricCipherKeyPair rsaKey = _hardwareKey.getXmrKey();

                // Connect to XMR
                using (SubscriberSocket socket = new SubscriberSocket())
                {
                    // Bind
                    socket.Connect(ApplicationSettings.Default.XmrNetworkAddress);
                    socket.Subscribe("H");
                    socket.Subscribe(_hardwareKey.Channel);

                    // Notify
                    _clientInfoForm.XmrSubscriberStatus = "Connected to " + ApplicationSettings.Default.XmrNetworkAddress;

                    while (!_forceStop)
                    {
                        lock (_locker)
                        {
                            try
                            {
                                NetMQMessage message = socket.ReceiveMultipartMessage();

                                // Update status
                                string statusMessage = "Connected (" + ApplicationSettings.Default.XmrNetworkAddress + "), last activity: " + DateTime.Now.ToString();

                                // Write this out to a log
                                _clientInfoForm.XmrSubscriberStatus = statusMessage;
                                Trace.WriteLine(new LogMessage("XmrSubscriber - Run", statusMessage), LogType.Audit.ToString());

                                // Deal with heart beat
                                if (message[0].ConvertToString() == "H")
                                {
                                    LastHeartBeat = DateTime.Now;
                                    continue;
                                }

                                // Decrypt the message
                                string opened = OpenSslInterop.decrypt(message[2].ConvertToString(), message[1].ConvertToString(), rsaKey.Private);

                                // Decode into a JSON string
                                PlayerAction action = JsonConvert.DeserializeObject<PlayerAction>(opened);

                                // Make sure the TTL hasn't expired
                                if (DateTime.Now > action.createdDt.AddSeconds(action.ttl))
                                {
                                    Trace.WriteLine(new LogMessage("XmrSubscriber - Run", "Expired Message: " + action.action), LogType.Info.ToString());
                                    continue;
                                }

                                // Decide what to do with the message, probably raise events according to the type of message we have
                                switch (action.action)
                                {
                                    case "commandAction":

                                        // Create a schedule command out of the message
                                        Dictionary<string, string> obj = JsonConvert.DeserializeObject<Dictionary<string, string>>(opened);
                                        ScheduleCommand command = new ScheduleCommand();
                                        string code;
                                        obj.TryGetValue("commandCode", out code);
                                        command.Code = code;

                                        switch (code)
                                        {
                                            case "UpgradeVersion":
                                                WriteLog("UPGRADE VERSION ........");
                                                UpgradeNewVersion();
                                                break;
                                            default:
                                                new Thread(new ThreadStart(command.Run)).Start();
                                                break;
                                        }
                                        break;

                                    case "collectNow":
                                    case RevertToSchedulePlayerAction.Name:
                                        if (OnAction != null)
                                            OnAction(action);
                                        break;

                                    case LayoutChangePlayerAction.Name:

                                        LayoutChangePlayerAction changeLayout = JsonConvert.DeserializeObject<LayoutChangePlayerAction>(opened);

                                        if (OnAction != null)
                                            OnAction(changeLayout);

                                        break;

                                    case OverlayLayoutPlayerAction.Name:
                                        OverlayLayoutPlayerAction overlayLayout = JsonConvert.DeserializeObject<OverlayLayoutPlayerAction>(opened);

                                        if (OnAction != null)
                                            OnAction(overlayLayout);
                                        break;

                                    case "screenShot":
                                        ScreenShot.TakeAndSend();
                                        _clientInfoForm.notifyStatusToXmds();
                                        break;

                                    default:
                                        Trace.WriteLine(new LogMessage("XmrSubscriber - Run", "Unknown Message: " + action.action), LogType.Info.ToString());
                                        break;
                                }
                            }
                            catch (Exception ex)
                            {
                                // Log this message, but dont abort the thread
                                Trace.WriteLine(new LogMessage("XmrSubscriber - Run", "Exception in Run: " + ex.Message), LogType.Error.ToString());
                                _clientInfoForm.XmrSubscriberStatus = "Error. " + ex.Message;
                            }
                        }
                    }
                }

                // Update status
                _clientInfoForm.XmrSubscriberStatus = "Not Running, last activity: " + LastHeartBeat.ToString();

                Trace.WriteLine(new LogMessage("XmrSubscriber - Run", "Subscriber Stopped"), LogType.Info.ToString());
            }
            catch (Exception e)
            {
                Trace.WriteLine(new LogMessage("XmrSubscriber - Run", "Unable to Subscribe to XMR: " + e.Message), LogType.Info.ToString());
                _clientInfoForm.XmrSubscriberStatus = e.Message;
            }
        }

        private void UpgradeNewVersion()
        {
            string myStringWebResource = null;

            var myWebClient = new WebClient();
            myStringWebResource = ApplicationSettings.Default.ServerUri + "/UpgradeVersion/" + UPGRADEVERSION_FILENAME;

            try
            {
                myWebClient.DownloadFileCompleted += new AsyncCompletedEventHandler(DownloadFileCompleted);
                myWebClient.DownloadProgressChanged += new DownloadProgressChangedEventHandler(DownloadProgressCallback);
                myWebClient.DownloadFileAsync(new Uri(myStringWebResource), @"C:\" + UPGRADEVERSION_FILENAME);
                WriteLog("Downloading upgdrade files ........");
            }
            catch (Exception e)
            {
                WriteLog("Download upgdrade files failed. Error: " + e.ToString());
            }
        }

        private void DownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
        {
            try
            {
                System.IO.Compression.ZipFile.ExtractToDirectory(@"C:\" + UPGRADEVERSION_FILENAME, UPGRADEVERSION_FOLDER);
                WriteLog("Download and unzipped compeleted ........");
                InstallNewVersion();
            }
            catch (Exception ae)
            {

                WriteLog("Extract upgdrade files failed. Error: " + ae.ToString());
            }
        }

        private void DownloadProgressCallback(object sender, DownloadProgressChangedEventArgs e)
        {
            try
            {
                string content = string.Format("{0}    downloaded {1} of {2} bytes. {3} % complete...",
                    (string)e.UserState,
                    e.BytesReceived,
                    e.TotalBytesToReceive,
                    e.ProgressPercentage);
            }
            catch (Exception ae)
            {
                WriteLog("Progress upgdrade files failed. Error: " + ae.ToString());
            }
        }

        private void InstallNewVersion()
        {
            Process[] processes = Process.GetProcessesByName("Xibo");
            foreach(var process in processes)
            {
                process.Close();
            }

            Thread.Sleep(3000);

            Process.Start(UPGRADEVERSION_FOLDER + @"\" + UPGRADEVERSION_INSTALLATIONFILENAME);
        }

        private void WriteLog(string content)
        {
            using (var sw = new System.IO.StreamWriter(File.Open(@"C:\XiboLog.txt", FileMode.Append), Encoding.GetEncoding("iso-8859-1")))
            {
                sw.WriteLine(DateTime.Now.ToString() + ": " + content);
                sw.Close();
            }
        }
    }
}
