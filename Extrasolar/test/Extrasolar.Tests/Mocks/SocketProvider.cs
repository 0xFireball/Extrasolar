using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace Extrasolar.Tests.Mocks
{
    internal class SocketProvider
    {
        public static async Task<Tuple<TcpClient, TcpClient>> CreateSockets()
        {
            TcpListener listener = new TcpListener(IPAddress.Loopback, 0);
            listener.Start();
            var testPort = ((IPEndPoint)listener.LocalEndpoint).Port;
            var clientSock = new TcpClient();
            var serverSockTask = listener.AcceptTcpClientAsync();
            await clientSock.ConnectAsync(IPAddress.Loopback, testPort);
            var serverSock = await serverSockTask;
            return new Tuple<TcpClient, TcpClient>(clientSock, serverSock);
        }
    }
}