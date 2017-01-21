using Extrasolar.IO;
using Extrasolar.IO.Transport;
using Extrasolar.Rpc;
using Extrasolar.Tests.Mocks;
using System;

namespace Extrasolar.Tests.Rpc
{
    public class TestClientFixture : IDisposable
    {
        public RpcService<ITestService> Service { get; }
        public RpcCaller<ITestService> Caller { get; }
        public ITestService Client { get; }

        public TestClientFixture()
        {
            var sockets =  SocketProvider.CreateSockets().Result;
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

        public void Dispose()
        {
            Service.Dispose();
            Caller.Dispose();
        }
    }
}