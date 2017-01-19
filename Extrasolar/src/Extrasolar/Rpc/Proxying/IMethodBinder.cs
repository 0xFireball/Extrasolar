namespace Extrasolar.Rpc.Proxying
{
    internal interface IMethodBinder
    {
        object[] InvokeMethod(string metadata, params object[] parameters);
    }
}