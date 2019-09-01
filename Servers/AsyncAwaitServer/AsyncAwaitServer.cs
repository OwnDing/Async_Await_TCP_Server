using Blank_TCP_Server.Function;
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Blank_TCP_Server.Servers.AsyncAwaitServer
{
    public class AsyncAwaitServer
    {
        private Int32 numConnectedSockets;
        CancellationTokenSource cts;
        TcpListener listener;
        private bool isRuning;
        private Int32 maxConnectedClients;

        private ConcurrentDictionary<string, TcpClient> clients;
        /// <summary>
        /// tell the main form to delete or add connected info
        /// </summary>
        public enum ConnectionStatus {delete,add}

        /// <summary>
        /// uodate the main form tcp connections
        /// </summary>
        /// <param name="ip">IP</param>
        /// <param name="status">Stattus</param>
        public delegate void ChangeListView(string ip, ConnectionStatus status);
        public event ChangeListView Eventlistview;
        private void UpdateListView(string ip, ConnectionStatus status)
        {
            Eventlistview?.Invoke(ip, status);
        }

        private delegate void DataChanged(Message data);
        private event DataChanged DataEvent;
        //
        StreamToTxt txt = new StreamToTxt();
        MessageQueue tq = new MessageQueue(2);

        public AsyncAwaitServer(int port, int maxConnectedClients)
        {
            cts = new CancellationTokenSource();
            listener = new TcpListener(IPAddress.Any, port);
            this.numConnectedSockets = 0;
            this.maxConnectedClients = maxConnectedClients;
            clients = new ConcurrentDictionary<string, TcpClient>();
        }
        /// <summary>
        /// start tcp server
        /// </summary>
        public void Run()
        {           
            try
            {
                listener.Start();
                isRuning = true;
                var task = Task.Run(() => AcceptClientsAsync(listener, cts.Token));
                if (task.IsFaulted)
                    task.Wait();
           
                DataEvent += tq.EnqueueTask;
            }
            finally
            {
            }
        }
        /// <summary>
        /// stop tcp server
        /// </summary>
        public void Stop()
        {
            try
            {
                cts.Cancel();
                listener.Stop();
                tq.Stop();
                tq.Dispose();
            }
            finally
            {
            }
            foreach (var client in clients.Values)
            {
                try
                {
                    ConnectionStatus cs = ConnectionStatus.delete;
                    string ip = client.Client.RemoteEndPoint.ToString();
                    UpdateListView(ip, cs);

                    client.Client.Close();
                }
                finally
                {

                }

            }
            clients.Clear();
            DataEvent -= tq.EnqueueTask;
            Console.Write("Server Stopped!");
        }

        private void ReStartListener()
        {
            isRuning = true;
            listener.Start();
            DataEvent += tq.EnqueueTask;
            var task = Task.Run(() => AcceptClientsAsync(listener, cts.Token));
        }

        async Task AcceptClientsAsync(TcpListener listener, CancellationToken ct)
        {
            var ip = string.Empty;
            while (!ct.IsCancellationRequested)
            {
                if (numConnectedSockets >= this.maxConnectedClients)
                {
                    isRuning = false;
                    listener.Stop();
                    break;
                }

                
                var client = await listener.AcceptTcpClientAsync().WithWaitCancellation(ct);
                ip = client.Client.RemoteEndPoint.ToString();
                var task = Task.Run(() => EchoAsync(client, ip, ct));
            }
        }

        

        async Task EchoAsync(TcpClient client,string ip,CancellationToken ct)
        {
            Console.WriteLine("New client ({0}) connected", ip);
            var buf = new byte[4096 * 3];

            using (client)
            {
                //if (!client.Client.Connected) return;
                if (client.Client.Poll(0, SelectMode.SelectRead))
                {
                    byte[] buff = new byte[1];
                    if (client.Client.Receive(buff, SocketFlags.Peek) == 0)
                    {
                        buff = null;
                        return;
                    }
                    buff = null;
                }

                Interlocked.Increment(ref this.numConnectedSockets);
                Console.WriteLine("Client connection accepted. There are {0} clients connected to the server",
                    this.numConnectedSockets);
                clients.AddOrUpdate(ip, client, (n, o) => { return o; });
                ConnectionStatus connectionstatus = ConnectionStatus.add;
                UpdateListView(ip, connectionstatus);
                using (var stream = client.GetStream())
                {
                    while (!ct.IsCancellationRequested)
                    {
                        var timeoutTask = Task.Delay(TimeSpan.FromSeconds(610));
                        var amountReadTask = stream.ReadAsync(buf, 0, buf.Length, ct);

                        var completedTask = await Task.WhenAny(timeoutTask, amountReadTask)
                                                      .ConfigureAwait(false);
                        if (completedTask == timeoutTask)
                        {
                            break;
                        }

                        if (amountReadTask.IsFaulted || amountReadTask.IsCanceled)
                        {
                            break;
                        }
                        var amountRead = amountReadTask.Result;
                        if (amountRead == 0) { break; }
                        Message ms = new Message()
                        {
                            ip = ip,
                            data = Encoding.ASCII.GetString(buf, 0, amountRead)
                        };
                        if (ms.data != string.Empty)
                        {
                            DataEvent(ms);
                        }
                    }
                }
            }
            buf = null;
            Interlocked.Decrement(ref this.numConnectedSockets);
            Console.WriteLine("Client ({0}) disconnected.There are {1} clients connected to the server", ip, numConnectedSockets);

            clients.TryRemove(ip, out TcpClient tcpClient);
            tcpClient.Close();
            tcpClient = null;
            ConnectionStatus cs = ConnectionStatus.delete;
            UpdateListView(ip, cs);
            if (numConnectedSockets < this.maxConnectedClients && isRuning == false)
            {
                ReStartListener();
            }
        }

        #region send
        /// <summary>
        /// Send the message to the specified client
        /// </summary>
        /// <param name="tcpClient">specified client</param>
        /// <param name="datagram">message</param>
        private void Send(TcpClient tcpClient, byte[] datagram)
        {
            if (tcpClient == null)
                throw new ArgumentNullException("tcpClient");

            if (datagram == null)
                throw new ArgumentNullException("datagram");

            try
            {
                NetworkStream stream = tcpClient.GetStream();
                if (stream.CanWrite)
                {
                    stream.BeginWrite(datagram, 0, datagram.Length, HandleDatagramWritten, tcpClient);
                }
            }
            catch (ObjectDisposedException ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        /// <summary>
        /// Send the message to the specified client
        /// </summary>
        /// <param name="tcpClient">specified client</param>
        /// <param name="datagram">message</param>
        private void Send(TcpClient tcpClient, string datagram)
        {
            Send(tcpClient, Encoding.ASCII.GetBytes(datagram));
        }

        /// <summary>
        /// Send the message to the specified client
        /// </summary>
        /// <param name="datagram">message</param>
        public void SendToAll(byte[] datagram)
        {
            foreach (var client in clients.Values)
            {
                Send(client, datagram);
            }
        }

        /// <summary>
        /// Send the message to the specified client
        /// </summary>
        /// <param name="datagram">message</param>
        public void SendToAll(string datagram)
        {
            SendToAll(Encoding.ASCII.GetBytes(datagram));
        }
        /// <summary>
        /// Send the message to the specified client
        /// </summary>
        /// <param name="ip">client's IP</param>
        /// <param name="datagram">message</param>
        public void SendToSelectedClient(string ip,string datagram)
        {
            Send(clients[ip], datagram);
        }

        public void SendToSelectedClient(string ip, byte[] datagram)
        {
            Send(clients[ip], datagram);
        }

        private void HandleDatagramWritten(IAsyncResult ar)
        {
            try
            {
                ((TcpClient)ar.AsyncState).GetStream().EndWrite(ar);
            }
            catch (ObjectDisposedException ex)
            {
                //ExceptionHandler.Handle(ex);
                Console.WriteLine(ex.ToString());
            }
            catch (InvalidOperationException ex)
            {
                //ExceptionHandler.Handle(ex);
                Console.WriteLine(ex.ToString());
            }
            catch (IOException ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }
        #endregion

        #region WriteInfoToTxtFile
        private async Task WriteInfoAsync(string info)
        {
            try
            {
                Task task = Task.Run(() => txt.WriteInfo(info));
                await task.ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in wtite info to txt!  {0}", ex.ToString());
            }
        }
        #endregion
    }
}
