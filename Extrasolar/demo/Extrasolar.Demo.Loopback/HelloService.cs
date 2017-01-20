using System;

namespace Extrasolar.Demo.Loopback
{
    public class HelloService : IHelloService
    {
        public string SayHello() => "Hello, World through RPC fluent API.";
    }
}