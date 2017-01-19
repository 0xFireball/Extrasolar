using Extrasolar.JsonRpc.Types;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Extrasolar.JsonRpc
{
    public class JsonRpcClient
    {
        public Stream TransportStream { get; set; }
        public StreamWriter DataWriter { get; private set; }
        public StreamReader DataReader { get; private set; }

        public bool Listening { get; set; } = true;

        [Flags]
        public enum ClientMode
        {
            Request,
            Response
        }

        public ClientMode Mode { get; }

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
            var requestJson = await DataReader.ReadLineAsync();
            var request = JsonConvert.DeserializeObject<Request>(requestJson);
            // Spawn new handler
            var handlerTask = Task.Factory.StartNew(async () => await HandleRequest(request));
        }

        private async Task HandleRequest(Request request)
        {
            var response = RequestHandler?.Invoke(request);
            await DataWriter.WriteAsync(response.ToString());
            await DataWriter.FlushAsync();
        }

        public Func<Request, Response> RequestHandler { get; set; }
    }
}