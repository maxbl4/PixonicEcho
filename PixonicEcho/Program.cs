using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PixonicEcho
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length < 1)
            {
                Console.WriteLine("Specificy client or server as argument");
                return;
            }
            switch (args[0].ToLowerInvariant())
            {
                case "server":
                    var server = new MemoryEchoServer();
                    var clients = new List<MemoryClient>();
                    for (int i = 0; i < Settings.Rooms; i++)
                    {
                        for (int j = 0; j < Settings.ClientsPerRoom; j++)
                        {
                            clients.Add(AddClient(server, $"Room {i}"));
                        }
                    }
                    Console.WriteLine($"Added {Settings.ClientsPerRoom * Settings.Rooms} clients");
                    PerfCounters.Monitor();
                    Console.ReadLine();
                    while (clients.Count > 0)
                    {
                        Thread.Sleep(Settings.ClientLifeMs);
                        clients[0].Dispose();
                        clients.RemoveAt(0);
                    }
                    Console.ReadLine();
                    GC.KeepAlive(server);
                    break;
                case "client":
                    break;
            }
        }

        static IEnumerable<MemoryClient> AddManyClients(MemoryEchoServer server, string room, int count)
        {
            for (int i = 0; i < count; i++)
            {
                yield return AddClient(server, room);
            }
        }

        static MemoryClient AddClient(MemoryEchoServer server, string room)
        {
            MemoryClient c;
            server.AcceptClient(c = new MemoryClient());
            c.Start(room);
            return c;
        }
    }
}
