using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Blank_TCP_Server.Function;

namespace Blank_TCP_Server.Servers.AsyncAwaitServer
{
    public class AsyncAwaitServer
    {
        private Int32 numConnectedSockets;
        CancellationTokenSource cts;
        TcpListener listener;
        private bool isRuning;
        private Int32 maxConnectedClients;

        private ConcurrentDictionary<string, TcpClient> _clients;
        /// <summary>
        /// tell the main form to delete or add connected info
        /// </summary>
        public enum ConnectionStatus { delete,add}

        /// <summary>
        /// uodate the main form tcp connections
        /// </summary>
        /// <param name="ip">IP</param>
        /// <param name="status">Stattus</param>
        public delegate void ChangeListView(string ip, ConnectionStatus status);
        public event ChangeListView eventlistview;
        private void UpdateListView(string ip, ConnectionStatus status)
        {
            if (eventlistview != null)
                eventlistview(ip, status);
        }

        private delegate void DataChanged(Message data);
        private event DataChanged dataEvent;
        //
        StreamToTxt txt = new StreamToTxt();
        MessageQueue tq = new MessageQueue(1);

        public AsyncAwaitServer(int port, int maxConnectedClients)
        {
            cts = new CancellationTokenSource();
            listener = new TcpListener(IPAddress.Any, port);
            this.numConnectedSockets = 0;
            this.maxConnectedClients = maxConnectedClients;
            _clients = new ConcurrentDictionary<string, TcpClient>();
        }
        /// <summary>
        /// start tcp server
        /// </summary>
        public void run()
        {           
            try
            {
                listener.Start();
                //just fire and forget. We break from the "forgotten" async loops
                //in AcceptClientsAsync using a CancellationToken from `cts`
                isRuning = true;
                var task=AcceptClientsAsync(listener, cts.Token);
                if (task.IsFaulted)
                    task.Wait();
           
                dataEvent += tq.EnqueueTask;
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
            cts.Cancel();
            listener.Stop();
            tq.Dispose();
            ConnectionStatus connectionstatus = ConnectionStatus.delete;
            foreach (var client in _clients.Values)
            {
                try
                {
                    client.Client.Disconnect(true);
                    UpdateListView(client.Client.RemoteEndPoint.ToString(), connectionstatus);
                }
                catch (Exception ex)
                {

                }
                
            }
            _clients.Clear();
            Console.Write("Server Stopped!");
        }

        private void reStartListener()
        {
            isRuning = true;
            listener.Start();
            var task = AcceptClientsAsync(listener, cts.Token);
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
                TcpClient client = await listener.AcceptTcpClientAsync().ConfigureAwait(false);
                ip = client.Client.RemoteEndPoint.ToString();
                //once again, just fire and forget, and use the CancellationToken
                //to signal to the "forgotten" async invocation.
                var task= EchoAsync(client, ip, ct);
            }
        }

        

        async Task EchoAsync(TcpClient client,string ip,CancellationToken ct)
        {
            Console.WriteLine("New client ({0}) connected", ip);
            string data;

            using (client)
            {
                var buf = new byte[4096];
                var stream = client.GetStream();
                if (!client.Client.Connected) return;
                Interlocked.Increment(ref this.numConnectedSockets);
                Console.WriteLine("Client connection accepted. There are {0} clients connected to the server",
                    this.numConnectedSockets);
                _clients.AddOrUpdate(ip, client, (n, o) => { return client; });
                ConnectionStatus connectionstatus = ConnectionStatus.add;
                UpdateListView(ip, connectionstatus);
                while (!ct.IsCancellationRequested)
                {
                    var timeoutTask = Task.Delay(TimeSpan.FromSeconds(250));
                    var amountReadTask = stream.ReadAsync(buf, 0, buf.Length, ct);

                    var completedTask = await Task.WhenAny(timeoutTask, amountReadTask)
                                                  .ConfigureAwait(false);
                    if (completedTask == timeoutTask)
                    {
                        var msg = Encoding.ASCII.GetBytes("Client timed out");
                        await stream.WriteAsync(msg, 0, msg.Length);
                        break;
                    }
                    //now we know that the amountTask is complete so
                    //we can ask for its Result without blocking
                    if (amountReadTask.IsFaulted || amountReadTask.IsCanceled) {
                        data = "Error:IsFaulted||IsCanceled   " + ip;
                        writeinfo(data);
                        break; }
                    var amountRead = amountReadTask.Result;
                    Message ms = new Message();
                    ms.ip = ip;
                    ms.data = Encoding.ASCII.GetString(buf, 0, amountRead);
                    if(ms.data!=string.Empty)
                        dataEvent(ms);
                    
                    if (amountRead == 0) break; //end of stream.
                }
            }
            Interlocked.Decrement(ref this.numConnectedSockets);
            Console.WriteLine("Client ({0}) disconnected.There are {1} clients connected to the server", ip,numConnectedSockets);
            data = "disconnected...   " + ip+"---Time:"+DateTime.Now.ToString();
            writeinfo(data);
            _clients.TryRemove(ip, out client);
            ConnectionStatus cs = ConnectionStatus.delete;
            UpdateListView(ip, cs);
            if (numConnectedSockets < this.maxConnectedClients&&isRuning==false)
            {
                reStartListener();
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
            foreach (var client in _clients.Values)
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
            Send(_clients[ip], datagram);
        }

        public void SendToSelectedClient(string ip, byte[] datagram)
        {
            Send(_clients[ip], datagram);
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
        private void writeinfo(string info)
        {
            try
            {
                txt.WriteInfo(info);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in wtite info to txt!  {0}", ex.ToString());
            }
        }
        #endregion
    }
}
