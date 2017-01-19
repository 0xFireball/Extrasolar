using System;
using System.Dynamic;
using System.Reflection;

namespace Extrasolar.Rpc
{
    internal class CallProxy<TTarget> : DynamicObject where TTarget : new()
    {
        private readonly TTarget _wrappedObject;

        public static TInterface Create<TInterface>(TTarget obj) where TInterface : class
        {
            if (!typeof(TInterface).GetTypeInfo().IsInterface)
                throw new ArgumentException($"{nameof(TInterface)} must be an interface");

            return ProxyFactory.Build<TInterface, TTarget>(new CallProxy<TTarget>(obj));
        }

        //you can make the contructor private so you are forced to use the Wrap method.
        private CallProxy(TTarget obj)
        {
            _wrappedObject = obj;
        }

        public override bool TryInvokeMember(InvokeMemberBinder binder, object[] args, out object result)
        {
            try
            {
                //do stuff here

                //call _wrappedObject object
                result = _wrappedObject.GetType().GetTypeInfo().GetMethod(binder.Name).Invoke(_wrappedObject, args);
                return true;
            }
            catch
            {
                result = null;
                return false;
            }
        }
    }
}