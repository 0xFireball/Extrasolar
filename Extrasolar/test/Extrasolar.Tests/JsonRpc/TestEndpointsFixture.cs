using Extrasolar.JsonRpc;
using Extrasolar.Tests.Mocks;
using System;
using System.Threading;

namespace Extrasolar.Tests.JsonRpc
{
    public class TestEndpointsFixture : IDisposable
    {
        public JsonRpcEndpoint Client { get; }
        public JsonRpcEndpoint Server { get; }

        public TestEndpointsFixture()
        {
            var sockets = SocketProvider.CreateSockets().Result;
            var clientSock = sockets.Item1;
            var serverSock = sockets.Item2;

            Client = new JsonRpcEndpoint(clientSock.GetStream(), JsonRpcEndpoint.EndpointMode.Client);
            Server = new JsonRpcEndpoint(serverSock.GetStream(), JsonRpcEndpoint.EndpointMode.Server);
        }

        public void Dispose()
        {
            Client.Dispose();
            Server.Dispose();
        }
    }
}