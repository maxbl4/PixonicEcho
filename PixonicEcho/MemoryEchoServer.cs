using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Subjects;
using System.Threading;

namespace PixonicEcho
{
    class MemoryEchoServer
    {
        private Dictionary<MemoryClient,ClientRecord> clients = new Dictionary<MemoryClient, ClientRecord>();
        private Dictionary<string, Room> rooms = new Dictionary<string, Room>();
        private Timer roomDisposer;
        
        public MemoryEchoServer()
        {
            roomDisposer = new Timer(RoomDisposer, null, Settings.RoomLifeTimeoutMs, Settings.RoomLifeTimeoutMs);
        }

        public void AcceptClient(MemoryClient client)
        {
            var clientRecord = new ClientRecord(this, client);
            client.Disconnected += ClientDisconnected;
            MyConsole.WriteLine($"{client.Id} connected");
            if (!client.IsConnected)
            {
                ClientDisconnected(client);
            }
            else
            {
                clients.AddSync(client, clientRecord);
            }
        }

        public void ClientDisconnected(MemoryClient client)
        {
            ClientRecord record;
            if (!clients.TryRemoveSync(client, out record))
                return;
            
            MyConsole.WriteLine($"{client.Id} disconnected");
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
                        MyConsole.WriteLine($"[Server]: Removed room {staticRoom.Name} due to inactivity");
                    }
                }
            }
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
    }
}