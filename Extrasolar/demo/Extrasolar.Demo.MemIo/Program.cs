using System;
using System.IO;
using System.Threading.Tasks;

namespace Extrasolar.Demo.MemIo
{
    public class Program
    {
        private static MemoryStream _basicIoStream = new MemoryStream();
        private static StreamWriter _basicIoWriter = new StreamWriter(_basicIoStream);
        private static StreamReader _basicIoReader = new StreamReader(_basicIoStream);

        public static void Main(string[] args)
        {
            // Test basic MemIO JSON RPC
            Task.Factory.StartNew(BasicMemIoClient1);
            Task.Factory.StartNew(BasicMemIoClient2);
        }

        private static void BasicMemIoClient2()
        {
            var rpcClient = new JsonRpcClient();
        }

        private static void BasicMemIoClient1()
        {
            throw new NotImplementedException();
        }
    }
}