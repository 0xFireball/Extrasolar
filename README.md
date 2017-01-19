
# Extrasolar

Framework for easy RPC (Remote Process Communication)

Extrasolar uses a simple and platform agnostic protocol
based on JSON-RPC 2.0.

## Why Extrasolar?

- A platform agnostic protocol means that you can call
  code running anywhere written in any language
  - Arguments and method calls can be sent
    over the network or another transport layer
- Run a service that can accept method calls
  and handle them in-process to save resources, then
  send them back
  - In terms of goals, JSON-RPC is very similar to REST