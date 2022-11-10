using GPM_AGV_LAT_CORE.Logger;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GPM_AGV_LAT_CORE.Protocols.Tcp
{
    public class TcpSocketClient : IDisposable
    {
        public ILogger logger;
        public Socket socket { get; set; }
        public string hostIP { get; }
        public int hostPort { get; }

        public string localIP { get; }
        public int localPort { get; }


        private ManualResetEvent serverReply = new ManualResetEvent(false);

        public event EventHandler<SocketStates> OnMessageReceive;
        public event EventHandler<byte[]> OnMessageSend;
        public event EventHandler OnDisconnect;
        public event EventHandler OnSendTimeout;

        public SocketStates socketState { get; set; } = new SocketStates(65536);
        public TcpSocketClient()
        {
            logger = new LoggerInstance(GetType());
        }
        public TcpSocketClient(string hostIP, int hostPort)
        {
            logger = new LoggerInstance(GetType());
            this.hostIP = hostIP;
            this.hostPort = hostPort;
        }
        public TcpSocketClient(string hostIP, int hostPort, string localIP, int localPort)
        {
            logger = new LoggerInstance(GetType());
            this.hostIP = hostIP;
            this.hostPort = hostPort;

            this.localIP = localIP;
            this.localPort = localPort;
        }
        public bool Connect()
        {
            try
            {
                IPEndPoint ipLocalEndPoint = new IPEndPoint(IPAddress.Parse(localIP), localPort);
                TcpClient clientSocket = new TcpClient(ipLocalEndPoint);
                clientSocket.Connect(hostIP, hostPort);
                socket = clientSocket.Client;

                //socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                //socket.Connect(hostIP, hostPort);

                socketState.socket = socket;
                socket.BeginReceive(socketState.buffer, 0, socketState.bufferSize, SocketFlags.None, new AsyncCallback(ReceieveCallBack), socketState);

                return true;
            }
            catch (SocketException)
            {
                OnDisconnect?.Invoke(this, null);
                return false;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
        internal bool Connect(out string errmsg)
        {
            errmsg = "";
            try
            {
                IPEndPoint ipLocalEndPoint = new IPEndPoint(IPAddress.Parse(localIP), localPort);
                TcpClient clientSocket = new TcpClient(ipLocalEndPoint);
                clientSocket.Connect(hostIP, hostPort);
                socket = clientSocket.Client;

                //socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                //socket.Connect(hostIP, hostPort);
                clientSocket.Client.ReceiveBufferSize = int.MaxValue;
                socketState.socket = socket;
                socket.BeginReceive(socketState.buffer, 0, socketState.bufferSize, SocketFlags.None, new AsyncCallback(ReceieveCallBack), socketState);

                return true;
            }
            catch (SocketException ex)
            {
                errmsg = ex.Message;
                OnDisconnect?.Invoke(this, null);
                return false;
            }
            catch (Exception ex)
            {
                errmsg = ex.Message;
                return false;
            }
        }

        private SocketStates serverReplyState;
        private bool disposedValue;

        private void ReceieveCallBack(IAsyncResult ar)
        {
            try
            {
                SocketStates _socketState = (SocketStates)ar.AsyncState;
                int receieveLen = _socketState.socket.EndReceive(ar);
                _socketState.receieveLen = receieveLen;
                serverReplyState = _socketState;
                serverReply.Set();
                if (receieveLen != 0)
                    OnMessageReceive?.Invoke(this, _socketState);

                _socketState.ClearBuffer();
                Task.Factory.StartNew(() => socket.BeginReceive(_socketState.buffer, 0, _socketState.bufferSize, SocketFlags.None, ReceieveCallBack, _socketState));

            }
            catch (SocketException)
            {
                OnDisconnect?.Invoke(this, null);
            }
            catch (Exception ex)
            {

            }
        }
        public void Disconnect()
        {
            try
            {
                socket.Disconnect(true);
                socket.Close();
            }
            catch (Exception)
            {

            }
            OnDisconnect?.Invoke(this, null);
        }

        internal async Task<SocketStates> Send(byte[] data, bool waitReply, string check_match_str = null)
        {
            try
            {
                bool repplyed = false;
                bool timeout = false;
                if (waitReply)
                {
                    CancellationTokenSource cancel = new CancellationTokenSource(5000);
                    _ = Task.Factory.StartNew(async () =>
                    {
                        while (repplyed == false)
                        {
                            if (cancel.IsCancellationRequested)
                            {
                                timeout = true;
                                serverReply.Set();
                                return;
                            }
                            await Task.Delay(100);
                        }
                        timeout = false;
                    }, cancel.Token);
                }

                var return_state = await Task.Run(() =>
                {
                    try
                    {
                        serverReply.Reset();

                        int sendout_bytes = socket.Send(data);
                        if (sendout_bytes > 0)
                            OnMessageSend?.Invoke(this, data);

                        if (waitReply)
                        {
                            if (check_match_str != null)
                            {

                                while (!serverReplyState.ASCIIRev.Contains(check_match_str))
                                {
                                    Thread.Sleep(1);
                                }
                            }
                            else

                                serverReply.WaitOne();

                            repplyed = true;
                        }
                        return serverReplyState;
                    }
                    catch (SocketException)
                    {
                        OnDisconnect?.Invoke(this, null);
                        return new SocketStates();
                    }
                    catch (Exception)
                    {
                        return new SocketStates();
                    }
                });
                if (waitReply)
                {
                    if (timeout)
                    {
                        OnSendTimeout?.Invoke(this, null);
                        return new SocketStates();
                    }
                    else
                        return return_state;
                }
                else
                    return return_state;

            }
            catch (SocketException)
            {
                OnDisconnect?.Invoke(this, null);
                return new SocketStates();
            }
            catch (Exception ex)
            {
                throw;
            }

        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: 處置受控狀態 (受控物件)
                }

                // TODO: 釋出非受控資源 (非受控物件) 並覆寫完成項
                // TODO: 將大型欄位設為 Null
                Disconnect();
                disposedValue = true;
            }
        }

        // // TODO: 僅有當 'Dispose(bool disposing)' 具有會釋出非受控資源的程式碼時，才覆寫完成項
        // ~TcpSocketClient()
        // {
        //     // 請勿變更此程式碼。請將清除程式碼放入 'Dispose(bool disposing)' 方法
        //     Dispose(disposing: false);
        // }

        public void Dispose()
        {
            // 請勿變更此程式碼。請將清除程式碼放入 'Dispose(bool disposing)' 方法
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
