using Extrasolar.JsonRpc.Types;
using Extrasolar.Types;
using Newtonsoft.Json;
using System;
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
        }

        public Stream TransportStream { get; set; }
        protected StreamWriter DataWriter { get; private set; }
        protected StreamReader DataReader { get; private set; }
        public ClientMode Mode { get; }
        public bool Listening => true;

        private SemaphoreSlim _transportLock = new SemaphoreSlim(1, 1);

        public JsonRpcClient(Stream transportStream, ClientMode clientMode)
        {
            TransportStream = transportStream;
            Mode = clientMode;
            DataWriter = new StreamWriter(TransportStream);
            DataReader = new StreamReader(TransportStream);

            Task.Factory.StartNew(ReceiveDataEventLoop);
        }

        public async Task SendRequest(Request request)
        {
            await _transportLock.WaitAsync();
            await DataWriter.WriteLineAsync(request.ToString());
            await DataWriter.FlushAsync();
            _transportLock.Release();
        }

        public async Task ReceiveDataEventLoop()
        {
            while (Listening)
            {
                var dataJson = await DataReader.ReadLineAsync();
                if (dataJson != null)
                {
                    try
                    {
                        if (Mode == ClientMode.Request)
                        {
                            var response = JsonConvert.DeserializeObject<Response>(dataJson);
                            // Spawn new handler
                            var handlerTask = Task.Factory.StartNew(() => HandleReceivedResponse(response));
                        }
                        if (Mode == ClientMode.Response)
                        {
                            var request = JsonConvert.DeserializeObject<Request>(dataJson);
                            // Spawn new handler
                            var handlerTask = Task.Factory.StartNew(async () => await HandleReceivedRequest(request));
                        }
                    }
                    catch (JsonSerializationException)
                    {
                        // Invalid data
                    }
                }
            }
        }

        private void HandleReceivedResponse(Response response)
        {
            ResponseReceived?.Invoke(this, response);
            lock (ResponsePipeline)
            {
                foreach (var handler in ResponsePipeline.Handlers)
                {
                    var result = handler.Invoke(response);
                    if (result)
                    {
                        break;
                    }
                }
            }
        }

        private async Task HandleReceivedRequest(Request request)
        {
            RequestReceived?.Invoke(this, request);
            Response response = null;
            lock (RequestPipeline)
            {
                foreach (var handler in RequestPipeline.Handlers)
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
                await DataWriter.WriteLineAsync(response.ToString());
                await DataWriter.FlushAsync();
                _transportLock.Release();
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
        public Pipelines<Request, Response> RequestPipeline { get; } = new Pipelines<Request, Response>();

        /// <summary>
        /// A list of callbacks that receive responses to previous requests.
        /// If a handler returns `true` the response will be considered handled
        /// and the pipeline will end. Otherwise, the next handler will be called.
        /// </summary>
        public Pipelines<Response, bool> ResponsePipeline { get; } = new Pipelines<Response, bool>();

        /// <summary>
        /// A notify-only event that fires whenever a request is received
        /// </summary>
        public event EventHandler<Request> RequestReceived;

        /// <summary>
        /// A notify-only event that fires whenever a response is received
        /// </summary>
        public event EventHandler<Response> ResponseReceived;
    }
}