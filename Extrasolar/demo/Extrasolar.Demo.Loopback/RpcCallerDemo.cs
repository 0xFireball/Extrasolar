using Extrasolar.IO;
using Extrasolar.IO.Transport;
using Extrasolar.Rpc;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace Extrasolar.Demo.Loopback
{
    public class RpcCallerDemo
    {
        private static TcpClient _clientSock;

        internal void Run(TcpClient clientSock)
        {
            _clientSock = clientSock;
            RunAsync().GetAwaiter().GetResult();
        }

        private async Task RunAsync()
        {
            var caller = new RpcCaller<IHelloServer>(
                new NetworkRpcClient(new TcpTransportLayer(_clientSock),
                JsonRpc.JsonRpcClient.ClientMode.Request
            ));
            var result = await caller.CallByNameAsync<string>(nameof(IHelloServer.SayHello));
        }
    }
}