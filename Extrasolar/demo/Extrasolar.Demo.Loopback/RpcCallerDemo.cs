using Extrasolar.Rpc;
using System.Threading.Tasks;

namespace Extrasolar.Demo.Loopback
{
    public class RpcCallerDemo
    {
        internal void Run()
        {
            RunAsync().GetAwaiter().GetResult();
        }

        private async Task RunAsync()
        {
            var caller = new RpcCaller<IHelloServer>();
            
        }
    }
}