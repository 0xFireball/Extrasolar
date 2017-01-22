using System.Dynamic;

namespace Extrasolar.Rpc.Proxying
{
    internal class CallProxy<TInterface> : DynamicObject where TInterface : class
    {
        private TInterface _proxiedObject;
        private readonly RpcCaller<TInterface> _remoteCaller;

        public CallProxy(TInterface target, RpcCaller<TInterface> caller)
        {
            _remoteCaller = caller;
            _proxiedObject = target;
        }

        public static TInterface CreateEmpty(RpcCaller<TInterface> caller)
        {
            var binder = new CallProxyBinder<TInterface>(caller);
            var dynamicCallProxy = ProxyGenerator.BuildEmpty<TInterface>(binder);
            return dynamicCallProxy;
        }
    }
}