using System;
using System.Collections.Generic;

namespace Extrasolar.Rpc.Proxying
{
    public class CallProxyBinder<TInterface> : DynamicMethodBinder where TInterface : class
    {
        private RpcCaller<TInterface> _caller;

        public override object Target => _caller;

        public CallProxyBinder(RpcCaller<TInterface> caller)
        {
            _caller = caller;
        }

        public override object[] InvokeMethod(string metadata, params object[] parameters)
        {
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
                var parameterTypeNames = metadataComponents[2].Split('|');
                foreach (var typeName in parameterTypeNames)
                {
                    // Resolve type
                    var rType = Type.GetType(typeName);
                    parameterTypes.Add(rType);
                }
            }
            // Invoke method based on arguments, it will be deserialized according to return type
            object result = null;
            if (returnType != null)
            {
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