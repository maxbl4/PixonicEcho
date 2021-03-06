using System;
using System.Diagnostics;
using System.Net.Sockets;
using System.Reactive.Linq;
using System.Text;
using System.Threading;

namespace PixonicEcho
{
    class NetworkReciever : NetworkTalkerBase
    {
        private readonly NetworkServer server;
        private readonly TcpClient client;

        public NetworkReciever(NetworkServer server, TcpClient client)
        {
            this.server = server;
            this.client = client;
        }

        public void Start()
        {
            Run(client);
        }

        public IDisposable Subscription { get; set; }
        public Room Room { get; set; }

        protected override void OnDisconnected()
        {
            Console.WriteLine("[NetReciever]: {0} disconnected", RemoteId);
        }

        protected  override void OnRecieveMessage(Message msg)
        {
            RemoteId = msg.From;
            switch (msg.Type)
            {
                case MessageType.Login:
                    MyConsole.WriteLine("[Server]: rcv from {0} {1} {2}", RemoteId, msg.Type, msg.Data);
                    var room = server.GetOrAddRoom(msg.Data);
                    Room = room;
                    if (Settings.UseServerSendBuffering)
                    {
                        Subscription =
                            room.Subject.Where(x => x.From != RemoteId)
                            .Buffer(TimeSpan.FromMilliseconds(20))
                                .Subscribe(x => {
                                               Interlocked.Add(ref PerfCounters.MessagesSentFromServer, x.Count);
                                               SendBulk(x);
                                           });
                    }
                    else
                    {
                        Subscription =
                            room.Subject.Where(x => x.From != RemoteId)
                                .Subscribe(x => 
                                {
                                    Interlocked.Increment(ref PerfCounters.MessagesSentFromServer);
                                    SendMessage(x);
                                });
                    }
                    break;
                case MessageType.Echo:
                    if (!Room.Online) Dispose();
                    Interlocked.Increment(ref PerfCounters.MessagesRecievedOnServer);
                    MyConsole.WriteLine("[Server]: rcv from {0} {1} {2} {3}",
                        RemoteId, msg.Type, Room.Name, msg.Data);
                    Room.Subject.OnNext(msg);
                    break;
                default:
                    server.ClientDisconnected(this);
                    break;
            }
        }
    }
}