using Extrasolar.Tests.Types;
using System;

namespace Extrasolar.Tests.Rpc
{
    public class TestService : ITestService
    {
        public string GetBasicString() => nameof(GetBasicString);

        public string NumString(int a, int b, string c)
        {
            return a + b + c;
        }

        public string GetBasicStringResult => nameof(GetBasicString);

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