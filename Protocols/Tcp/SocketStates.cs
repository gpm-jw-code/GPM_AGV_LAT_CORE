using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace GPM_AGV_LAT_CORE.Protocols.Tcp
{
    public class SocketStates
    {
        private int _bufferSize = 16384;
        public int bufferSize
        {
            get => _bufferSize;
            set
            {
                _bufferSize = value;
                buffer = new byte[_bufferSize];
            }
        }
        public Socket socket;
        public byte[] buffer = new byte[16384];
        public SocketStates(int bufferSize = 16384)
        {
            this.bufferSize = bufferSize;
        }

        private int _receieveLen = 0;
        internal int receieveLen
        {
            get => _receieveLen;
            set
            {
                try
                {
                    _receieveLen = value;
                    revDataBytes = new byte[_receieveLen];
                    Array.Copy(buffer, 0, revDataBytes, 0, _receieveLen);
                    ASCIIRev = Encoding.ASCII.GetString(revDataBytes);
                }
                catch (Exception ex)
                {
                    throw ex;
                }

            }
        }

        public string ASCIIRev { get; private set; }

        public byte[] revDataBytes { get; private set; }

        internal void ClearBuffer()
        {
            buffer = new byte[_bufferSize];
        }
    }
}
