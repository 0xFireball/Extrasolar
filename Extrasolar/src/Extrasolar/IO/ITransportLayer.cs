using System.IO;

namespace Extrasolar.IO
{
    public interface ITransportLayer
    {
        Stream GetStream();
    }
}