using System.Linq;
using System.Reflection;

namespace Extrasolar.Rpc.Proxying
{
    public class ProxyGenerator
    {
        internal static TInterface BuildEmpty<TInterface>(DynamicMethodBinder binder) where TInterface : class
        {
            var paramType = binder.GetType().GetTypeInfo().GetConstructors().First().GetParameters().First().ParameterType;
            return ProxyFactory.CreateEmptyProxy<TInterface>(binder, binder.GetType(), paramType, binder.Target);
        }
    }
}