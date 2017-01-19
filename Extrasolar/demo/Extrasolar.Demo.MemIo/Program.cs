using Extrasolar.JsonRpc;
using Extrasolar.JsonRpc.Types;
using System.IO;
using System.Threading.Tasks;

namespace Extrasolar.Demo.MemIo
{
    public class Program
    {
        private static MemoryStream _basicIoStream = new MemoryStream();

        public static void Main()
        {
            // Test basic MemIO JSON RPC
            Task.Factory.StartNew(BasicMemIoClient1);
            Task.Factory.StartNew(BasicMemIoClient2);
        }

        private static async Task BasicMemIoClient2()
        {
            var rpcClient = new JsonRpcClient(_basicIoStream, JsonRpcClient.ClientMode.TwoWay);
            await rpcClient.SendRequest(new Request("ping", null, "0"));
        }

        private static async Task BasicMemIoClient1()
        {
            var rpcClient = new JsonRpcClient(_basicIoStream, JsonRpcClient.ClientMode.TwoWay);
            rpcClient.AddRequestHandler((req) =>
            {
                return new ResultResponse(req, null);
            });
        }
    }
}