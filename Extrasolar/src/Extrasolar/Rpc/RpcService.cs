using Extrasolar.IO;
using Extrasolar.JsonRpc.Types;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Extrasolar.Rpc
{
    /// <summary>
    /// Represents a service that automatically implements an RPC request handler.
    /// An instance implementing the RPC interface is used to automatically
    /// handle RPC requests
    /// </summary>
    /// <typeparam name="TInterface">The interface for the service to implement</typeparam>
    public class RpcService<TInterface> : IDisposable where TInterface : class
    {
        public NetworkRpcEndpoint RpcClient { get; }
        public TInterface ServiceImplementation { get; private set; }
        private MethodInfo[] _cachedMethodInfo;

        public RpcService(NetworkRpcService netRpcClient)
        {
            RpcClient = netRpcClient;
            netRpcClient.RpcLayer.RequestPipeline.AddItemToEnd(HandleRequest);
        }

        private async Task<Response> HandleRequest(Request request)
        {
            try
            {
                // Handle request
                // First bind to service method
                var methodName = request.Method;
                // Process arguments
                var rawArgs = request.Parameters;
                var callArgs = new List<object>();
                var rawParams = ((JValue)rawArgs).Value;
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
                            return new ErrorResponse(request, new Error(JsonRpcErrorCode.MethodNotFound, "Could not resolve method definitively", methodName));
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
                    return await Task.FromResult(new ResultResponse(request, resultJObject));
                }
                catch (Exception ex)
                {
                    return new ErrorResponse(request, new Error(JsonRpcErrorCode.InternalError, ex.Message, JToken.FromObject(ex)));
                }
            }
            catch (InvalidCastException)
            {
                return new ErrorResponse(request, new Error(JsonRpcErrorCode.InvalidParams, "Could not call method with specified parameters", request.Method));
            }
            catch (NotImplementedException)
            {
                return new ErrorResponse(request, new Error(JsonRpcErrorCode.MethodNotFound, "Could not find a matching method", request.Method));
            }
            catch (Exception ex)
            {
                return new ErrorResponse(request, new Error(JsonRpcErrorCode.InternalError, ex.Message, JToken.FromObject(ex)));
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
                    // Reserialize to anonymous object
                    result = (object)obj;
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