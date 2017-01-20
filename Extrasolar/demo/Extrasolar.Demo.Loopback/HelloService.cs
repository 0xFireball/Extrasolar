using Extrasolar.Demo.Loopback.Types;
using System;

namespace Extrasolar.Demo.Loopback
{
    public class HelloService : IHelloService
    {
        public int Add(int a, int b)
        {
            return a + b;
        }

        public bool IsSkyBlue()
        {
            return true; // actually: usually :P :D
        }

        public string Add(string a, string b)
        {
            return a + b;
        }

        public string SayHello() => "Hello, World through RPC fluent API.";

        public double GetVolume(TastyCookie cookie)
        {
            return Math.Pow(cookie.Radius, 2) * Math.PI * cookie.Thickness;
        }

        public bool Compare(TastyCookie c1, TastyCookie c2)
        {
            return GetVolume(c1) >= GetVolume(c2);
        }
    }
}