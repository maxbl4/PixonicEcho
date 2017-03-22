using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reactive.Subjects;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;

namespace PixonicEcho
{
    class NetworkServer
    {
        private HashSet<NetworkReciever> clients = new HashSet<NetworkReciever>();
        private Dictionary<string, Room> rooms = new Dictionary<string, Room>();
        private Timer roomDisposer;
        private TcpListener listener;

        public NetworkServer()
        {
            roomDisposer = new Timer(RoomDisposer, null, Settings.RoomLifeTimeoutMs, Settings.RoomLifeTimeoutMs);
        }

        public async void Run()
        {
            listener = new TcpListener(IPAddress.Any, Settings.NetworkPort);
            listener.Start();
            while (true)
            {
                var client = await listener.AcceptTcpClientAsync();
                AcceptClient(client);
            }
        }

        void AcceptClient(TcpClient client)
        {
            var rcv = new NetworkReciever(this, client);
            lock (clients)
                clients.Add(rcv);
            rcv.Start();
            Console.WriteLine($"accept connection");
        }

        public void ClientDisconnected(NetworkReciever client)
        {
            lock (clients)
            if (!clients.Remove(client))
                return;

            Console.WriteLine($"{client.RemoteId} disconnected");
        }

        public Room GetOrAddRoom(string name)
        {
            return rooms.GetOrAddSync(name, key =>
            {
                var r = new Room(key, new Subject<Message>());
                MyConsole.WriteLine($"[Server]: created room {key}");
                return r;
            });
        }

        void RoomDisposer(object o)
        {
            List<Room> staticRooms;
            lock (rooms)
            {
                staticRooms = rooms.Values.ToList();
            }
            foreach (var staticRoom in staticRooms)
            {
                if ((DateTime.Now - staticRoom.LastMessage).TotalMilliseconds > Settings.RoomLifeTimeoutMs)
                {
                    Room r;
                    if (rooms.RemoveWhereSync(staticRoom.Name,
                        (k, v) => (DateTime.Now - v.LastMessage).TotalMilliseconds > Settings.RoomLifeTimeoutMs, out r))
                    {
                        r.Online = false;
                        Console.WriteLine($"[Server]: Removed room {staticRoom.Name} due to inactivity");
                    }
                }
            }
        }
    }
}