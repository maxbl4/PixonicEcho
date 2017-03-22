using System;
using System.Diagnostics;
using System.Net.Sockets;
using System.Text;

namespace PixonicEcho
{
    abstract class NetworkTalkerBase : IDisposable
    {
        private TcpClient client;
        private NetworkStream stream;
        #region Equality
        protected bool Equals(NetworkReciever other)
        {
            return Id == other.Id;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((NetworkReciever)obj);
        }

        public override int GetHashCode()
        {
            return Id;
        }
        #endregion

        private static int rcvId;
        public int Id { get; protected set; } = rcvId++;

        private int? remoteId;
        public int RemoteId
        {
            get { return remoteId ?? -1; }
            protected set
            {
                if (!remoteId.HasValue)
                    remoteId = value;
                else if (remoteId.Value != value)
                    throw new InvalidOperationException("Remoted Id already set");

            }
        }
        
        protected virtual void Run(TcpClient client)
        {
            this.client = client;
            stream = client.GetStream();
            RecieveAsync();
        }

        protected abstract void OnDisconnected();
        protected abstract void OnRecieveMessage(Message msg);

        async void RecieveAsync()
        {
            const int bufferSize = 1000;
            const int headerSize = 5;
            byte[] buffer = new byte[bufferSize];
            var read = 0;
            while (true)
            {
                try
                {
                    if (!stream.CanRead)
                    {
                        Disconnect();
                        return;
                    }
                    read = await stream.ReadAsync(buffer, read, headerSize - read);
                    if (read == 0)
                    {
                        Disconnect();
                        return;
                    }
                    if (read >= headerSize)
                    {
                        var messageType = (MessageType)buffer[0];
                        var length = (buffer[1] << 8) + buffer[2];
                        var from = (buffer[3] << 8) + buffer[4];
                        while (read < length)
                        {
                            read += await stream.ReadAsync(buffer, read, length - read);
                        }
                        var data = Encoding.UTF8.GetString(buffer, headerSize, length - headerSize);
                        var msg = new Message { From = from, Type = messageType, Data = data };
                        OnRecieveMessage(msg);

                        read = 0;
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex);
                    Disconnect();
                    return;
                }
            }
        }

        private void Disconnect()
        {
            stream.Close();
            OnDisconnected();
        }

        protected async void SendMessage(Message msg)
        {
            const int bufferSize = 1000;
            const int headerSize = 5;
            var buffer = new byte[bufferSize];
            var msgLength = headerSize + Encoding.UTF8.GetBytes(msg.Data, 0, msg.Data.Length, buffer, headerSize);

            buffer[0] = (byte)msg.Type;
            buffer[1] = (byte)(msgLength >> 8);
            buffer[2] = (byte)(msgLength);
            buffer[3] = (byte)(msg.From >> 8);
            buffer[4] = (byte)(msg.From);
            try
            {
                await stream.WriteAsync(buffer, 0, msgLength);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
        }

        public virtual void Dispose()
        {
            stream.Dispose();
            client.Close();
        }
    }
}