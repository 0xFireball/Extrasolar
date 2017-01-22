using System.IO;
using System.Net.Sockets;

namespace Extrasolar.IO.Transport
{
    public class TcpTransportLayer : ITransportLayer
    {
        private readonly TcpClient _tcpClient;

        public TcpTransportLayer(TcpClient tcpClient)
        {
            _tcpClient = tcpClient;
        }

        public Stream GetStream()
        {
            return _tcpClient.GetStream();
        }
    }
}