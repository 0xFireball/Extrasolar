using Extrasolar.JsonRpc;
using Extrasolar.JsonRpc.Types;
using Extrasolar.Tests.Mocks;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
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
            Barrier responseReceived = new Barrier(2);
            var commStream = new UnlimitedMemoryStream();
            using (var sw = new StreamWriter(commStream))
            {
                sw.WriteLine(); // Add trailing data
                sw.Flush();
                sw.BaseStream.Position = 0; // Reset position
                using (var client = new JsonRpcEndpoint(commStream, JsonRpcEndpoint.EndpointMode.Client))
                {
                    using (var server = new JsonRpcEndpoint(commStream, JsonRpcEndpoint.EndpointMode.Server))
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
                        await client.SendRequest(new Request("ping", null, "0"));
                        responseReceived.SignalAndWait();
                    }
                }
            }
        }
    }
}