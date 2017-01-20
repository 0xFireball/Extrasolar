using Extrasolar.IO;
using Extrasolar.JsonRpc.Types;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Extrasolar.Rpc
{
    public class RpcService<TInterface> : IDisposable where TInterface : class
    {
        public NetworkRpcEndpoint RpcClient { get; set; }
        public TInterface ServiceImplementation { get; private set; }
        private MethodInfo[] _cachedMethodInfo;

        public RpcService(NetworkRpcService netRpcClient)
        {
            RpcClient = netRpcClient;
            netRpcClient.RpcLayer.RequestPipeline.AddItemToEnd(HandleRequest);
        }

        private Response HandleRequest(Request request)
        {
            // Handle request
            // First bind to service method
            var methodName = request.Method;
            // Process arguments
            var rawArgs = request.Parameters;
            var callArgs = new List<object>();
            if (rawArgs is JArray)
            {
                callArgs.AddRange((rawArgs as JArray).Children().Select(x => x.ToObject<object>()));
            }
            else if (rawArgs is JValue)
            {
                var rawParams = (rawArgs as JValue).Value;
                if (rawParams is string)
                {
                    var paramsData = JsonConvert.DeserializeObject<object>((string)rawParams);
                    if (paramsData is JArray)
                    {
                        // TODO: Properly deserialize values
                        //var paramsArray = (paramsData as JArray).Children().Select(x => x.ToObject<object>());
                        var paramsArray = JsonConvert.DeserializeObject<object[]>((string)rawParams);
                        foreach (var parameter in paramsArray)
                        {
                            callArgs.Add(parameter);
                        }
                    }
                    else
                    {
                        callArgs.Add(paramsData);
                    }
                }
            }
            else
            {
                callArgs.Add(rawArgs.ToObject<object>());
            }
            // TODO: Get proper object args
            var targetMethodCandidates = _cachedMethodInfo.Where(x => x.Name == methodName);
            // Check if we have a definitive target
            if (!targetMethodCandidates.Any())
            {
                // No implementation found
                throw new NotImplementedException();
            }
            if (targetMethodCandidates.Count() > 1)
            {
                // Attempt to resolve by checking parameter count and types
                var paramCount = callArgs.Count;
                targetMethodCandidates = targetMethodCandidates.Where(x => x.GetParameters().Count() == paramCount);
                targetMethodCandidates = targetMethodCandidates.Where(method =>
                {
                    var prmTypes = method.GetParameters().Select(x => x.ParameterType).ToArray();
                    var tmpCallArgs = new List<object>(callArgs);
                    for (int i = 0; i < prmTypes.Length; i++)
                    {
                        var paramType = prmTypes[i];
                        var callParam = tmpCallArgs[i];
                        if (!paramType.IsInstanceOfType(callParam))
                        {
                            // If the call parameter isn't an instance, cast
                            var originalCallArgType = callParam.GetType();
                            // Only convert long to int for calls
                            //tmpCallArgs[i] = Convert.ChangeType(callParam, paramType);
                            if (paramType == typeof(int) && callParam is long)
                            {
                                tmpCallArgs[i] = Convert.ChangeType(callParam, typeof(int));
                            }
                            // If that succeeded, types are convertible.
                            // Now make sure it's assignable
                            if (!paramType.IsAssignableFrom(tmpCallArgs[i].GetType()))
                            {
                                // Not assignable
                                return false;
                            }
                            // Finally, use a type constraint
                            if (paramType.FullName != tmpCallArgs[i].GetType().FullName)
                            {
                                // Type does not exactly match
                                return false;
                            }
                        }
                    }
                    return true;
                });
                if (!targetMethodCandidates.Any())
                {
                    // No implementation found
                    throw new NotImplementedException();
                }
                else if (targetMethodCandidates.Count() > 1)
                {
                    // TODO: Check all types
                    if (targetMethodCandidates.Count() > 1)
                    {
                        // Could not resolve method definitively
                        throw new ArgumentException("Could not resolve method definitively");
                    }
                }
            }
            var selectedMethod = targetMethodCandidates.First();
            // Normalize arguments
            var parameterTypes = selectedMethod.GetParameters().Select(x => x.ParameterType).ToArray();
            for (int i = 0; i < parameterTypes.Length; i++)
            {
                var paramType = parameterTypes[i];
                var callParam = callArgs[i];
                if (!paramType.IsInstanceOfType(callParam) || paramType == typeof(object))
                {
                    // If the call parameter doesn't match, change type
                    callArgs[i] = AutoConvert(callParam, paramType);
                }
            }
            try
            {
                var result = selectedMethod.Invoke(ServiceImplementation, callArgs.ToArray());
                var resultJObject = JToken.FromObject(result);
                return new ResultResponse(request, resultJObject);
            }
            catch (Exception ex)
            {
                return new ErrorResponse(request, new Error(-1, ex.Message, ex.ToString()));
            }
        }

        public object AutoConvert(object obj, Type type)
        {
            object result = null;
            // If the call parameter doesn't match, cast
            if ((obj is JObject) || (obj is JArray))
            {
                if (type == typeof(object))
                {
                    result = ((JToken)obj).ToObject<dynamic>();
                }
                else
                {
                    result = ((JToken)obj).ToObject(type);
                }
            }
            else if (obj is JValue)
            {
                var valueObject = ((JValue)obj).Value;
                result = Convert.ChangeType(valueObject, type);
            }
            else
            {
                result = Convert.ChangeType(obj, type);
            }
            return result;
        }

        /// <summary>
        /// Sets the implementation that will handle requests
        /// </summary>
        /// <param name="implementation"></param>
        public void Export(TInterface implementation)
        {
            ServiceImplementation = implementation;
            _cachedMethodInfo = ServiceImplementation.GetType().GetTypeInfo().GetMethods();
        }

        public void Dispose()
        {
            RpcClient.Dispose();
        }
    }
}