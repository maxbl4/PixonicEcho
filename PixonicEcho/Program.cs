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
            NetworkServer netServer;
            NetworkClient netClient;
            switch (args[0].ToLowerInvariant())
            {
                case "memory":
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
                case "network":
                    netServer = new NetworkServer();
                    netServer.Run();
                    var nc1 = new NetworkClient();
                    nc1.Start("Room 1");
                    Thread.Sleep(200);
                    var nc2 = new NetworkClient();
                    nc2.Start("Room 1");
                    Console.ReadLine();
                    nc1.Dispose();
                    Console.ReadLine();
                    nc2.Dispose();
                    Console.ReadLine();
                    break;
                case "server":
                    Console.WriteLine("This is SERVER");
                    netServer = new NetworkServer();
                    netServer.Run();
                    Console.ReadLine();
                    break;
                case "client":
                    if (args.Length < 2)
                    {
                        Console.WriteLine("Specify room name: client <room>");
                        return;
                    }
                    netClient = new NetworkClient();
                    netClient.Start(args[1]);
                    Console.ReadLine();
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
