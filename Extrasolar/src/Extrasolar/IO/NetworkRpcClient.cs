using Extrasolar.IO.Transport;
using Extrasolar.JsonRpc;

namespace Extrasolar.IO
{
    public class NetworkRpcClient : NetworkRpcEndpoint
    {
        public NetworkRpcClient(ITransportLayer transport) : base(transport, JsonRpcEndpoint.EndpointMode.Client)
        {
        }
    }
}