using Extrasolar.Tests.Types;
using System;
using System.Linq;

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

        public int ArraySum(int[] vals)
        {
            return vals.Sum();
        }

        public int[] CombineArrays(int[] a1, int[] a2)
        {
            return a1.Concat(a2).ToArray();
        }
    }
}