
# Extrasolar

Framework for easy RPC (Remote Process Communication)!
- Supports .NET Standard 1.6!


[![Build Status](https://travis-ci.org/0xFireball/Extrasolar.svg?branch=master)](https://travis-ci.org/0xFireball/Extrasolar)

Extrasolar abstracts all the underlying details, and lets you
design your application as if the remote calls you make were local
.NET methods! See the **API sample** below for code samples and a detailed
explanation of how it works.

Extrasolar uses a simple and platform agnostic protocol
based on JSON-RPC 2.0.

## Features

- Extrasolar protocol implementation
  - A protocol based on JSON-RPC 2.0
- Fluent .NET driver for Extrasolar RPC
  - Dynamic object proxying and method dispatching
  - Simulates an interface instance
  - Transparent RPC relay

## .NET API Sample

### Server

This code will start a RPC server on TCP for a custom interface `IHelloService` implemented by the `HelloService` class.

```csharp
TcpListener listener = new TcpListener(IPAddress.Loopback, Program.lbPort);
listener.Start();
_barrier.SignalAndWait();
var srvSock = await listener.AcceptTcpClientAsync();
var service = new RpcService<IHelloService>(
    new NetworkRpcService(new TcpTransportLayer(srvSock)
));
service.Export(new HelloService());
```

### Client

This code will connect to the RPC server over TCP, and call a remote method through a dynamically generated proxy. Extrasolar takes care of proxy generation, and you can
write your code in a way that abstracts the RPC calls.

```csharp
var clientSock = new TcpClient();
await clientSock.ConnectAsync(IPAddress.Loopback, Program.lbPort);
var caller = new RpcCaller<IHelloService>(
    new NetworkRpcClient(new TcpTransportLayer(clientSock)
));
var client = caller.CreateClient();
var result = client.SayHello();
```

### Details

With Extrasolar, adding RPC to an application is as simple as referencing a shared
interface type between the client and the server (use a common types library) and
registering a client and server as shown before.

Once you've done that, you can design your application as if the server
implementation was part of your application, thanks to the awesome dynamic proxy!

#### How it works

When you call `CreateClient`, Extrasolar uses `System.Reflection.Emit` to generate
an in-memory type that implements your interface and interprets and proxies
all calls to the RPC transport layer (abstracted from your view, so you never have to deal with it).

On the server, when you call `Export`, Extrasolar stores your interface implementation
internally, and dynamically dispatches methods from it, parsing the RPC arguments and return
values. It then returns the value in a JSON-RPC response to the client, to whom it looks just
like a normal method call.

## Why Extrasolar?

- A platform agnostic protocol means that you can call
  code running anywhere written in any language
  - Arguments and method calls can be sent
    over the network or another transport layer
- Run a service that can accept method calls
  and handle them in-process to save resources, then
  send them back
  - In terms of goals, JSON-RPC is very similar to REST
- If you're building your own application, Extrasolar will
  fit better connecting your client application to a server application
  than using another protocol such as REST over HTTP, as the library abstracts
  all of the remote calling logic, leaving you with interfaces that you can
  use just as you would local types

### Use cases

- A client application with a server that provides an API
  - Remote data processing
- A resource-intensive process that hosts a service, making it
  available to other processes on the local machine


## Credits

- Extrasolar's API was inspired by that of [ServiceWire](https://github.com/tylerjensen/ServiceWire)
  - While ServiceWire aims to accomplish a similar goal of providing RPC,
    Extrasolar's protocol is completely different and written from scratch
  - Extrasolar uses a heavily modified and adapted version of ServiceWire's object proxying
    as part of its fluent proxying API in addition to several other components originally written
    from scratch in Extrasolar
  - Extrasolar uses a more general protocol that can run over any two-way stream system,
    and is not limited to just TCP or pipes but also supports standard I/O and in-memory
    streams, and any other two-way stream.
  - ServiceWire is licensed under the Apache License 2.0

## License

Copyright &copy; 2017 0xFireball (Nihal Talur)

Licensed under the Apache License 2.0.
