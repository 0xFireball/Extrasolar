using Extrasolar.IO;

namespace Extrasolar.Rpc
{
    public class RpcService<TInterface> where TInterface : class
    {
        public NetworkRpcEndpoint RpcClient { get; set; }

        public RpcService(NetworkRpcService netRpcClient)
        {
            RpcClient = netRpcClient;
        }
    }
}