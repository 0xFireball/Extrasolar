﻿using Extrasolar.JsonRpc.Types;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Extrasolar.JsonRpc
{
    public class JsonRpcClient : IDisposable
    {
        [Flags]
        public enum ClientMode
        {
            Request = 1 << 0,
            Response = 1 << 1,
            TwoWay = Request | Response
        }

        public Stream TransportStream { get; set; }
        protected StreamWriter DataWriter { get; private set; }
        protected StreamReader DataReader { get; private set; }
        public ClientMode Mode { get; }
        public bool Listening => Mode.HasFlag(ClientMode.Response);

        private SemaphoreSlim _transportLock = new SemaphoreSlim(1, 1);

        public JsonRpcClient(Stream transportStream, ClientMode clientMode = ClientMode.Request | ClientMode.Response)
        {
            TransportStream = transportStream;
            Mode = clientMode;
            if (Mode.HasFlag(ClientMode.Request))
            {
                DataWriter = new StreamWriter(TransportStream);
            }
            if (Mode.HasFlag(ClientMode.Response))
            {
                DataReader = new StreamReader(TransportStream);
            }

            if (Mode.HasFlag(ClientMode.Response))
            {
                Task.Factory.StartNew(ReceiveDataEventLoop);
            }
        }

        public async Task SendRequest(Request request)
        {
            await DataWriter.WriteLineAsync(request.ToString());
            await DataWriter.FlushAsync();
        }

        public async Task ReceiveDataEventLoop()
        {
            while (Listening)
            {
                var requestJson = await DataReader.ReadLineAsync();
                if (requestJson != null)
                {
                    try
                    {
                        var request = JsonConvert.DeserializeObject<Request>(requestJson);
                        // Spawn new handler
                        var handlerTask = Task.Factory.StartNew(async () => await HandleRequest(request));
                    }
                    catch (JsonSerializationException)
                    {
                        // Invalid data
                    }
                }
            }
        }

        private async Task HandleRequest(Request request)
        {
            RequestReceived?.Invoke(this, request);
            Response response = null;
            lock (RequestHandlers)
            {
                foreach (var handler in RequestHandlers)
                {
                    var resp = handler.Invoke(request);
                    if (resp != null)
                    {
                        response = resp;
                        break;
                    }
                }
            }
            if (response != null)
            {
                // Only if response was handled
                // Remember, notifications do not get a reply
                await _transportLock.WaitAsync();
                await DataWriter.WriteAsync(response.ToString());
                await DataWriter.FlushAsync();
                _transportLock.Release();
            }
        }

        public void AddRequestHandler(Func<Request, Response> handler)
        {
            lock (RequestHandlers)
            {
                RequestHandlers.Add(handler);
            }
        }

        public void Dispose()
        {
            DataWriter?.Dispose();
            DataReader?.Dispose();
        }

        /// <summary>
        /// A list of callbacks that process requests and return responses.
        /// They are evaluated in order until a Response object is received. If a handler
        /// returns null, the next handler will be invoked.
        /// </summary>
        protected List<Func<Request, Response>> RequestHandlers { get; } = new List<Func<Request, Response>>();

        /// <summary>
        /// A notify-only event that fires whenever a request is received
        /// </summary>
        public event EventHandler<Request> RequestReceived;
    }
}