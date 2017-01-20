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
            
            await Task.Delay(0);
        }

        internal async Task RunClientAsync(TcpClient clientSock)
        {
            var caller = new RpcCaller<IHelloService>(
                new NetworkRpcClient(new TcpTransportLayer(clientSock),
                JsonRpc.JsonRpcClient.ClientMode.Request
            ));
            //var result = await caller.CallByNameAsync<string>(nameof(IHelloServer.SayHello));
            var client = caller.CreateClient();
            var result = client.SayHello();
            Console.WriteLine("Sent command from client.");
            await Task.Delay(0);
        }
    }
}