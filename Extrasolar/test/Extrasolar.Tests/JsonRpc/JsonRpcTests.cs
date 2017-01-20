using Extrasolar.JsonRpc;
using Extrasolar.JsonRpc.Types;
using Extrasolar.Tests.Mocks;
using Newtonsoft.Json.Linq;
using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Extrasolar.Tests.JsonRpc
{
    public class JsonRpcTests
    {
        [Fact]
        public void IsDisposable()
        {
            using (var rpcEndpoint = new JsonRpcEndpoint(new UnlimitedMemoryStream(), JsonRpcEndpoint.EndpointMode.Client))
            {
                // Do some work
                Assert.IsAssignableFrom(typeof(IDisposable), rpcEndpoint);
            }
        }

        [Fact]
        public async Task CanCommunicate()
        {
            int testPort = 12983;
            TcpListener listener = new TcpListener(IPAddress.Loopback, testPort);
            listener.Start();
            var clientSock = new TcpClient();
            var serverSockTask = listener.AcceptTcpClientAsync();
            await clientSock.ConnectAsync(IPAddress.Loopback, testPort);
            var serverSock = await serverSockTask;
            Barrier responseReceived = new Barrier(2);

            using (var client = new JsonRpcEndpoint(serverSock.GetStream(), JsonRpcEndpoint.EndpointMode.Client))
            {
                using (var server = new JsonRpcEndpoint(clientSock.GetStream(), JsonRpcEndpoint.EndpointMode.Server))
                {
                    string pong = "pong";
                    server.RequestPipeline.AddItemToEnd((req) =>
                    {
                        return new ResultResponse(req, pong);
                    });
                    client.ResponsePipeline.AddItemToEnd((res) =>
                    {
                        Assert.Equal((res.Result as JValue).Value, pong);
                        responseReceived.SignalAndWait();
                        return true;
                    });
                    await Task.Factory.StartNew(async () =>
                    {
                        await client.SendRequest(new Request("ping", null, "0"));
                    });
                    responseReceived.SignalAndWait();
                }
            }
        }
    }
}