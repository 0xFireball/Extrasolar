using Extrasolar.IO;
using Extrasolar.IO.Transport;
using Extrasolar.JsonRpc;
using Extrasolar.JsonRpc.Types;
using Extrasolar.Rpc;
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
        private static TcpClient _clientSock;
        private static TcpClient _serverSock;

        public static void Main()
        {
            // Test basic MemIO JSON RPC
            var server = Task.Factory.StartNew(NetServerThread);
            var client = Task.Factory.StartNew(NetClientThread);
            // Wait for first demo
            client.GetAwaiter().GetResult().GetAwaiter().GetResult();
            // Test RPCCaller
            var rpcCallerDemo = new RpcCallerDemo();
            rpcCallerDemo.RunClientAsync(_clientSock).GetAwaiter().GetResult();
            Task.Delay(-1).GetAwaiter().GetResult();
        }

        private static async Task NetClientThread()
        {
            _ioClientsReady.SignalAndWait();
            var client = new TcpClient();
            _clientSock = client;
            await client.ConnectAsync(IPAddress.Loopback, lbPort);
            var rpcClient = new NetworkRpcEndpoint(new TcpTransportLayer(client), JsonRpcEndpoint.EndpointMode.Client);
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
            _serverSock = client;
            var rpcClient = new NetworkRpcEndpoint(new TcpTransportLayer(client), JsonRpcEndpoint.EndpointMode.Server);
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