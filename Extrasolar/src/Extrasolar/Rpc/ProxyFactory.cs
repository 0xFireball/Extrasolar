namespace Extrasolar.Rpc
{
    public class ProxyFactory
    {
        internal static TInterface Build<TInterface, TTarget>(CallProxy<TTarget> callProxy)
            where TInterface : class
            where TTarget : new()
        {
            return default(TInterface);
        }
    }
}