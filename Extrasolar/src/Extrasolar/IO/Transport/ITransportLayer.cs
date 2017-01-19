using System.IO;

namespace Extrasolar.IO.Transport
{
    public interface ITransportLayer
    {
        Stream GetStream();
    }
}