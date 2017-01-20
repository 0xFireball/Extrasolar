using Extrasolar.IO;
using Extrasolar.IO.Transport;
using Extrasolar.Rpc;
using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace Extrasolar.Tests.Rpc
{
    public class TestClientFixture : IDisposable
    {
        public RpcService<ITestService> Service { get; }
        public RpcCaller<ITestService> Caller { get; }
        public ITestService Client { get; }

        public TestClientFixture()
        {
            var sockets = CreateSockets().Result;
            var clientSock = sockets.Item1;
            var serverSock = sockets.Item2;
            Service = new RpcService<ITestService>(
                new NetworkRpcService(new TcpTransportLayer(serverSock)
            ));
            Caller = new RpcCaller<ITestService>(
                new NetworkRpcClient(new TcpTransportLayer(clientSock)
            ));
            Service.Export(new TestService());
            Client = Caller.CreateClient();
        }

        private async Task<Tuple<TcpClient, TcpClient>> CreateSockets()
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

        public void Dispose()
        {
            Service.Dispose();
            Caller.Dispose();
        }
    }
}