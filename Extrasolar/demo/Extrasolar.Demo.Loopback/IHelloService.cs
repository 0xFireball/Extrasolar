using Extrasolar.Demo.Loopback.Types;

namespace Extrasolar.Demo.Loopback
{
    public interface IHelloService
    {
        string SayHello();
        bool IsSkyBlue();
        int Add(int a, int b);
        string Add(string a, string b);
        double GetVolume(Cookie cookie);
        bool Compare(Cookie c1, Cookie c2);
    }
}