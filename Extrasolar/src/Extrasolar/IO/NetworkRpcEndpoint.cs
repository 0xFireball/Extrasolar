using Extrasolar.IO.Transport;
using Extrasolar.JsonRpc;
using Extrasolar.JsonRpc.Types;
using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace Extrasolar.IO
{
    public class NetworkRpcEndpoint : IRemoteRpcEndpoint, IDisposable
    {
        public JsonRpcEndpoint RpcLayer { get; }
        public JsonRpcEndpoint.EndpointMode Mode { get; set; }
        protected ITransportLayer Transport { get; set; }

        public NetworkRpcEndpoint(ITransportLayer transport, JsonRpcEndpoint.EndpointMode clientMode)
        {
            Transport = transport;
            Mode = clientMode;

            RpcLayer = new JsonRpcEndpoint(transport.GetStream(), Mode);
            if (Mode.HasFlag(JsonRpcEndpoint.EndpointMode.Server))
            {
                RpcLayer.RequestPipeline.AddItemToStart(HandleRpcRequest);
            }
            if (Mode.HasFlag(JsonRpcEndpoint.EndpointMode.Client))
            {
                RpcLayer.ResponsePipeline.AddItemToStart(HandleRpcResponse);
            }
        }

        private bool HandleRpcResponse(Response response)
        {
            // Store result and signal that response is ready
            ResultCache[response.Id] = response;
            RequestQueue[response.Id].Set();
            return false;
        }

        private Response HandleRpcRequest(Request request)
        {
            // Empty handler
            return null;
        }

        public async Task<Response> Request(Request request)
        {
            // Send request
            var resultReady = new AutoResetEvent(false);
            RequestQueue[request.Id] = resultReady;
            await RpcLayer.SendRequest(request);
            await Task.Run(() => resultReady.WaitOne());
            // Remove lock from queue
            AutoResetEvent removedEvent;
            RequestQueue.TryRemove(request.Id, out removedEvent);
            // Retrieve result
            var result = ResultCache[request.Id];
            Response dequeuedResponse;
            ResultCache.TryRemove(request.Id, out dequeuedResponse);
            return result;
        }

        public void Dispose()
        {
            RpcLayer.Dispose();
        }

        protected ConcurrentDictionary<string, AutoResetEvent> RequestQueue { get; } = new ConcurrentDictionary<string, AutoResetEvent>();
        protected ConcurrentDictionary<string, Response> ResultCache { get; } = new ConcurrentDictionary<string, Response>();
    }
}