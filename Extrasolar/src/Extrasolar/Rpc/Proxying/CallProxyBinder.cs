using System;

namespace Extrasolar.Rpc.Proxying
{
    public class CallProxyBinder<TInterface> : IMethodBinder where TInterface : class
    {
        public object[] InvokeMethod(string metadata, params object[] parameters)
        {
            throw new NotImplementedException();
        }
    }
}