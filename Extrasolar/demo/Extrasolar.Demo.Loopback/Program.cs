using Extrasolar.IO;
using Extrasolar.JsonRpc;
using Extrasolar.JsonRpc.Types;
using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace Extrasolar.Demo.Loopback
{
    public class Program
    {
        private static Barrier _ioClientsReady = new Barrier(2);
        private static int lbPort = 28754;

        public static void Main()
        {
            // Test basic MemIO JSON RPC
            Task.Factory.StartNew(NetServerThread);
            Task.Factory.StartNew(NetClientThread);
            Task.Delay(-1).GetAwaiter().GetResult();
        }

        private static async Task NetClientThread()
        {
            _ioClientsReady.SignalAndWait();
            var client = new TcpClient();
            await client.ConnectAsync(IPAddress.Loopback, lbPort);
            var rpcClient = new NetworkRpcClient(new TcpTransportLayer(client), JsonRpcClient.ClientMode.Request);
            _ioClientsReady.SignalAndWait();
            var reqTask = rpcClient.Request(new Request("ping", null, "0"));
            var response = await reqTask;
            Console.WriteLine($"Server responded: {response}");
        }

        private static async Task NetServerThread()
        {
            TcpListener listener = new TcpListener(IPAddress.Loopback, lbPort);
            listener.Start();
            _ioClientsReady.SignalAndWait();
            var client = await listener.AcceptTcpClientAsync();
            var rpcClient = new NetworkRpcClient(new TcpTransportLayer(client), JsonRpcClient.ClientMode.Response);
            rpcClient.RpcLayer.RequestPipeline.AddItemToStart((req) =>
            {
                if (!req.IsNotification)
                {
                    Console.WriteLine($"Client called method {req.Method}.");
                    return new ResultResponse(req, "pong");
                }
                return null;
            });
            _ioClientsReady.SignalAndWait();
        }
    }
}