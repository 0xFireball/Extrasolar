namespace Extrasolar.JsonRpc.Types
{
    /// <summary>
    /// Used to specify a specific error code.
    /// </summary>
    public enum JsonRpcErrorCode
    {
        // User-defined

        ApplicationDefined = 0,

        // Specification

        ParseError = -32700,
        InvalidRequest = -32600,
        MethodNotFound = -32601,
        InvalidParams = -32602,
        InternalError = -32603,
        ServerError = -32000
    }
}