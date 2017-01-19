using System;

namespace Extrasolar.Rpc.Proxying
{
    public class CallProxyBinder<TInterface> : IMethodBinder where TInterface : class
    {
        private RpcCaller<TInterface> _caller;

        public CallProxyBinder(RpcCaller<TInterface> caller)
        {
            _caller = caller;
        }

        public object[] InvokeMethod(string metadata, params object[] parameters)
        {
            throw new NotImplementedException();
        }
    }
}