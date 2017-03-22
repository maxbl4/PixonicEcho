using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;

namespace PixonicEcho
{
    class NetworkServer
    {
        private TcpListener listener;
        private List<NetworkReciever> clients = new List<NetworkReciever>();

        public async void Run()
        {
            listener = new TcpListener(IPAddress.Any, Settings.NetworkPort);
            listener.Start();
            while (true)
            {
                var client = await listener.AcceptTcpClientAsync();
                var rcv = new NetworkReciever(client);
                rcv.Run();
                clients.Add(rcv);
            }
        }
    }

    class NetworkReciever
    {
        private static int rcvId;
        public int Id { get; } = rcvId++;
        private readonly TcpClient client;
        public event Action<NetworkReciever, Message> NewMessage = delegate { };
        public event Action<NetworkReciever> Disconnected = delegate { };

        public NetworkReciever(TcpClient client)
        {
            this.client = client;
        }

        public async void Run()
        {
            const int bufferSize = 1000;
            const int headerSize = 5;
            byte[] buffer = new byte[bufferSize];
            var stream = client.GetStream();
            var read = 0;
            while (true)
            {
                try
                {
                    read = await stream.ReadAsync(buffer, read, headerSize - read);
                    if (read == 0)
                    {
                        client.Close();
                        MyConsole.WriteLine("[NetReciever]: {0} disconnected", Id);
                        Disconnected(this);
                        return;
                    }
                    if (read >= headerSize)
                    {
                        var messageType = (MessageType) buffer[0];
                        var length = (buffer[1] << 8) + buffer[2];
                        var from = (buffer[3] << 8) + buffer[4];
                        while (read < length)
                        {
                            read += await stream.ReadAsync(buffer, read, length - read);
                        }
                        var data = Encoding.UTF8.GetString(buffer, headerSize, length - headerSize);
                        var msg = new Message {From = from, Type = messageType, Data = data};
                        MyConsole.WriteLine("[NetReciever]: {0} {1} {2}", msg.From, msg.Type, msg.Data);
                        NewMessage(this, msg);

                        read = 0;
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex);
                }
            }
        }
    }

    class NetworkClient : IDisposable
    {
        private static int clientId;
        public int Id { get; } = clientId++;
        private TcpClient client;
        private Timer timer;
        private string room;

        public async void Start(string room)
        {
            if (room == null) throw new ArgumentNullException(nameof(room));
            if (timer != null) throw new InvalidOperationException("Already started");
            this.room = room;
            client = new TcpClient();
            await client.ConnectAsync(IPAddress.Loopback, Settings.NetworkPort);

            timer = new Timer(OnTimer, null, Settings.ClientMessageIntervalMs, Settings.ClientMessageIntervalMs);
            SendMessage(new Message { From = Id, Type = MessageType.Login, Data = room });
        }

        private void OnTimer(object state)
        {
            SendMessage(new Message { From = Id, Type = MessageType.Echo, Data = $"From {Id}" });
        }

        async void SendMessage(Message msg)
        {
            const int bufferSize = 1000;
            const int headerSize = 5;
            var buffer = new byte[bufferSize];
            var msgLength = headerSize + Encoding.UTF8.GetBytes(msg.Data, 0, msg.Data.Length, buffer, headerSize);

            buffer[0] = (byte)msg.Type;
            buffer[1] = (byte)(msgLength >> 8);
            buffer[2] = (byte)(msgLength);
            buffer[3] = (byte)(Id >> 8);
            buffer[4] = (byte)(Id);

            var stream = client.GetStream();
            await stream.WriteAsync(buffer, 0, msgLength);
        }

        public void Dispose()
        {
            client.Close();
            timer.Dispose();
        }
    }
}