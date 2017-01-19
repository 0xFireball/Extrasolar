namespace Extrasolar.Rpc.Proxying
{
    public class ProxyGenerator
    {
        internal static TInterface BuildEmpty<TInterface>(IMethodBinder binder) where TInterface : class
        {
            return ProxyFactory.CreateEmptyProxy<TInterface>(binder);
        }
    }
}