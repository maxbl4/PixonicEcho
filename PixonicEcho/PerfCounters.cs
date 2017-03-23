using System;
using System.Threading;

namespace PixonicEcho
{
    class PerfCounters
    {
        public static long MessagesRecievedOnServer;
        public static long MessagesSentFromClient;
        public static long MessagesSentFromServer;
        public static long MessagesRecievedOnClient;
        private static long lastMessages;
        private static long lastServerMessages;
        private static Timer timer;

        public static void Monitor()
        {
            timer = new Timer(CheckCounters, null, 1000, 1000);
        }

        private static void CheckCounters(object state)
        {
            var msgServer = MessagesRecievedOnServer + MessagesSentFromServer;
            var deltaServer = msgServer - lastServerMessages;
            lastServerMessages = msgServer;
            var msgClient = MessagesRecievedOnClient + MessagesSentFromClient;
            var deltaClient = msgClient - lastMessages;
            lastMessages = msgClient;
            Console.WriteLine("Throughput: Client {0}/s ({1}), Server {2}/s ({3})", deltaClient, msgClient, deltaServer, msgServer);
        }
    }
}