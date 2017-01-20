namespace Extrasolar.Rpc.Proxying
{
    internal abstract class DynamicMethodBinder
    {
        public const string InvokeMethodName = nameof(InvokeMethod);
        protected abstract object[] InvokeMethod(string metadata, params object[] parameters);
    }
}