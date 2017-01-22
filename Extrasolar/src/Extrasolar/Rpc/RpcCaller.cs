using Extrasolar.IO;
using Extrasolar.JsonRpc.Types;
using Extrasolar.Rpc.Proxying;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;

namespace Extrasolar.Rpc
{
    public class RpcCaller<TInterface> : IDisposable where TInterface : class
    {
        public NetworkRpcEndpoint RpcClient { get; }

        private int _requestCount;

        public RpcCaller(NetworkRpcClient netRpcClient)
        {
            RpcClient = netRpcClient;
        }

        public TInterface CreateClient()
        {
            var client = CallProxy<TInterface>.CreateEmpty(this);
            return client;
        }

        internal void CallByName(string methodName, params object[] args)
        {
            var response = CallByNameAsync(methodName, args).Result;
        }

        internal object CallByName(string methodName, Type returnType, params object[] args)
        {
            var response = CallByNameAsync(methodName, args).Result;
            return response.Result.ToObject(returnType);
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

        public void Dispose()
        {
            RpcClient.Dispose();
        }
    }
}