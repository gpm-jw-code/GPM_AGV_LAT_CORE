using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Net;

namespace GPM_AGV_LAT_CORE.Protocols.Tcp
{
    public class TcpSocketServer : TcpSocketClient
    {
        public event EventHandler<SocketStates> OnClientMessageRev;
        public TcpListener tcpListener;
        public TcpSocketServer(string hostIP, int hostPort) : base(hostIP, hostPort)
        {

        }

        public bool Listen()
        {
            try
            {

                tcpListener = new TcpListener(IPAddress.Parse(hostIP), hostPort);
                socketState = new SocketStates()
                {
                    socket = tcpListener.Server
                };
                tcpListener.Start();
                tcpListener.BeginAcceptSocket(new AsyncCallback(AcceptSocketCallback), tcpListener);
                return true;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void AcceptSocketCallback(IAsyncResult ar)
        {  // Get the listener that handles the client request.
            TcpListener listener = (TcpListener)ar.AsyncState;
            Socket clientSocket = listener.EndAcceptSocket(ar);
            SocketStates _socketStaete = new SocketStates() { socket = clientSocket };
            clientSocket.BeginReceive(_socketStaete.buffer, 0, _socketStaete.bufferSize, SocketFlags.None, new AsyncCallback(ClientMessageRevCallback), _socketStaete);
            listener.BeginAcceptSocket(new AsyncCallback(AcceptSocketCallback), listener);

        }

        private void ClientMessageRevCallback(IAsyncResult ar)
        {
            SocketStates state = (SocketStates)ar.AsyncState;
            int rev = state.socket.EndReceive(ar);
            state.receieveLen = rev;
            OnClientMessageRev?.Invoke(this, state);
            state.ClearBuffer();
            state.socket.BeginReceive(state.buffer, 0, state.bufferSize, SocketFlags.None, new AsyncCallback(ClientMessageRevCallback), state);
        }
    }
}
