using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Net;
using GPM_AGV_LAT_CORE.Logger;

namespace GPM_AGV_LAT_CORE.Protocols.Tcp
{
    public class TcpSocketServer : TcpSocketClient
    {
        private ILogger logger = new LoggerInstance(typeof(TcpSocketServer));
        public event EventHandler<Socket> OnClientConnected;
        public new event EventHandler<SocketStates> OnMessageReceive;
        public TcpSocketServer(string hostIP, int hostPort) : base(hostIP, hostPort)
        {

        }
        public async Task<bool> Listen()
        {
            return await Task.Run(() =>
             {
                 try
                 {
                     socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                     socket.ReceiveBufferSize = 32768;
                     socket.SendBufferSize = 32768;
                     socketState = new SocketStates()
                     {
                         socket = socket
                     };
                     IPEndPoint ipEndPoint = new IPEndPoint(IPAddress.Parse(hostIP), hostPort);

                     socket.Bind(ipEndPoint);
                     socket.Listen(10);
                     socket.BeginAccept(new AsyncCallback(AcceptSocketCallback), socket);
                     logger.InfoLog($"Server build success. Now listening:{hostIP}:{hostPort}");
                     return true;
                 }
                 catch (Exception ex)
                 {
                     logger.ErrorLog(ex.StackTrace, ex);
                     return false;
                 }
             });
        }

        private void AcceptSocketCallback(IAsyncResult ar)
        {  // Get the listener that handles the client request.

            Socket serverSocekt = (Socket)ar.AsyncState;
            Socket clientSocket = serverSocekt.EndAccept(ar);
            OnClientConnected?.Invoke(this, clientSocket);
            SocketStates _socketStaete = new SocketStates() { socket = clientSocket };
            clientSocket.BeginReceive(_socketStaete.buffer, 0, _socketStaete.bufferSize, SocketFlags.None, new AsyncCallback(ClientMessageRevCallback), _socketStaete);
            socket.BeginAccept(new AsyncCallback(AcceptSocketCallback), socket);

        }

        private void ClientMessageRevCallback(IAsyncResult ar)
        {
            try
            {
                SocketStates state = (SocketStates)ar.AsyncState;
                Socket socket = state.socket;
                int rev = 0;

                try
                {
                    rev = socket.EndReceive(ar);
                }
                catch (Exception ex)
                {
                    logger.ErrorLog(ex.StackTrace, ex);
                    return;
                }
                if (rev > 0)
                {
                    state.receieveLen = rev;
                    OnMessageReceive?.Invoke(this, state);
                    state.ClearBuffer();
                    Task.Factory.StartNew(() => socket.BeginReceive(state.buffer, 0, state.bufferSize, 0, ClientMessageRevCallback, state));
                }

            }
            catch (Exception ex)
            {
                logger.ErrorLog(ex.StackTrace, ex);
            }
        }
    }
}
