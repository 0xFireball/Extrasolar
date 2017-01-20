using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using Xunit;

namespace Extrasolar.Tests.Rpc
{
    public class RpcTransferTests
    {
        [Fact]
        public async Task CanCallParameterless()
        {
            var sockets = await CreateSockets();
            var clientSock = sockets.Item1;
            var serverSock = sockets.Item2;

        }

        public async Task<Tuple<TcpClient, TcpClient>> CreateSockets()
        {
            int testPort = 12983;
            TcpListener listener = new TcpListener(IPAddress.Loopback, testPort);
            listener.Start();
            var clientSock = new TcpClient();
            var serverSockTask = listener.AcceptTcpClientAsync();
            await clientSock.ConnectAsync(IPAddress.Loopback, testPort);
            var serverSock = await serverSockTask;
            return new Tuple<TcpClient, TcpClient>(clientSock, serverSock);
        }
    }
}