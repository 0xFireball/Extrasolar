using Extrasolar.IO;
using Extrasolar.IO.Transport;
using Extrasolar.Rpc;
using System;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace Extrasolar.Demo.Loopback
{
    public class RpcCallerDemo
    {
        private Barrier _barrier = new Barrier(2);

        internal async Task RunServerAsync(TcpClient serverSock)
        {
            var service = new RpcService<IHelloService>(
                new NetworkRpcService(new TcpTransportLayer(serverSock)
            ));
            service.Export(new HelloService());
            Console.WriteLine($"{nameof(HelloService)} ready.");
            await Task.Delay(0);
            _barrier.SignalAndWait();
        }

        internal async Task RunClientAsync(TcpClient clientSock)
        {
            var caller = new RpcCaller<IHelloService>(
                new NetworkRpcClient(new TcpTransportLayer(clientSock)
            ));
            //var result = await caller.CallByNameAsync<string>(nameof(IHelloServer.SayHello));
            _barrier.SignalAndWait();
            var client = caller.CreateClient();
            var result = client.SayHello();
            Console.WriteLine("Sent command from client.");
            await Task.Delay(0);
        }
    }
}