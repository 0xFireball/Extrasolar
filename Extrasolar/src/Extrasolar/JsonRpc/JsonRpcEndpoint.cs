using Extrasolar.JsonRpc.Types;
using Extrasolar.Types;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Extrasolar.JsonRpc
{
    /// <summary>
    /// Represents an endpoint (client or server) of the Extrasolar RPC protocol
    /// </summary>
    public class JsonRpcEndpoint : IDisposable
    {
        [Flags]
        public enum EndpointMode
        {
            Client = 1 << 0,
            Server = 1 << 1,
        }

        public Stream TransportStream { get; set; }
        protected StreamWriter DataWriter { get; private set; }
        protected StreamReader DataReader { get; private set; }
        public EndpointMode Mode { get; }
        public bool Listening = true;

        private SemaphoreSlim _transportLock = new SemaphoreSlim(1, 1);
        private CancellationToken _eventLoopCancellationToken;
        private CancellationTokenSource _eventLoopCancellationTokenSource;

        public JsonRpcEndpoint(Stream transportStream, EndpointMode clientMode)
        {
            TransportStream = transportStream;
            Mode = clientMode;
            DataWriter = new StreamWriter(TransportStream);
            DataReader = new StreamReader(TransportStream);

            _eventLoopCancellationTokenSource = new CancellationTokenSource();
            _eventLoopCancellationToken = _eventLoopCancellationTokenSource.Token;

            EventLoopTask = Task.Run(ReceiveDataEventLoop, _eventLoopCancellationToken);
        }

        public async Task SendRequest(Request request)
        {
            await _transportLock.WaitAsync();
            await DataWriter.WriteLineAsync(request.ToString());
            await DataWriter.FlushAsync();
            _transportLock.Release();
        }

        public async Task SendRequest(IEnumerable<Request> requests)
        {
            await _transportLock.WaitAsync();
            await DataWriter.WriteLineAsync(JsonConvert.SerializeObject(requests));
            await DataWriter.FlushAsync();
            _transportLock.Release();
        }

        public async Task ReceiveDataEventLoop()
        {
            while (Listening)
            {
                if (_eventLoopCancellationToken.IsCancellationRequested) break;
                try
                {
                    var dataJson = await DataReader.ReadLineAsync();

                    if (dataJson != null)
                    {
                        try
                        {
                            var dataObject = JToken.Parse(dataJson);
                            if (Mode == EndpointMode.Server)
                            {
                                var requests = new List<Request>();
                                if (dataObject is JArray)
                                {
                                    requests.AddRange(dataObject.ToObject<Request[]>());
                                }
                                else if (dataObject is JObject)
                                {
                                    requests.Add(dataObject.ToObject<Request>());
                                }
                                // Spawn new handlers
                                foreach (var request in requests)
                                {
                                    var handlerTask = Task.Run(async () => await HandleReceivedRequest(request));
                                }
                            }
                            else if (Mode == EndpointMode.Client)
                            {
                                var responses = new List<Response>();
                                if (dataObject is JArray)
                                {
                                    var responseArray = (JArray)dataObject;
                                    foreach (var responseEl in responseArray)
                                    {
                                        var successful = responseEl["error"] == null;
                                        if (successful)
                                        {
                                            responses.Add(responseEl.ToObject<ResultResponse>());
                                        }
                                        else
                                        {
                                            responses.Add(responseEl.ToObject<ErrorResponse>());
                                        }
                                    }
                                }
                                else if (dataObject is JObject)
                                {
                                    var successful = dataObject["error"] == null;
                                    if (successful)
                                    {
                                        responses.Add(dataObject.ToObject<ResultResponse>());
                                    }
                                    else
                                    {
                                        responses.Add(dataObject.ToObject<ErrorResponse>());
                                    }
                                }

                                foreach (var response in responses)
                                {
                                    // Spawn new handler
                                    var handlerTask = Task.Run(() => HandleReceivedResponse(response));
                                }
                            }
                        }
                        catch (JsonSerializationException)
                        {
                            // Invalid data
                        }
                    }
                }
                catch (IOException)
                {
                    // Socket closing
                    break;
                }
                catch (ObjectDisposedException)
                {
                    // Connection closed, most likely while listening
                    break;
                }
            }
        }

        private async Task HandleReceivedResponse(Response response)
        {
            ResponseReceived?.Invoke(this, response);
            foreach (var handler in ResponsePipeline.GetHandlers())
            {
                var result = await handler.Invoke(response);
                if (result)
                {
                    break;
                }
            }
        }

        private async Task HandleReceivedRequest(Request request)
        {
            RequestReceived?.Invoke(this, request);
            Response response = null;
            foreach (var handler in RequestPipeline.GetHandlers())
            {
                var resp = await handler.Invoke(request);
                if (resp != null)
                {
                    response = resp;
                    break;
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
            Listening = false;
            _eventLoopCancellationTokenSource.Cancel(false);
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

        protected Task EventLoopTask { get; private set; }

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