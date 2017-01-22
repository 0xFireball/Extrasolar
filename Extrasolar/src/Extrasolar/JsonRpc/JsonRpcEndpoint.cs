using Extrasolar.JsonRpc.Types;
using Extrasolar.Types;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Extrasolar.JsonRpc
{
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
            EventLoopTask = Task.Factory.StartNew(ReceiveDataEventLoop, _eventLoopCancellationToken);
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
                if (_eventLoopCancellationToken.IsCancellationRequested) break;
                try
                {
                    var dataJson = await DataReader.ReadLineAsync();

                    if (dataJson != null)
                    {
                        try
                        {
                            var dataObject = JToken.Parse(dataJson);
                            // Ignore requests if they do not match the role
                            bool isRequest = dataObject["method"] != null;
                            if (Mode == EndpointMode.Server && isRequest)
                            {
                                var requestJObj = JToken.Parse(dataJson);
                                // Spawn new handler
                                if (requestJObj is JArray) // Support request array
                                {
                                    var requestArray = requestJObj.ToObject<Request[]>();
                                    foreach (var request in requestArray)
                                    {
                                        var handlerTask = Task.Factory.StartNew(async () => await HandleReceivedRequest(request));
                                    }
                                }
                                else if (requestJObj is JObject)
                                {
                                    var handlerTask = Task.Factory.StartNew(async () => await HandleReceivedRequest(requestJObj.ToObject<Request>()));
                                }
                            }
                            else if (Mode == EndpointMode.Client && !isRequest)
                            {
                                Response response;
                                var successful = dataObject["error"] == null;
                                if (successful)
                                {
                                    response = dataObject.ToObject<ResultResponse>();
                                }
                                else
                                {
                                    response = dataObject.ToObject<ErrorResponse>();
                                }
                                // Spawn new handler
                                var handlerTask = Task.Factory.StartNew(() => HandleReceivedResponse(response));
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

        protected Task<Task> EventLoopTask { get; private set; }

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