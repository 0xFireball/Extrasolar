using Extrasolar.IO.Transport;
using Extrasolar.JsonRpc;

namespace Extrasolar.IO
{
    public class NetworkRpcService : NetworkRpcEndpoint
    {
        public NetworkRpcService(ITransportLayer transport) : base(transport, JsonRpcEndpoint.EndpointMode.Server)
        {
        }
    }
}