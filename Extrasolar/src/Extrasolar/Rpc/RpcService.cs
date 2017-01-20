﻿using Extrasolar.IO;
using Extrasolar.JsonRpc.Types;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Extrasolar.Rpc
{
    public class RpcService<TInterface> where TInterface : class
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
                        var paramsArray = (paramsData as JArray).ToArray().Select(x => (x as JValue).Value);
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
                // Attempt to resolve by parameter count
                var paramCount = callArgs.Count;
                targetMethodCandidates = targetMethodCandidates.Where(x => x.GetParameters().Count() == paramCount);
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
            try
            {
                var result = targetMethodCandidates.First().Invoke(ServiceImplementation, callArgs.ToArray());
                var resultJObject = JToken.FromObject(result);
                return new ResultResponse(request, resultJObject);
            }
            catch (Exception ex)
            {
                return new ErrorResponse(request, new Error(-1, ex.Message, ex.ToString()));
            }
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
    }
}