namespace Extrasolar.Rpc.Proxying
{
    internal abstract class MethodBinder
    {
        public abstract object[] InvokeMethod(string metadata, params object[] parameters);
    }
}