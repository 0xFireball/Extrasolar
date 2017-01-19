using System.Dynamic;
using System.Reflection;

namespace Extrasolar.Rpc.Proxying
{
    internal class CallProxy<TInterface> : DynamicObject where TInterface : class
    {
        private TInterface _proxiedObject;

        public CallProxy(TInterface target)
        {
            _proxiedObject = target;
        }

        public override bool TryInvokeMember(InvokeMemberBinder binder, object[] args, out object result)
        {
            try
            {
                // TODO: Processing

                // Forward call to proxied object
                result = _proxiedObject.GetType().GetTypeInfo().GetMethod(binder.Name).Invoke(_proxiedObject, args);
                return true;
            }
            catch
            {
                result = null;
                return false;
            }
        }

        public static CallProxy<TInterface> CreateEmpty()
        {
            var emptyTarget = InstanceGenerator.BuildInstance<TInterface>();
            return new CallProxy<TInterface>(emptyTarget);
        }
    }
}