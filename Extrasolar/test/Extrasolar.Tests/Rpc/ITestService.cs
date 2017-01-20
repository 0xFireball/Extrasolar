using Extrasolar.Tests.Types;

namespace Extrasolar.Tests.Rpc
{
    public interface ITestService
    {
        string GetBasicString();
        string GetBasicStringResult { get; }
        string NumString(int a, int b, string c);
        double GetVolume(TastyCookie cookie);
        bool Compare(TastyCookie c1, TastyCookie c2);
    }
}