using NetMQ;
using NetMQ.Sockets;
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
        public void SubscribeQueue()
        {
            // Create a Poller
            var _poller = new NetMQPoller();
            var _hardwareKey = new HardwareKey();
            try
            {
                // Create a Socket
                using (SubscriberSocket socket = new SubscriberSocket())
                {
                    // Options
                    socket.Options.ReconnectInterval = TimeSpan.FromSeconds(5);
                    socket.Options.Linger = TimeSpan.FromSeconds(0);

                    // Bind
                    socket.Connect(System.Configuration.ConfigurationManager.AppSettings["xmr.url"]);
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
        }
    }
}
