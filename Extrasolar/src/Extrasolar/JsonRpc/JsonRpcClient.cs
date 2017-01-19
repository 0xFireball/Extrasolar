using System.IO;

namespace Extrasolar.JsonRpc
{
    public class JsonRpcClient
    {
        public Stream TransportStream { get; set; }
        public StreamWriter DataWriter { get; private set; }
        public StreamReader DataReader { get; private set; }

        public JsonRpcClient(Stream transportStream)
        {
            TransportStream = transportStream;
            DataWriter = new StreamWriter(TransportStream);
            DataReader = new StreamReader(TransportStream);
        }
    }
}