using Extrasolar.IO;
using Extrasolar.JsonRpc.Types;
using Extrasolar.Rpc;
using Newtonsoft.Json;
using System.Reflection;
using System.Threading.Tasks;

namespace Extrasolar.Rpc
{
    public class RpcCaller<TInterface> where TInterface : class
    {
        public NetworkRpcClient RpcClient { get; set; }

        private int _requestCount;

        public RpcCaller(NetworkRpcClient netRpcClient)
        {
            RpcClient = netRpcClient;
            var methods = typeof(TInterface).GetTypeInfo().GetMethods();
        }

        public TInterface CreateClient()
        {
            var client = CallProxy;
            return default(TInterface);
        }

        public async Task<Response> CallByNameAsync(string methodName, params object[] args)
        {
            var jArgs = JsonConvert.SerializeObject(args);
            var response = await RpcClient.Request(new Request(methodName, jArgs, _requestCount.ToString()));
            ++_requestCount;
            return response;
        }

        public async Task<TResult> CallByNameAsync<TResult>(string methodName, params object[] args)
        {
            var response = await CallByNameAsync(methodName, args);
            var result = response.Result.ToObject<TResult>();
            return result;
        }
    }
}