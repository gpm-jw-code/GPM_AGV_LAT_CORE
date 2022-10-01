using GPM_AGV_LAT_CORE.Logger;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GPM_AGV_LAT_CORE.Protocols.Tcp
{
    public class TcpSocketClient
    {
        public ILogger logger;
        public Socket socket { get; set; }
        public string hostIP { get; }
        public int hostPort { get; }

        private ManualResetEvent serverReply = new ManualResetEvent(false);

        public event EventHandler<SocketStates> OnMessageReceive;

        public SocketStates socketState { get; set; } = new SocketStates(8192);
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

        internal bool Connect()
        {
            try
            {
                socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                socket.Connect(hostIP, hostPort);
                socketState.socket = socket;
                socket.BeginReceive(socketState.buffer, 0, socketState.bufferSize, SocketFlags.None, new AsyncCallback(ReceieveCallBack), socketState);

                return true;
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
                socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                socket.Connect(hostIP, hostPort);
                socketState.socket = socket;
                socket.BeginReceive(socketState.buffer, 0, socketState.bufferSize, SocketFlags.None, new AsyncCallback(ReceieveCallBack), socketState);
                return true;
            }
            catch (Exception ex)
            {
                errmsg = ex.Message;
                return false;
            }
        }

        private SocketStates serverReplyState;

        private void ReceieveCallBack(IAsyncResult ar)
        {
            try
            {
                SocketStates _socketState = (SocketStates)ar.AsyncState;
                int receieveLen = _socketState.socket.EndReceive(ar);
                _socketState.receieveLen = receieveLen;
                serverReplyState = _socketState;
                serverReply.Set();
                OnMessageReceive?.Invoke(this, _socketState);
                _socketState.ClearBuffer();
                socket.BeginReceive(_socketState.buffer, 0, _socketState.bufferSize, SocketFlags.None, ReceieveCallBack, _socketState);

            }
            catch (Exception ex)
            {

            }
        }

        internal async Task<SocketStates> Send(byte[] data, bool waitReply)
        {
            return await Task.Run(() =>
            {
                try
                {
                    serverReply.Reset();
                    socket.Send(data, data.Length, SocketFlags.None);
                    if (waitReply)
                        serverReply.WaitOne();
                    return serverReplyState;
                }
                catch (Exception)
                {
                    return new SocketStates();
                }
            });
        }

    }
}
