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
    }
}