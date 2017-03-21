using System;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Threading;

namespace PixonicEcho
{
    class ClientRecord
    {
        private readonly MemoryEchoServer server;
        object sync = new object();

        #region Equality
        protected bool Equals(ClientRecord other)
        {
            return Equals(Client, other.Client);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((ClientRecord)obj);
        }

        public override int GetHashCode()
        {
            return (Client != null ? Client.GetHashCode() : 0);
        } 
        #endregion

        public ClientRecord(MemoryEchoServer server, MemoryClient client)
        {
            this.server = server;
            Client = client;
            client.NewMessage += HandleMessageFromClient;
            client.Disconnected += HandleDisconnected;
        }
        
        public MemoryClient Client { get; }
        public IDisposable Subscription { get; set; }
        public Room Room { get; set; }

        private void HandleMessageFromClient(MemoryClient client, Message msg)
        {
            lock (sync)
            {
                switch (msg.Type)
                {
                    case MessageType.Login:
                        MyConsole.WriteLine("[Server]: rcv from {0} {1} {2}", client.Id, msg.Type, msg.Data);
                        var room = server.GetOrAddRoom(msg.Data);
                        Room = room;
                        Subscription =
                            room.Subject.Where(x => x.From != client.Id)
                                .Subscribe(client.AcceptMessage);
                        break;
                    case MessageType.Echo:
                        Interlocked.Increment(ref PerfCounters.MessagesRecievedOnServer);
                        MyConsole.WriteLine("[Server]: rcv from {0} {1} {2} {3}", 
                            client.Id, msg.Type, Room.Name, msg.Data);
                        Room.Subject.OnNext(msg);
                        break;
                    default:
                        server.ClientDisconnected(client);
                        break;
                }
            }
        }

        private void HandleDisconnected(MemoryClient obj)
        {
            Subscription.Dispose();
            Client.NewMessage -= HandleMessageFromClient;
            server.ClientDisconnected(Client);
        }
    }
}