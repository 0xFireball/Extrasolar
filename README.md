
# Extrasolar

Framework for easy RPC (Remote Process Communication)

Extrasolar uses a simple and platform agnostic protocol
based on JSON-RPC 2.0.

## Features

- Extrasolar protocol implementation
  - A protocol based on JSON-RPC 2.0
- Fluent .NET driver for Extrasolar RPC
  - Dynamic object proxying and method dispatching
  - Simulates an interface instance

## Why Extrasolar?

- A platform agnostic protocol means that you can call
  code running anywhere written in any language
  - Arguments and method calls can be sent
    over the network or another transport layer
- Run a service that can accept method calls
  and handle them in-process to save resources, then
  send them back
  - In terms of goals, JSON-RPC is very similar to REST


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