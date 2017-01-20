namespace Extrasolar.Tests.Rpc
{
    public class TestService : ITestService
    {
        public string GetBasicString() => nameof(GetBasicString);

        public string GetBasicStringResult => nameof(GetBasicString);
    }
}