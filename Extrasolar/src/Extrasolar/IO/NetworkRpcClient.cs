﻿using Extrasolar.JsonRpc;
using Extrasolar.JsonRpc.Types;
using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace Extrasolar.IO
{
    public class NetworkRpcClient
    {
        public JsonRpcClient RpcLayer { get; }
        public JsonRpcClient.ClientMode Mode { get; set; }
        protected TcpClient TcpClient { get; set; }

        public NetworkRpcClient(TcpClient tcpClient, JsonRpcClient.ClientMode clientMode)
        {
            TcpClient = tcpClient;
            Mode = clientMode;

            RpcLayer = new JsonRpcClient(tcpClient.GetStream(), Mode);
            if (Mode.HasFlag(JsonRpcClient.ClientMode.Response))
            {
                RpcLayer.AddRequestHandler(HandleRpcRequest);
            }
            if (Mode.HasFlag(JsonRpcClient.ClientMode.Request))
            {
                RpcLayer.AddResponseHandler(HandleRpcResponse);
            }
        }

        private void HandleRpcResponse(Response response)
        {
            // Store result and signal that response is ready
            ResultCache[response.Id] = response;
            RequestQueue[response.Id].Set();
        }

        private Response HandleRpcRequest(Request request)
        {
            throw new NotImplementedException();
        }

        private async Task<Response> Request(Request request)
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