using Extrasolar.Demo.Loopback.Types;
using Extrasolar.IO;
using Extrasolar.IO.Transport;
using Extrasolar.Rpc;
using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace Extrasolar.Demo.Loopback
{
    public class RpcCallerDemo
    {
        private Barrier _barrier = new Barrier(2);

        internal async Task RunServerAsync()
        {
            TcpListener listener = new TcpListener(IPAddress.Loopback, Program.lbPort);
            listener.Start();
            _barrier.SignalAndWait();
            var srvSock = await listener.AcceptTcpClientAsync();
            var service = new RpcService<IHelloService>(
                new NetworkRpcService(new TcpTransportLayer(srvSock)
            ));
            service.Export(new HelloService());
            Console.WriteLine($"{nameof(HelloService)} ready.");
            await Task.Delay(0);
            _barrier.SignalAndWait();
        }

        internal async Task RunClientAsync()
        {
            var clientSock = new TcpClient();
            _barrier.SignalAndWait();
            await clientSock.ConnectAsync(IPAddress.Loopback, Program.lbPort);
            var caller = new RpcCaller<IHelloService>(
                new NetworkRpcClient(new TcpTransportLayer(clientSock)
            ));
            //var result = await caller.CallByNameAsync<string>(nameof(IHelloServer.SayHello));
            _barrier.SignalAndWait();
            var client = caller.CreateClient();
            var helloResult = client.SayHello();
            Console.WriteLine($"Sent command from client. Server responded {helloResult}");
            Console.WriteLine($"Is the sky blue?: {client.IsSkyBlue()}");
            Console.WriteLine($"1 + 3 is: {client.Add(1, 3)}");
            var c1 = new TastyCookie(TastyCookie.CookieFlavor.Chocolate, 6, 1);
            var c2 = new TastyCookie(TastyCookie.CookieFlavor.Chocolate, 8, 1.2);
            Console.WriteLine($"Volume of TastyCookie 1 is: {client.GetVolume(c1)}");
            Console.WriteLine($"Cookie {(client.Compare(c1, c2) ? "1" : "2")} is larger.");
            await Task.Delay(0);
        }
    }
}