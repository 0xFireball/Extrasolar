using Extrasolar.Demo.Loopback.Types;

namespace Extrasolar.Demo.Loopback
{
    public interface IHelloService
    {
        string SayHello();
        bool IsSkyBlue();
        int Add(int a, int b);
        string Add(string a, string b);
        double GetVolume(TastyCookie cookie);
        bool Compare(TastyCookie c1, TastyCookie c2);
    }
}