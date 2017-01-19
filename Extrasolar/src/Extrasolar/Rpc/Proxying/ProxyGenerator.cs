namespace Extrasolar.Rpc.Proxying
{
    public class ProxyGenerator
    {
        internal static TInterface BuildEmpty<TInterface>() where TInterface : class
        {
            return ProxyFactory.CreateEmptyProxy<TInterface>();
        }
    }
}