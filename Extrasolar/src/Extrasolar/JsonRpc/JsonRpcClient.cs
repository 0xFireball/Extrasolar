using Extrasolar.JsonRpc.Types;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Extrasolar.JsonRpc
{
    public class JsonRpcClient
    {
        [Flags]
        public enum ClientMode
        {
            Request,
            Response,
            TwoWay = Request | Response
        }

        public Stream TransportStream { get; set; }
        public StreamWriter DataWriter { get; private set; }
        public StreamReader DataReader { get; private set; }
        public ClientMode Mode { get; }
        public bool Listening => Mode.HasFlag(ClientMode.Response);

        private SemaphoreSlim _transportLock = new SemaphoreSlim(1, 1);

        public JsonRpcClient(Stream transportStream, ClientMode clientMode = ClientMode.Request | ClientMode.Response)
        {
            TransportStream = transportStream;
            DataWriter = new StreamWriter(TransportStream);
            DataReader = new StreamReader(TransportStream);
            Mode = clientMode;
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
                var request = JsonConvert.DeserializeObject<Request>(requestJson);
                // Spawn new handler
                var handlerTask = Task.Factory.StartNew(async () => await HandleRequest(request));
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
            await _transportLock.WaitAsync();
            await DataWriter.WriteAsync(response.ToString());
            await DataWriter.FlushAsync();
            _transportLock.Release();
        }

        /// <summary>
        /// A list of callbacks that process requests and return responses.
        /// They are evaluated in order until a Response object is received. If a handler
        /// returns null, the next handler will be invoked.
        /// </summary>
        public List<Func<Request, Response>> RequestHandlers { get; } = new List<Func<Request, Response>>();

        /// <summary>
        /// A notify-only event that fires whenever a request is received
        /// </summary>
        public event EventHandler<Request> RequestReceived;
    }
}