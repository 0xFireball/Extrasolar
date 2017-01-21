using Extrasolar.Tests.Types;
using System;
using System.Linq;
using Xunit;

namespace Extrasolar.Tests.Rpc
{
    public class RpcTransferTests : IClassFixture<TestClientFixture>
    {
        private readonly TestClientFixture _fixture;

        public RpcTransferTests()
        {
            _fixture = new TestClientFixture();
        }

        [Fact]
        public void CanCallParameterless()
        {
            Assert.Equal(_fixture.Client.GetBasicString(), _fixture.Client.GetBasicStringResult);
        }

        [Fact]
        public void CanCallSimpleParameters()
        {
            Assert.Equal(_fixture.Client.NumString(1, 3, "hello"), "4hello");
        }

        [Fact]
        public void CanCallSimpleObjects()
        {
            var c1 = new TastyCookie(TastyCookie.CookieFlavor.Chocolate, 6, 1);
            var vol = Math.PI * c1.Radius * c1.Radius * c1.Thickness;
            Assert.Equal(_fixture.Client.GetVolume(c1), vol);
        }

        [Fact]
        public void CanCallMultipleSimpleObjects()
        {
            var c1 = new TastyCookie(TastyCookie.CookieFlavor.Chocolate, 6, 1);
            var c2 = new TastyCookie(TastyCookie.CookieFlavor.Chocolate, 8, 1.2);
            Assert.Equal(_fixture.Client.Compare(c1, c2), false);
        }

        [Fact]
        public void CanCallArrayParameters()
        {
            var ints = new[] { 1, 2, 3, 4 };
            Assert.Equal(_fixture.Client.ArraySum(ints), ints.Sum());
        }

        [Fact]
        public void CanReceiveArrayReturn()
        {
            var a1 = new[] { 1, 2, 3, 4 };
            var a2 = new[] { 5, 6, 7, 8 };
            var combined = a1.Concat(a2);
            Assert.Equal(_fixture.Client.CombineArrays(a1, a2), combined);
        }

        [Fact]
        public void CanTransferAnonymousObjects()
        {
            var blob = new { Foo = "Bar" };
            dynamic returnBlob = _fixture.Client.EchoObject(blob);
            Assert.Equal((string)returnBlob.Foo, blob.Foo);
        }
    }
}