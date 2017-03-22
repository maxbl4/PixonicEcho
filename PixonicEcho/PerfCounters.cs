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
        private static Timer timer;

        public static void Monitor()
        {
            timer = new Timer(CheckCounters, null, 1000, 1000);
        }

        private static void CheckCounters(object state)
        {
            var curMsg = MessagesRecievedOnServer;
            var delta = curMsg - lastMessages;
            lastMessages = curMsg;
            Console.WriteLine($"Sent from client {MessagesSentFromClient}, Recieved on server {MessagesRecievedOnServer}, Recieved on client {MessagesRecievedOnClient}");
            Console.WriteLine($"{delta}/s {curMsg}");
        }
    }
}