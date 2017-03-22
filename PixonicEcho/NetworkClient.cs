using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace PixonicEcho
{
    class NetworkClient : NetworkTalkerBase
    {
        private Timer timer;
        private string room;

        public async void Start(string room)
        {
            if (room == null) throw new ArgumentNullException(nameof(room));
            if (timer != null) throw new InvalidOperationException("Already started");
            this.room = room;
            var c = new TcpClient();
            await c.ConnectAsync(IPAddress.Loopback, Settings.NetworkPort);
            Id = ((IPEndPoint) c.Client.LocalEndPoint).Port;
            Console.WriteLine("Started CLIENT {0}", Id);
            Run(c);

            timer = new Timer(OnTimer, null, Settings.ClientMessageIntervalMs, Settings.ClientMessageIntervalMs);
            SendMessage(new Message { From = Id, Type = MessageType.Login, Data = room });
        }

        private void OnTimer(object state)
        {
            Interlocked.Increment(ref PerfCounters.MessagesSentFromClient);
            SendMessage(new Message { From = Id, Type = MessageType.Echo, Data = $"From {Id}" });
        }

        protected override void OnRecieveMessage(Message msg)
        {
            Interlocked.Increment(ref PerfCounters.MessagesRecievedOnClient);
            MyConsole.WriteLine("[Client {0}] rcv: {1}", Id, msg.Data);
        }

        protected override void OnDisconnected()
        {
            Dispose();
            Console.WriteLine("[Client {0}] Server dropped connection", Id);
        }

        public override void Dispose()
        {
            timer.Dispose();
            base.Dispose();
        }
    }
}