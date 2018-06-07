using NetMQ;
using NetMQ.Sockets;
using Newtonsoft.Json;
using SMS.Testing.Console.Action;
using SMS.Testing.Console.Logic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMS.Testing.Console.Modules
{
    public class ConnectXMR
    {
        private HardwareKey _hardwareKey;

        public void SubscribeQueue()
        {
            // Create a Poller
            var _poller = new NetMQPoller();
            _hardwareKey = new HardwareKey();
            try
            {
                // Create a Socket
                using (SubscriberSocket socket = new SubscriberSocket())
                {
                    // Options
                    socket.Options.ReconnectInterval = TimeSpan.FromSeconds(5);
                    socket.Options.Linger = TimeSpan.FromSeconds(0);

                    // Bind
                    socket.Connect(AppSetting.ServerUri);
                    socket.Subscribe("H");
                    socket.Subscribe(_hardwareKey.Channel);

                    // Add Socket to Poller
                    _poller.Add(socket);

                    // Bind to the receive ready event
                    socket.ReceiveReady += _socket_ReceiveReady;

                    // Sit and wait, processing messages, indefinitely or until we are interrupted.
                    _poller.Run();
                }
            }
            finally
            {
                _poller.Dispose();
            }
        }

        private void _socket_ReceiveReady(object sender, NetMQSocketEventArgs e)
        {
            var messages = e.Socket.ReceiveMultipartMessage();
            var rsaKey = _hardwareKey.getXmrKey();
            if (messages.Any())
            {
                if (messages[0].ConvertToString() == "H")
                {
                    System.Console.WriteLine("Last heart beat at " + DateTime.Now);
                }

                if (!string.IsNullOrWhiteSpace(messages[2].ConvertToString()) && !string.IsNullOrWhiteSpace(messages[1].ConvertToString()))
                {
                    var opened = OpenSslInterop.decrypt(messages[2].ConvertToString(), messages[1].ConvertToString(), rsaKey.Private);

                    PlayerAction action = JsonConvert.DeserializeObject<PlayerAction>(opened);
                    Dictionary<string, string> obj = JsonConvert.DeserializeObject<Dictionary<string, string>>(opened);
                    string code;
                    obj.TryGetValue("commandCode", out code);

                    System.Console.WriteLine("Command action is " + action.action);
                    System.Console.WriteLine("Command code is " + code);
                }
            }
            else
            {
                System.Console.WriteLine("There is no any messages");
            }
        }
    }
}
