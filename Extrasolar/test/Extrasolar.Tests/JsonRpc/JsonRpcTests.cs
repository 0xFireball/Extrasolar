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
    public class JsonRpcTests : IClassFixture<TestEndpointsFixture>
    {
        private TestEndpointsFixture _fixture;

        public JsonRpcTests(TestEndpointsFixture fixture)
        {
            _fixture = fixture;
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
            _fixture.Server.RequestPipeline.AddItemToEnd((req) =>
            {
                return new ResultResponse(req, pong);
            });
            _fixture.Client.ResponsePipeline.AddItemToEnd((res) =>
            {
                Assert.Equal((res.Result as JValue).Value, pong);
                responseReceived.SignalAndWait();
                return true;
            });
            await Task.Factory.StartNew(async () =>
            {
                await _fixture.Client.SendRequest(new Request("ping", null, "0"));
            });
            responseReceived.SignalAndWait();
        }

        [Fact]
        public async Task ResponseIdIsPresent()
        {
            Barrier responseReceived = new Barrier(2);
            string pong = "pong";
            _fixture.Server.RequestPipeline.AddItemToEnd((req) =>
            {
                return new ResultResponse(req, pong);
            });
            _fixture.Client.ResponsePipeline.AddItemToEnd((res) =>
            {
                Assert.NotNull(res.Id);
                responseReceived.SignalAndWait();
                return true;
            });
            await Task.Factory.StartNew(async () =>
            {
                await _fixture.Client.SendRequest(new Request("ping", null, "0"));
            });
            responseReceived.SignalAndWait();
        }

        [Fact]
        public async Task ServerDoesNotRespondToNotifications()
        {
            Barrier responseReceived = new Barrier(2);
            string pong = "pong";
            _fixture.Server.RequestPipeline.AddItemToEnd((req) =>
            {
                if (req.Id == null)
                {
                    responseReceived.SignalAndWait();
                    return null;
                }
                return new ResultResponse(req, pong);
            });
            _fixture.Client.ResponsePipeline.AddItemToEnd((res) =>
            {
                // Server should not respond
                throw new NotImplementedException();
            });
            await Task.Factory.StartNew(async () =>
            {
                await _fixture.Client.SendRequest(new Request("ping", null, null));
            });
            responseReceived.SignalAndWait();
        }

        [Fact]
        public async Task ServerSendsValidErrors()
        {
            Barrier responseReceived = new Barrier(2);
            string pong = "pong";
            _fixture.Server.RequestPipeline.AddItemToEnd((req) =>
            {
                return new ErrorResponse(req, new Error(Error.JsonRpcErrorCode.ServerError, pong, null));
            });
            _fixture.Client.ResponsePipeline.AddItemToEnd((res) =>
            {
                // Server should send response with server error
                Assert.Equal(res.Error.GetErrorCode(), Error.JsonRpcErrorCode.ServerError);
                Assert.Null(res.Result);
                responseReceived.SignalAndWait();
                return true;
            });
            await Task.Factory.StartNew(async () =>
            {
                await _fixture.Client.SendRequest(new Request("ping", null, null));
            });
            responseReceived.SignalAndWait();
        }
    }
}