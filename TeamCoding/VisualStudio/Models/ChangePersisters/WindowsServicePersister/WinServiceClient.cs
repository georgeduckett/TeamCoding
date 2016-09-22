using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AwesomeSockets.Domain.Sockets;
using AwesomeSockets.Sockets;
using TeamCoding.Options;
using ASBuffer = AwesomeSockets.Buffers.Buffer;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.IO;

namespace TeamCoding.VisualStudio.Models.ChangePersisters.WindowsServicePersister
{
    public class WinServiceClient : IDisposable
    {
        public class MessageReceivedEventArgs : EventArgs { public byte[] Message; }

        private ISocket Socket;
        public event EventHandler<MessageReceivedEventArgs> MessageReceived;
        private readonly Property<string> IPAddressSetting;
        private CancellationTokenSource CancelTokenSource;
        private CancellationToken CancelToken;
        private Task ListenTask;
        public WinServiceClient(Property<string> ipAddressSetting)
        {
            IPAddressSetting = ipAddressSetting;
            IPAddressSetting.Changed += IpAddressSetting_Changed;
            ListenTask = ListenAsync();
        }
        private void IpAddressSetting_Changed(object sender, EventArgs e)
        {
            Disconnect();
            ListenTask = ListenAsync();
        }
        private Task ListenAsync()
        {
            CancelTokenSource = new CancellationTokenSource();
            CancelToken = CancelTokenSource.Token;

            var listenTask = new Task(Listen, TaskCreationOptions.LongRunning);
            listenTask.Start();

            return listenTask;
        }
        private void Listen()
        {
            try
            {
                if (IPAddressSetting.Value?.Contains(':') ?? false)
                {
                    Socket = AweSock.TcpConnect(IPAddressSetting.Value.Split(':')[0], int.Parse(IPAddressSetting.Value.Split(':')[1]));
                    ListenForMessages(Socket);
                }
            }
            catch(SocketException)
            {
                Thread.Sleep(1000);
                if (!CancelToken.IsCancellationRequested)
                {
                    // TODO: Handle socket exception (try and re-connect after a while?)
                }
            }
        }
        private void ListenForMessages(ISocket socket)
        {
            var receiveBuffer = ASBuffer.New();
            while (!CancelToken.IsCancellationRequested)
            {
                ASBuffer.ClearBuffer(receiveBuffer);
                Tuple<int, EndPoint> result = null;
                result = AweSock.ReceiveMessage(socket, receiveBuffer);
                ASBuffer.FinalizeBuffer(receiveBuffer);
                if (result.Item1 == 0) return;

                var length = ASBuffer.Get<short>(receiveBuffer);
                var bytes = new byte[length];
                ASBuffer.BlockCopy(ASBuffer.GetBuffer(receiveBuffer), sizeof(short), bytes, 0, length);
                MessageReceived?.Invoke(this, new MessageReceivedEventArgs() { Message = bytes });
            }
        }
        public void SendModel(RemoteIDEModel model)
        {
            if (Socket != null)
            {
                using (var ms = new MemoryStream())
                {
                    ProtoBuf.Serializer.Serialize(ms, model);

                    var buffer = ASBuffer.New((int)ms.Length + sizeof(short));
                    ASBuffer.ClearBuffer(buffer);
                    // TODO: Remove AwesomeSocket and roll my own or find another because of crap like this
                    ASBuffer.Add(buffer, BitConverter.GetBytes((short)ms.Length).Concat(ms.ToArray()).ToArray());
                    ASBuffer.FinalizeBuffer(buffer);
                    try
                    {
                        Socket.SendMessage(buffer);
                    }
                    catch (SocketException ex)
                    {
                        TeamCodingPackage.Current.Logger.WriteError(ex);
                    }
                }
            }
        }
        private void Disconnect()
        {
            Socket?.Close();
            CancelTokenSource.Cancel();
            ListenTask?.Wait();
        }
        public void Dispose()
        {
            Disconnect();
            // TODO: Close the connection
            IPAddressSetting.Changed -= IpAddressSetting_Changed;
        }
    }
}
