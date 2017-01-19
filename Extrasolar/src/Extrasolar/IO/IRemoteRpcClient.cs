using Extrasolar.JsonRpc;
using Extrasolar.JsonRpc.Types;
using System.Threading.Tasks;

namespace Extrasolar.IO
{
    public interface IRemoteRpcClient
    {
        JsonRpcClient.ClientMode Mode { get; set; }
        JsonRpcClient RpcLayer { get; }

        Task<Response> Request(Request request);
    }
}