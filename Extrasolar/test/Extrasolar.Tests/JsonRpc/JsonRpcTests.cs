using Extrasolar.JsonRpc;
using Extrasolar.JsonRpc.Types;
using Extrasolar.Tests.Mocks;
using Newtonsoft.Json.Linq;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Extrasolar.Tests.JsonRpc
{
    public class JsonRpcTests : IDisposable
    {
        public JsonRpcEndpoint Client { get; }
        public JsonRpcEndpoint Server { get; }

        public JsonRpcTests()
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
            Barrier responseReceived = new Barrier(2);
            string pong = "pong";
            Server.RequestPipeline.AddItemToEnd((req) =>
            {
                return new ResultResponse(req, pong);
            });
            Client.ResponsePipeline.AddItemToEnd((res) =>
            {
                Assert.Equal((res.Result as JValue).Value, pong);
                responseReceived.SignalAndWait();
                return true;
            });
            await Task.Factory.StartNew(async () =>
            {
                await Client.SendRequest(new Request("ping", null, "0"));
            });
            responseReceived.SignalAndWait();
        }

        [Fact]
        public async Task ResponseIdIsPresent()
        {
            Barrier responseReceived = new Barrier(2);
            string pong = "pong";
            Server.RequestPipeline.AddItemToEnd((req) =>
            {
                return new ResultResponse(req, pong);
            });
            Client.ResponsePipeline.AddItemToEnd((res) =>
            {
                Assert.NotNull(res.Id);
                responseReceived.SignalAndWait();
                return true;
            });
            await Task.Factory.StartNew(async () =>
            {
                await Client.SendRequest(new Request("ping", null, "0"));
            });
            responseReceived.SignalAndWait();
        }

        [Fact]
        public async Task ServerDoesNotRespondToNotifications()
        {
            Barrier responseReceived = new Barrier(2);
            string pong = "pong";
            Server.RequestPipeline.AddItemToEnd((req) =>
            {
                if (req.Id == null)
                {
                    responseReceived.SignalAndWait();
                    return null;
                }
                return new ResultResponse(req, pong);
            });
            Client.ResponsePipeline.AddItemToEnd((res) =>
            {
                // Server should not respond
                throw new NotImplementedException();
            });
            await Task.Factory.StartNew(async () =>
            {
                await Client.SendRequest(new Request("ping", null, null));
            });
            responseReceived.SignalAndWait();
        }

        [Fact]
        public async Task ServerSendsValidErrors()
        {
            Barrier responseReceived = new Barrier(2);
            string pong = "pong";
            Server.RequestPipeline.AddItemToEnd((req) =>
            {
                return new ErrorResponse(req, new Error(Error.JsonRpcErrorCode.ServerError, pong, null));
            });
            Client.ResponsePipeline.AddItemToEnd((res) =>
            {
                // Server should send response with server error
                Assert.Equal(res.Error.GetErrorCode(), Error.JsonRpcErrorCode.ServerError);
                Assert.Null(res.Result);
                responseReceived.SignalAndWait();
                return true;
            });
            await Task.Factory.StartNew(async () =>
            {
                await Client.SendRequest(new Request("ping", null, "0"));
            });
            responseReceived.SignalAndWait();
        }

        [Fact]
        public async Task ServerHandlesRequestArray()
        {
            // Barrier: 1 for sender, 4 receivers
            Barrier responseReceived = new Barrier(5);
            string pong = "pong";
            Server.RequestPipeline.AddItemToEnd((req) =>
            {
                return new ResultResponse(req, pong);
            });
            Client.ResponsePipeline.AddItemToEnd((res) =>
            {
                Assert.NotNull(res.Id);
                responseReceived.SignalAndWait();
                return true;
            });
            await Task.Factory.StartNew(async () =>
            {
                await Client.SendRequest(new Request[]
                {
                    new Request("ping", null, "0"),
                    new Request("ping", null, "1"),
                    new Request("ping", null, "2"),
                    new Request("ping", null, "3")
                });
            });
            responseReceived.SignalAndWait();
        }
    }
}