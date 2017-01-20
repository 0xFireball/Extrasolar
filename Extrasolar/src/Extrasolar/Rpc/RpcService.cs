using Extrasolar.IO;

namespace Extrasolar.Rpc
{
    public class RpcService<TInterface>
    {
        private NetworkRpcEndpoint networkRpcEndpoint;

        public RpcService(NetworkRpcEndpoint networkRpcEndpoint)
        {
            this.networkRpcEndpoint = networkRpcEndpoint;
        }
    }
}