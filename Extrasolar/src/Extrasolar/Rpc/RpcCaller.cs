using Extrasolar.IO;
using System.Reflection;
using System.Threading.Tasks;

namespace Extrasolar.Rpc
{
    public class RpcCaller<TInterface>
    {
        public NetworkRpcClient RpcClient { get; set; }

        public RpcCaller(NetworkRpcClient netRpcClient)
        {
            RpcClient = netRpcClient;
            var methods = typeof(TInterface).GetTypeInfo().GetMethods();
        }

        public Task CallByNameAsync(string methodName)
        {
            return null;
        }
    }
}