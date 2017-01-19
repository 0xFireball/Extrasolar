namespace Extrasolar.Rpc.Proxying
{
    public class ProxyGenerator
    {
        internal static TInterface BuildInstance<TInterface>() where TInterface : class
        {
            return ProxyFactory.CreateEmptyProxy<TInterface>(typeof(TInterface));
        }
    }
}