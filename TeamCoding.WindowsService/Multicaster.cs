using AwesomeSockets.Domain.Sockets;
using AwesomeSockets.Sockets;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ASBuffer = AwesomeSockets.Buffers.Buffer;
namespace TeamCoding.WindowsService
{
    public class Multicaster : IDisposable
    {
        private readonly int ListenPort;
        private CancellationTokenSource CancelTokenSource;
        private CancellationToken CancelToken;
        private readonly Task ServerTask;
        private readonly ConcurrentDictionary<ISocket, byte> ClientSockets = new ConcurrentDictionary<ISocket, byte>();
        private readonly List<Task> ListenTasks = new List<Task>();
        public Multicaster(int listenPort)
        {
            ListenPort = listenPort;

            CancelTokenSource = new CancellationTokenSource();
            CancelToken = CancelTokenSource.Token;
            ServerTask = new Task(ListenForConnections);
            ServerTask.Start();
        }
        public void ListenForConnections()
        {
            var taskFactory = new TaskFactory();
            var socket = AweSock.TcpListen(ListenPort);
            while (!CancelToken.IsCancellationRequested)
            {
                var clientSocket = AweSock.TcpAccept(socket); // TODO: Convert this to non-blocking!
                ClientSockets.TryAdd(clientSocket, 0);
                var listenTask = new Task(() => ListenForMessages(clientSocket), TaskCreationOptions.LongRunning);
                listenTask.ContinueWith(t =>
                {
                    lock (ListenTasks)
                    {
                        ListenTasks.Remove(t);
                    }
                }).ConfigureAwait(false);
                lock (ListenTasks)
                {
                    ListenTasks.Add(listenTask);
                }
            }
        }
        public void ListenForMessages(ISocket socket)
        {
            var receiveBuffer = ASBuffer.New();
            while (!CancelToken.IsCancellationRequested)
            {
                ASBuffer.ClearBuffer(receiveBuffer);
                Tuple<int, EndPoint> result = null;
                try
                {
                    result = AweSock.ReceiveMessage(socket, receiveBuffer); // TODO: Convert this to non-blocking!
                }
                catch(SocketException)
                {
                    byte _;
                    ClientSockets.TryRemove(socket, out _);
                    break;
                    // TODO: Determine which exceptions are intermittent and handle recovering before just bailing out
                }
                if (result.Item1 == 0) return;
                var sendBuffer  = ASBuffer.Duplicate(receiveBuffer);
                
                foreach (var client in ClientSockets.Keys)
                {
                    if (client != socket || System.Diagnostics.Debugger.IsAttached)
                    {
                        try
                        {
                            client.SendMessage(sendBuffer); // TODO: Convert this to non-blocking!
                        }
                        catch(SocketException)
                        {
                            // Swallow the exception trying to send, if it's not intermittent (connection closed etc) the receive message will throw and we handle it there (in another Task)
                        }
                    }
                }
            }
        }
        public void Dispose()
        {
            CancelTokenSource.Cancel();
            Task.WaitAll(ListenTasks.ToArray());
            ServerTask.Wait();
        }
    }
}
