namespace Extrasolar.Rpc
{
    public class ProxyFactory
    {
        internal static TInterface BuildInstance<TInterface>()
        {
            return default(TInterface);
        }
    }
}