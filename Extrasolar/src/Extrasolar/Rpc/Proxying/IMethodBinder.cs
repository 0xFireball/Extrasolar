namespace Extrasolar.Rpc.Proxying
{
    public interface IMethodBinder
    {
        bool InvokeMethod(string metadata, out object result, params object[] args);
    }
}