using Extrasolar.IO;

namespace Extrasolar.Rpc
{
    public class RpcService<TInterface> where TInterface : class
    {
        public NetworkRpcEndpoint RpcClient { get; set; }

        private int _requestCount;

        public RpcService(NetworkRpcService netRpcClient)
        {
            RpcClient = netRpcClient;
        }
    }
}