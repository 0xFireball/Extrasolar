namespace Extrasolar.Rpc.Proxying
{
    public abstract class DynamicMethodBinder
    {
        public const string InvokeMethodName = nameof(InvokeMethod);
        public abstract object Target { get; }
        public abstract object[] InvokeMethod(string metadata, params object[] parameters);
    }
}