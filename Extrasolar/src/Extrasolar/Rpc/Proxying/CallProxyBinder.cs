using System;
using System.Collections.Generic;
using System.Linq;

namespace Extrasolar.Rpc.Proxying
{
    public class CallProxyBinder<TInterface> : DynamicMethodBinder, IDisposable where TInterface : class
    {
        private RpcCaller<TInterface> _caller;

        public override object Target => _caller;

        public CallProxyBinder(RpcCaller<TInterface> caller)
        {
            _caller = caller;
        }

        public void Dispose()
        {
            //GC.SuppressFinalize(this);
        }

        public override object[] InvokeMethod(string metadata, params object[] parameters)
        {
            Console.WriteLine(metadata);
            // Parse metadata
            var metadataComponents = metadata.Split('|');
            var returnTypeName = metadataComponents[0];
            Type returnType = null;
            if (!string.IsNullOrWhiteSpace(returnTypeName))
            {
                returnType = Type.GetType(returnTypeName);
            }
            var methodName = metadataComponents[1];
            List<Type> parameterTypes = new List<Type>();
            if (metadataComponents.Length > 2)
            {
                parameterTypes.AddRange(metadataComponents[1].Split('|').Select(typeFullName => Type.GetType(typeFullName)));
            }
            // Invoke method based on arguments, it will be deserialized according to return type
            object result = null;
            if (returnType != null)
            {
                //Console.WriteLine($"Caller available: {_caller != null}");
                //Console.WriteLine($"Return type available: {returnType != null}");
                //Console.WriteLine($"Parameters available: {parameters != null}");
                result = _caller.CallByName(methodName, returnType, parameters);
                // Return result
                return new[] { result };
            }
            else
            {
                _caller.CallByName(methodName, parameters);
                // Return empty
                return new object[0];
            }
        }
    }
}