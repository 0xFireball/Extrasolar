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

        public string SayHello() => "Hello, World through RPC fluent API.";
    }
}