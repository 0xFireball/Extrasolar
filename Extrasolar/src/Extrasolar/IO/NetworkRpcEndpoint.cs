﻿using Extrasolar.IO.Transport;
using Extrasolar.JsonRpc;
using Extrasolar.JsonRpc.Types;
using System;
using System.Collections.Generic;
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
            // Retrieve result
            var result = ResultCache[request.Id];
            ResultCache.Remove(request.Id);
            return result;
        }

        public void Dispose()
        {
            RpcLayer.Dispose();
        }

        protected Dictionary<string, AutoResetEvent> RequestQueue { get; } = new Dictionary<string, AutoResetEvent>();
        protected Dictionary<string, Response> ResultCache { get; } = new Dictionary<string, Response>();
    }
}