using Extrasolar.JsonRpc;
using Extrasolar.JsonRpc.Types;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Extrasolar.Demo.MemIo
{
    public class Program
    {
        //private static Stream _basicIoStream = Console.OpenStandardOutput();
        private static Barrier _ioClientsReady = new Barrier(2);

        public static void Main()
        {
            // Test basic MemIO JSON RPC
            Task.Factory.StartNew(BasicMemIoClient1);
            Task.Factory.StartNew(BasicMemIoClient2);
            Task.Delay(-1).GetAwaiter().GetResult();
        }

        private static async Task BasicMemIoClient2()
        {
            var rpcClient = new JsonRpcClient(Console.OpenStandardOutput(), JsonRpcClient.ClientMode.Request);
            _ioClientsReady.SignalAndWait();
            await rpcClient.SendRequest(new Request("ping", null, "0"));
        }

        private static async Task BasicMemIoClient1()
        {
            var rpcClient = new JsonRpcClient(Console.OpenStandardInput(), JsonRpcClient.ClientMode.Response);
            rpcClient.AddRequestHandler((req) =>
            {
                if (!req.IsNotification)
                {
                    Console.WriteLine($"Client called method ${req.Method}.");
                    return new ResultResponse(req, null);
                }
                return null;
            });
            _ioClientsReady.SignalAndWait();
        }
    }
}