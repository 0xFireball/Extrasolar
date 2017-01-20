using Extrasolar.JsonRpc;
using Extrasolar.JsonRpc.Types;
using System.Threading.Tasks;

namespace Extrasolar.IO
{
    public interface IRemoteRpcEndpoint
    {
        JsonRpcEndpoint.EndpointMode Mode { get; set; }
        JsonRpcEndpoint RpcLayer { get; }

        Task<Response> Request(Request request);
    }
}