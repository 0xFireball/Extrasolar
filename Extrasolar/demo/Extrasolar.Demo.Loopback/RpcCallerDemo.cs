using Extrasolar.IO;
using Extrasolar.IO.Transport;
using Extrasolar.Rpc;
using System;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace Extrasolar.Demo.Loopback
{
    public class RpcCallerDemo
    {
        internal async Task RunServerAsync(TcpClient serverSock)
        {
            var service = new RpcService<IHelloService>(
                new NetworkRpcEndpoint(new TcpTransportLayer(serverSock),
                JsonRpc.JsonRpcEndpoint.EndpointMode.Server
            ));
            await Task.Delay(0);
        }

        internal async Task RunClientAsync(TcpClient clientSock)
        {
            var caller = new RpcCaller<IHelloService>(
                new NetworkRpcEndpoint(new TcpTransportLayer(clientSock),
                JsonRpc.JsonRpcEndpoint.EndpointMode.Client
            ));
            //var result = await caller.CallByNameAsync<string>(nameof(IHelloServer.SayHello));
            var client = caller.CreateClient();
            var result = client.SayHello();
            Console.WriteLine("Sent command from client.");
            await Task.Delay(0);
        }
    }
}