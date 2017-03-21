using System;
using System.Threading;

namespace PixonicEcho
{
    class MemoryClient : IDisposable
    {
        private static int clientId;
        public int Id { get; }  = clientId++;
        private Timer timer;
        public event Action<MemoryClient, Message> NewMessage = delegate{  };
        public event Action<MemoryClient> Disconnected = delegate{  };
        public bool IsConnected { get; private set; } = true;

        public string Room { get; private set; }

        public void Start(string room)
        {
            if (room == null) throw new ArgumentNullException(nameof(room));
            if (timer != null) throw new InvalidOperationException("Already started");
            Room = room;
            timer = new Timer(OnTimer, null, Settings.ClientMessageIntervalMs, Settings.ClientMessageIntervalMs);
            NewMessage(this, new Message { From = Id, Type = MessageType.Login, Data = room });
        }

        public void AcceptMessage(Message msg)
        {
            Interlocked.Increment(ref PerfCounters.MessagesRecievedOnClient);
            MyConsole.WriteLine("[{0}] rcv: {1}", Id, msg.Data);
        }

        void OnTimer(object o)
        {
            Interlocked.Increment(ref PerfCounters.MessagesSentFromClient);
            NewMessage(this, new Message {From = Id, Type = MessageType.Echo, Data = $"From {Id}"});
        }

        public void Dispose()
        {
            timer.Dispose();
            IsConnected = false;
            Disconnected(this);
        }
    }
}