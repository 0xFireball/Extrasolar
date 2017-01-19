using System;
using System.Dynamic;
using System.Reflection;

namespace Extrasolar.Rpc.Proxying
{
    internal class CallProxy<TInterface> : DynamicObject where TInterface : class
    {
        private TInterface _proxiedObject;
        private readonly RpcCaller<TInterface> _remoteCaller;

        public CallProxy(TInterface target, RpcCaller<TInterface> caller)
        {
            this._remoteCaller = caller;
            _proxiedObject = target;
        }

        /// <summary>
        /// Delete this.
        /// </summary>
        /// <param name="binder"></param>
        /// <param name="args"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        public override bool TryInvokeMember(InvokeMemberBinder binder, object[] args, out object result)
        {
            try
            {
                // TODO: Processing

                result = _remoteCaller.CallByNameAsync(binder.Name, args).GetAwaiter().GetResult();
                return true;

                //// Forward call to proxied object
                //result = _proxiedObject.GetType().GetTypeInfo().GetMethod(binder.Name).Invoke(_proxiedObject, args);
                //return true;
            }
            catch
            {
                result = null;
                return false;
            }
        }

        public static TInterface CreateEmpty(RpcCaller<TInterface> caller)
        {
            var binder = new CallProxyBinder<TInterface>(caller);
            var emptyTarget = ProxyGenerator.BuildEmpty<TInterface>(binder);
            return emptyTarget;
        }
    }
}