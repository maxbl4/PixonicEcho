using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Sockets;
using System.Text;

namespace PixonicEcho
{
    abstract class NetworkTalkerBase : IDisposable
    {
        const int headerSize = 5;
        private const int bufferSize = 8000;
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

        bool SerializeMessage(Message msg, byte[] buffer, ref int offset)
        {
            var msgLength = headerSize + Encoding.UTF8.GetByteCount(msg.Data);
            if (msgLength > buffer.Length - offset) return false;
            Encoding.UTF8.GetBytes(msg.Data, 0, msg.Data.Length, buffer, offset + headerSize);
            buffer[offset + 0] = (byte)msg.Type;
            buffer[offset + 1] = (byte)(msgLength >> 8);
            buffer[offset + 2] = (byte)(msgLength);
            buffer[offset + 3] = (byte)(msg.From >> 8);
            buffer[offset + 4] = (byte)(msg.From);

            offset += msgLength;
            return true;
        }

        protected async void SendBulk(IList<Message> messages)
        {
            if (messages.Count == 0) return;
            var buffer = new byte[bufferSize];
            var totalSent = 0;
            do
            {
                var offset = 0;
                var serializedCount = messages.Skip(totalSent)
                    .TakeWhile(x => SerializeMessage(x, buffer, ref offset))
                    .Count();
                totalSent += serializedCount;
                if (serializedCount == 0)
                    throw new ArgumentOutOfRangeException("Message data too big. It should not exceed 3997 chars.");
                try
                {
                    await stream.WriteAsync(buffer, 0, offset);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex);
                }
            } while (totalSent < messages.Count);
        }

        protected async void SendMessage(Message msg)
        {
            var buffer = new byte[bufferSize];
            var offset = 0;
            if (!SerializeMessage(msg, buffer, ref offset))
                throw new ArgumentOutOfRangeException("Message data too big. It should not exceed 3997 chars.");
            try
            {
                await stream.WriteAsync(buffer, 0, offset);
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