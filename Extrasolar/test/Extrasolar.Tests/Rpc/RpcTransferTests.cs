using Xunit;

namespace Extrasolar.Tests.Rpc
{
    public class RpcTransferTests : IClassFixture<TestClientFixture>
    {
        private TestClientFixture _fixture;

        public RpcTransferTests()
        {
            _fixture = new TestClientFixture();
        }

        [Fact]
        public void CanCallParameterless()
        {
            Assert.Equal(_fixture.Client.GetBasicString(), _fixture.Client.GetBasicStringResult);
        }
    }
}