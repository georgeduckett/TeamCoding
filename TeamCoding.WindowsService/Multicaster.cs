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
        private ISocket ListenSocket;
        public Multicaster(int listenPort)
        {
            if (Environment.UserInteractive)
            {
                Console.WriteLine();
                Console.WriteLine($"Listening on port {listenPort}.");
            }

            ListenPort = listenPort;

            CancelTokenSource = new CancellationTokenSource();
            CancelToken = CancelTokenSource.Token;
            ServerTask = new Task(ListenForConnections);
            ServerTask.Start();
        }
        public void ListenForConnections()
        {
            var taskFactory = new TaskFactory();
            ListenSocket = AweSock.TcpListen(ListenPort);
            while (!CancelToken.IsCancellationRequested)
            {
                try
                {
                    var clientSocket = AweSock.TcpAccept(ListenSocket);
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
                    listenTask.Start();
                }
                catch (SocketException)
                {
                    if (!CancelToken.IsCancellationRequested)
                    {
                        try
                        {
                            // Try and re-connect.
                            Thread.Sleep(1000);
                            ListenSocket = AweSock.TcpListen(ListenPort);
                        }
                        catch (SocketException)
                        {

                        }
                    }
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
                    result = AweSock.ReceiveMessage(socket, receiveBuffer);
                }
                catch(SocketException)
                {
                    ClientSockets.TryRemove(socket, out byte _);
                    break;
                    // TODO: Determine which exceptions are intermittent and handle recovering before just bailing out
                }
                if (result.Item1 == 0) return;
                var sendBuffer  = ASBuffer.Duplicate(receiveBuffer);
                ASBuffer.FinalizeBuffer(sendBuffer);
                
                foreach (var client in ClientSockets.Keys)
                {
#if RELEASE
                    if (client != socket)
#endif
                    { // TODO: Does this work for displaying myself?
                        try
                        {
                            client.SendMessage(sendBuffer);
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

            ListenSocket?.Close();
            foreach (var client in ClientSockets.Keys)
            {
                client.Close();
            }

            if (ListenTasks.Count != 0)
            {
                Task.WaitAll(ListenTasks.ToArray());
            }
            ServerTask.Wait();
        }
    }
}
