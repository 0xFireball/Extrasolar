
# Extrasolar RPC protocol specification

## Based on JSON-RPC 2.0

The Extrasolar RPC protocol is based on the JSON-RPC 2.0
protocol, the full specification for which can be found
here: <http://www.jsonrpc.org/specification>

This document will list differences to the JSON-RPC 2.0
protocol.

---

## 5. Response object

## 5 Request handling

Changes

- A request that results in a parse error will not receive a response. The
  protocol is designed to be used over a reliable mode of transport such as TCP,
  so appropriate validation should be performed before sending the request

## 5.1 Error object

Changes

- Server error is `-32000`, instead of `-32000` to `-32099` as in JSON-RPC 2.0



## 6. Batch

Changes

- Batch requests are neither supported nor implemented
  in the Extrasolar client or server.