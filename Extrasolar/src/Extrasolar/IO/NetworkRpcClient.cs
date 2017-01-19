using Extrasolar.IO.Transport;
using Extrasolar.JsonRpc;
using Extrasolar.JsonRpc.Types;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Extrasolar.IO
{
    public class NetworkRpcClient : IRemoteRpcClient
    {
        public JsonRpcClient RpcLayer { get; }
        public JsonRpcClient.ClientMode Mode { get; set; }
        protected ITransportLayer Transport { get; set; }

        public NetworkRpcClient(ITransportLayer transport, JsonRpcClient.ClientMode clientMode)
        {
            Transport = transport;
            Mode = clientMode;

            RpcLayer = new JsonRpcClient(transport.GetStream(), Mode);
            if (Mode.HasFlag(JsonRpcClient.ClientMode.Response))
            {
                RpcLayer.RequestPipeline.AddItemToStart(HandleRpcRequest);
            }
            if (Mode.HasFlag(JsonRpcClient.ClientMode.Request))
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
            // Retrieve result
            var result = ResultCache[request.Id];
            ResultCache.Remove(request.Id);
            return result;
        }

        protected Dictionary<string, AutoResetEvent> RequestQueue { get; } = new Dictionary<string, AutoResetEvent>();
        protected Dictionary<string, Response> ResultCache { get; } = new Dictionary<string, Response>();
    }
}