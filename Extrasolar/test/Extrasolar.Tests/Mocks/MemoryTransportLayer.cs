using Extrasolar.IO.Transport;
using System.IO;

namespace Extrasolar.Tests.Mocks
{
    public class MemoryTransportLayer : ITransportLayer
    {
        private UnlimitedMemoryStream _strm = new UnlimitedMemoryStream();

        public Stream GetStream()
        {
            return _strm;
        }
    }
}