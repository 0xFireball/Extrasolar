using Extrasolar.Rpc.Proxying.SWUtil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace Extrasolar.Rpc.Proxying
{
    internal static class ProxyFactory
    {
        private const string PROXY_ASSEMBLY = "ProxyAssembly";
        private const string INVOKE_METHOD = "InvokeMethod";
        private const string PROXY_MODULE = "ProxyModule";
        private const string PROXY = "Proxy";

        // pooled dictionary achieves same or better performance as ThreadStatic without creating as many builders under average load
        private static PooledDictionary<string, ProxyBuilder> _proxies = new PooledDictionary<string, ProxyBuilder>();

        public static TInterface CreateEmptyProxy<TInterface>(DynamicMethodBinder methodBinder, Type binderType) where TInterface : class
        {
            Type interfaceType = typeof(TInterface);

            // derive unique key for this dynamic assembly by interface, channel and ctor type names
            var proxyName = $"{PROXY}_" + interfaceType.FullName + binderType?.FullName + $"_{Guid.NewGuid().ToString("N")}";

            // get pooled proxy builder
            ProxyBuilder proxyBuilder = null;
            TInterface proxy = null;
            try
            {
                proxyBuilder = _proxies.Request(proxyName, () => CreateSimpleProxyBuilder(proxyName, interfaceType, methodBinder, binderType));
                proxy = CreateEmptyProxy<TInterface>(proxyBuilder);
            }
            finally
            {
                // return builder to the pool
                if (null != proxyBuilder) _proxies.Release(proxyName, proxyBuilder);
            }
            return proxy;
        }

        public static TInterface CreateProxy<TInterface>(Type channelType, Type ctorArgType, object channelCtorValue) where TInterface : class
        {
            Type interfaceType = typeof(TInterface);

            // derive unique key for this dynamic assembly by interface, channel and ctor type names
            var proxyName = interfaceType.FullName + channelType.FullName + ctorArgType.FullName;

            // get pooled proxy builder
            var localChannelType = channelType;
            var localCtorArgType = ctorArgType;
            ProxyBuilder proxyBuilder = null;
            TInterface proxy = null;
            try
            {
                proxyBuilder = _proxies.Request(proxyName, () => CreateProxyBuilder(proxyName, interfaceType, localChannelType, localCtorArgType));
                proxy = CreateProxy<TInterface>(proxyBuilder, channelCtorValue);
            }
            finally
            {
                // return builder to the pool
                if (null != proxyBuilder) _proxies.Release(proxyName, proxyBuilder);
            }
            return proxy;
        }

        private static TInterface CreateProxy<TInterface>(ProxyBuilder proxyBuilder, object channelCtorValue) where TInterface : class
        {
            //create the type and construct an instance
            Type[] ctorArgTypes = { typeof(Type), proxyBuilder.CtorType };
            var tInfo = proxyBuilder.TypeBuilder.CreateTypeInfo();
            var t = tInfo.AsType();
            var constructorInfo = t.GetConstructor(ctorArgTypes);
            if (constructorInfo != null)
            {
                var instance = (TInterface)constructorInfo.Invoke(new object[] { typeof(TInterface), channelCtorValue });
                return instance;
            }
            return null;
        }

        private static TInterface CreateEmptyProxy<TInterface>(ProxyBuilder proxyBuilder) where TInterface : class
        {
            //create the type and construct an instance
            //Type[] ctorArgTypes = { typeof(Type) };
            Type[] ctorArgTypes = Type.EmptyTypes;
            var tInfo = proxyBuilder.TypeBuilder.CreateTypeInfo();
            var t = tInfo.AsType();
            var constructorInfo = t.GetConstructor(ctorArgTypes);
            if (constructorInfo != null)
            {
                //var instance = (TInterface)constructorInfo.Invoke(new object[] { typeof(TInterface) });
                var instance = (TInterface)constructorInfo.Invoke(new object[0]);
                return instance;
            }
            return null;
        }

        private static ProxyBuilder CreateSimpleProxyBuilder(string proxyName, Type interfaceType, DynamicMethodBinder methodBinder, Type parentType)
        {
            // create a new assembly for the proxy
            var assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(new AssemblyName(PROXY_ASSEMBLY), AssemblyBuilderAccess.Run);

            // create a new module for the proxy
            var moduleBuilder = assemblyBuilder.DefineDynamicModule(PROXY_MODULE);

            // Set the class to be public and sealed
            TypeAttributes typeAttributes = TypeAttributes.Class | TypeAttributes.Public | TypeAttributes.Sealed;

            // Construct the type builder
            TypeBuilder typeBuilder;
            var genTypeName = $"{PROXY}_" + interfaceType.FullName + parentType?.FullName + $"_{Guid.NewGuid().ToString("N")}_{PROXY}";
            if (parentType != null)
            {
                typeBuilder = moduleBuilder.DefineType(genTypeName, typeAttributes, parentType);
            }
            else
            {
                typeBuilder = moduleBuilder.DefineType(genTypeName, typeAttributes);
            }
            var allInterfaces = new List<Type>(interfaceType.GetInterfaces());
            allInterfaces.Add(interfaceType);

            // add the interface
            typeBuilder.AddInterfaceImplementation(interfaceType);

            // construct the constructor
            // TODO

            // construct the type maps
            var ldindOpCodeTypeMap = new Dictionary<Type, OpCode>();
            ldindOpCodeTypeMap.Add(typeof(Boolean), OpCodes.Ldind_I1);
            ldindOpCodeTypeMap.Add(typeof(Byte), OpCodes.Ldind_U1);
            ldindOpCodeTypeMap.Add(typeof(SByte), OpCodes.Ldind_I1);
            ldindOpCodeTypeMap.Add(typeof(Int16), OpCodes.Ldind_I2);
            ldindOpCodeTypeMap.Add(typeof(UInt16), OpCodes.Ldind_U2);
            ldindOpCodeTypeMap.Add(typeof(Int32), OpCodes.Ldind_I4);
            ldindOpCodeTypeMap.Add(typeof(UInt32), OpCodes.Ldind_U4);
            ldindOpCodeTypeMap.Add(typeof(Int64), OpCodes.Ldind_I8);
            ldindOpCodeTypeMap.Add(typeof(UInt64), OpCodes.Ldind_I8);
            ldindOpCodeTypeMap.Add(typeof(Char), OpCodes.Ldind_U2);
            ldindOpCodeTypeMap.Add(typeof(Double), OpCodes.Ldind_R8);
            ldindOpCodeTypeMap.Add(typeof(Single), OpCodes.Ldind_R4);
            var stindOpCodeTypeMap = new Dictionary<Type, OpCode>();
            stindOpCodeTypeMap.Add(typeof(Boolean), OpCodes.Stind_I1);
            stindOpCodeTypeMap.Add(typeof(Byte), OpCodes.Stind_I1);
            stindOpCodeTypeMap.Add(typeof(SByte), OpCodes.Stind_I1);
            stindOpCodeTypeMap.Add(typeof(Int16), OpCodes.Stind_I2);
            stindOpCodeTypeMap.Add(typeof(UInt16), OpCodes.Stind_I2);
            stindOpCodeTypeMap.Add(typeof(Int32), OpCodes.Stind_I4);
            stindOpCodeTypeMap.Add(typeof(UInt32), OpCodes.Stind_I4);
            stindOpCodeTypeMap.Add(typeof(Int64), OpCodes.Stind_I8);
            stindOpCodeTypeMap.Add(typeof(UInt64), OpCodes.Stind_I8);
            stindOpCodeTypeMap.Add(typeof(Char), OpCodes.Stind_I2);
            stindOpCodeTypeMap.Add(typeof(Double), OpCodes.Stind_R8);
            stindOpCodeTypeMap.Add(typeof(Single), OpCodes.Stind_R4);

            // TODO: Allow passing in a binder to handle method calls
            // construct the method builders from the method infos defined in the interface
            var methods = GetAllMethods(allInterfaces);
            foreach (MethodInfo methodInfo in methods)
            {
                var methodBuilder = BindMethod(methodBinder, methodInfo, typeBuilder, ldindOpCodeTypeMap, stindOpCodeTypeMap);
                typeBuilder.DefineMethodOverride(methodBuilder, methodInfo);
            }

            // create proxy builder
            var result = new ProxyBuilder
            {
                ProxyName = proxyName,
                InterfaceType = interfaceType,
                CtorType = null, // Shouldn't matter if we use the Empty proxy builder
                AssemblyBuilder = assemblyBuilder,
                ModuleBuilder = moduleBuilder,
                TypeBuilder = typeBuilder
            };
            return result;
        }

        /// <summary>
        /// Bind a generated proxy method to an existing target method
        /// </summary>
        /// <param name="binder"></param>
        /// <param name="proxyMethodInfo"></param>
        /// <param name="typeBuilder"></param>
        /// <param name="ldindOpCodeTypeMap"></param>
        /// <param name="stindOpCodeTypeMap"></param>
        /// <returns></returns>
        public static MethodBuilder BindMethod(DynamicMethodBinder binder, MethodInfo proxyMethodInfo, TypeBuilder typeBuilder, Dictionary<Type, OpCode> ldindOpCodeTypeMap, Dictionary<Type, OpCode> stindOpCodeTypeMap)
        {
            var paramInfos = proxyMethodInfo.GetParameters();
            int nofParams = paramInfos.Length;
            Type[] parameterTypes = new Type[nofParams];
            for (int i = 0; i < nofParams; i++) parameterTypes[i] = paramInfos[i].ParameterType;
            Type returnType = proxyMethodInfo.ReturnType;
            var methodBuilder = typeBuilder.DefineMethod(proxyMethodInfo.Name, MethodAttributes.Public | MethodAttributes.Virtual, returnType, parameterTypes);

            var mIL = methodBuilder.GetILGenerator();
            // TODO: Inject call to binder
            var binderType = binder.GetType();
            var binderInvokeInfo = binderType.GetMethod(DynamicMethodBinder.InvokeMethodName, BindingFlags.Instance | BindingFlags.Public);
            GenerateILBinding(binderInvokeInfo, proxyMethodInfo, mIL, parameterTypes, returnType, ldindOpCodeTypeMap, stindOpCodeTypeMap);

            return methodBuilder;
        }

        private static ProxyBuilder CreateProxyBuilder(string proxyName, Type interfaceType, Type channelType, Type ctorArgType)
        {
#if NETSTANDARD1_6
            // create a new assembly for the proxy
            var assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(new AssemblyName(PROXY_ASSEMBLY), AssemblyBuilderAccess.Run);

            // create a new module for the proxy
            var moduleBuilder = assemblyBuilder.DefineDynamicModule(PROXY_MODULE);
#else
            AppDomain domain = Thread.GetDomain();
            // create a new assembly for the proxy
            AssemblyBuilder assemblyBuilder = domain.DefineDynamicAssembly(new AssemblyName(PROXY_ASSEMBLY), AssemblyBuilderAccess.Run);

            // create a new module for the proxy
            ModuleBuilder moduleBuilder = assemblyBuilder.DefineDynamicModule(PROXY_MODULE, true);
#endif

            // Set the class to be public and sealed
            TypeAttributes typeAttributes = TypeAttributes.Class | TypeAttributes.Public | TypeAttributes.Sealed;

            // Construct the type builder
            var typeBuilder = moduleBuilder.DefineType(interfaceType.Name + PROXY, typeAttributes, channelType);
            var allInterfaces = new List<Type>(interfaceType.GetInterfaces());
            allInterfaces.Add(interfaceType);

            // add the interface
            typeBuilder.AddInterfaceImplementation(interfaceType);

            // construct the constructor
            Type[] ctorArgTypes = { typeof(Type), ctorArgType };
            CreateParameterizedConstructor(channelType, typeBuilder, ctorArgTypes);

            // construct the type maps
            var ldindOpCodeTypeMap = new Dictionary<Type, OpCode>();
            ldindOpCodeTypeMap.Add(typeof(Boolean), OpCodes.Ldind_I1);
            ldindOpCodeTypeMap.Add(typeof(Byte), OpCodes.Ldind_U1);
            ldindOpCodeTypeMap.Add(typeof(SByte), OpCodes.Ldind_I1);
            ldindOpCodeTypeMap.Add(typeof(Int16), OpCodes.Ldind_I2);
            ldindOpCodeTypeMap.Add(typeof(UInt16), OpCodes.Ldind_U2);
            ldindOpCodeTypeMap.Add(typeof(Int32), OpCodes.Ldind_I4);
            ldindOpCodeTypeMap.Add(typeof(UInt32), OpCodes.Ldind_U4);
            ldindOpCodeTypeMap.Add(typeof(Int64), OpCodes.Ldind_I8);
            ldindOpCodeTypeMap.Add(typeof(UInt64), OpCodes.Ldind_I8);
            ldindOpCodeTypeMap.Add(typeof(Char), OpCodes.Ldind_U2);
            ldindOpCodeTypeMap.Add(typeof(Double), OpCodes.Ldind_R8);
            ldindOpCodeTypeMap.Add(typeof(Single), OpCodes.Ldind_R4);
            var stindOpCodeTypeMap = new Dictionary<Type, OpCode>();
            stindOpCodeTypeMap.Add(typeof(Boolean), OpCodes.Stind_I1);
            stindOpCodeTypeMap.Add(typeof(Byte), OpCodes.Stind_I1);
            stindOpCodeTypeMap.Add(typeof(SByte), OpCodes.Stind_I1);
            stindOpCodeTypeMap.Add(typeof(Int16), OpCodes.Stind_I2);
            stindOpCodeTypeMap.Add(typeof(UInt16), OpCodes.Stind_I2);
            stindOpCodeTypeMap.Add(typeof(Int32), OpCodes.Stind_I4);
            stindOpCodeTypeMap.Add(typeof(UInt32), OpCodes.Stind_I4);
            stindOpCodeTypeMap.Add(typeof(Int64), OpCodes.Stind_I8);
            stindOpCodeTypeMap.Add(typeof(UInt64), OpCodes.Stind_I8);
            stindOpCodeTypeMap.Add(typeof(Char), OpCodes.Stind_I2);
            stindOpCodeTypeMap.Add(typeof(Double), OpCodes.Stind_R8);
            stindOpCodeTypeMap.Add(typeof(Single), OpCodes.Stind_R4);

            // construct the method builders from the method infos defined in the interface
            var methods = GetAllMethods(allInterfaces);
            foreach (MethodInfo methodInfo in methods)
            {
                var methodBuilder = ConstructMethod(channelType, methodInfo, typeBuilder, ldindOpCodeTypeMap, stindOpCodeTypeMap);
                typeBuilder.DefineMethodOverride(methodBuilder, methodInfo);
            }

            // create proxy builder
            var result = new ProxyBuilder
            {
                ProxyName = proxyName,
                InterfaceType = interfaceType,
                CtorType = ctorArgType,
                AssemblyBuilder = assemblyBuilder,
                ModuleBuilder = moduleBuilder,
                TypeBuilder = typeBuilder
            };
            return result;
        }

        private static List<MethodInfo> GetAllMethods(List<Type> allInterfaces)
        {
            var methods = new List<MethodInfo>();
            foreach (Type interfaceType in allInterfaces)
                methods.AddRange(interfaceType.GetMethods());
            return methods;
        }

        private static void CreateParameterizedConstructor(Type channelType, TypeBuilder typeBuilder, Type[] ctorArgTypes)
        {
            var ctor = typeBuilder.DefineConstructor(MethodAttributes.Public, CallingConventions.HasThis, ctorArgTypes);
            var baseCtor = channelType.GetConstructor(ctorArgTypes);

            var ctorIL = ctor.GetILGenerator();
            ctorIL.Emit(OpCodes.Ldarg_0); // load "this"
            ctorIL.Emit(OpCodes.Ldarg_1); // load serviceType
            ctorIL.Emit(OpCodes.Ldarg_2); // load "endpoint"
            ctorIL.Emit(OpCodes.Call, baseCtor); // call "base(...)"
            ctorIL.Emit(OpCodes.Ret);
        }

        private static MethodBuilder ConstructMethod(Type channelType, MethodInfo methodInfo, TypeBuilder typeBuilder, Dictionary<Type, OpCode> ldindOpCodeTypeMap, Dictionary<Type, OpCode> stindOpCodeTypeMap)
        {
            var paramInfos = methodInfo.GetParameters();
            int nofParams = paramInfos.Length;
            Type[] parameterTypes = new Type[nofParams];
            for (int i = 0; i < nofParams; i++) parameterTypes[i] = paramInfos[i].ParameterType;
            Type returnType = methodInfo.ReturnType;
            var methodBuilder = typeBuilder.DefineMethod(methodInfo.Name, MethodAttributes.Public | MethodAttributes.Virtual, returnType, parameterTypes);

            var mIL = methodBuilder.GetILGenerator();
            GenerateILBinding(channelType, methodInfo, mIL, parameterTypes, methodBuilder.ReturnType, ldindOpCodeTypeMap, stindOpCodeTypeMap);
            return methodBuilder;
        }

        private static void GenerateILBinding(Type channelType, MethodInfo proxyMethodInfo, ILGenerator mIlGen, Type[] inputArgTypes, Type returnType, Dictionary<Type, OpCode> ldindOpCodeTypeMap, Dictionary<Type, OpCode> stindOpCodeTypeMap)
        {
            //get the MethodInfo for InvokeMethod
            var invokeMethodMI = channelType.GetMethod(INVOKE_METHOD, BindingFlags.Instance | BindingFlags.NonPublic);
            GenerateILBinding(invokeMethodMI, proxyMethodInfo, mIlGen, inputArgTypes, returnType, ldindOpCodeTypeMap, stindOpCodeTypeMap);
        }

        /// <summary>
        /// Generate IL to bind a generated proxy method to an existing target method
        /// </summary>
        /// <param name="targetMethodInfo"></param>
        /// <param name="proxyMethodInfo"></param>
        /// <param name="mIlGen"></param>
        /// <param name="inputArgTypes"></param>
        /// <param name="returnType"></param>
        /// <param name="ldindOpCodeTypeMap"></param>
        /// <param name="stindOpCodeTypeMap"></param>
        private static void GenerateILBinding(MethodInfo targetMethodInfo, MethodInfo proxyMethodInfo, ILGenerator mIlGen, Type[] inputArgTypes, Type returnType, Dictionary<Type, OpCode> ldindOpCodeTypeMap, Dictionary<Type, OpCode> stindOpCodeTypeMap)
        {
            mIlGen.Emit(OpCodes.Ldarg_0); //load "this"

            int nofArgs = inputArgTypes.Length;

            //declare local variables
            var resultLb = mIlGen.DeclareLocal(typeof(object[])); // object[] result

            // set local value with method name and arg types to improve perfmance
            // metadata: methodInfo.Name | inputArgTypes[x].FullName .. |
            //var metadata = returnType.FullName + "|" + proxyMethodInfo.Name;
            var metadata = $"{returnType.FullName}|{proxyMethodInfo.Name}";
            //if (inputArgTypes.Length > 0)
            //{
            //    var args = new string[inputArgTypes.Length];
            //    for (int i = 0; i < inputArgTypes.Length; i++) args[i] = inputArgTypes[i].FullName;
            //    metadata += "|" + string.Join("|", args);
            //}
            metadata += inputArgTypes.Length > 0 ? "|" + string.Join("|", inputArgTypes.Select(x => x.FullName)) : "";
            // declare and assign string literal
            var metaLB = mIlGen.DeclareLocal(typeof(string));

#if !NETSTANDARD1_6
            metaLB.SetLocalSymInfo("metaData", 1, 2);
#endif

            //mIL.Emit(OpCodes.Dup);  // causes InvalidProgramException - Common Language Runtime detected an invalid program.
            mIlGen.Emit(OpCodes.Ldstr, metadata);
            mIlGen.Emit(OpCodes.Stloc_1); //load into metaData local variable

            // load metadata into first param for invokeMethodMI
            // mIL.Emit(OpCodes.Dup);  // causes InvalidProgramException - Common Language Runtime detected an invalid program.
            mIlGen.Emit(OpCodes.Ldloc_1);

            mIlGen.Emit(OpCodes.Ldc_I4, nofArgs); // push the number of arguments
            mIlGen.Emit(OpCodes.Newarr, typeof(object)); // create an array of objects

            //store every input argument in the args array
            for (int i = 0; i < nofArgs; i++)
            {
                Type inputType = inputArgTypes[i].IsByRef ? inputArgTypes[i].GetElementType() : inputArgTypes[i];

                mIlGen.Emit(OpCodes.Dup);
                mIlGen.Emit(OpCodes.Ldc_I4, i); //push the index onto the stack
                mIlGen.Emit(OpCodes.Ldarg, i + 1); //load the i'th argument. This might be an address
                if (inputArgTypes[i].IsByRef)
                {
                    if (inputType.GetTypeInfo().IsValueType)
                    {
                        if (inputType.GetTypeInfo().IsPrimitive)
                        {
                            mIlGen.Emit(ldindOpCodeTypeMap[inputType]);
                            mIlGen.Emit(OpCodes.Box, inputType);
                        }
                        else
                            throw new NotSupportedException("Non-primitive native types (e.g. Decimal and Guid) ByRef are not supported.");
                    }
                    else
                        mIlGen.Emit(OpCodes.Ldind_Ref);
                }
                else
                {
                    if (inputArgTypes[i].GetTypeInfo().IsValueType)
                        mIlGen.Emit(OpCodes.Box, inputArgTypes[i]);
                }
                mIlGen.Emit(OpCodes.Stelem_Ref); //store the reference in the args array
            }

            mIlGen.Emit(OpCodes.Call, targetMethodInfo);
            mIlGen.Emit(OpCodes.Stloc, resultLb.LocalIndex); //store the result
            //store the results in the arguments
            for (int i = 0; i < nofArgs; i++)
            {
                if (inputArgTypes[i].IsByRef)
                {
                    var inputType = inputArgTypes[i].GetElementType();
                    mIlGen.Emit(OpCodes.Ldarg, i + 1); //load the address of the argument
                    mIlGen.Emit(OpCodes.Ldloc, resultLb.LocalIndex); //load the result array
                    mIlGen.Emit(OpCodes.Ldc_I4, i + 1); //load the index into the result array
                    mIlGen.Emit(OpCodes.Ldelem_Ref); //load the value in the index of the array
                    if (inputType.GetTypeInfo().IsValueType)
                    {
                        mIlGen.Emit(OpCodes.Unbox, inputArgTypes[i].GetElementType());
                        mIlGen.Emit(ldindOpCodeTypeMap[inputArgTypes[i].GetElementType()]);
                        mIlGen.Emit(stindOpCodeTypeMap[inputArgTypes[i].GetElementType()]);
                    }
                    else
                    {
                        mIlGen.Emit(OpCodes.Castclass, inputArgTypes[i].GetElementType());
                        mIlGen.Emit(OpCodes.Stind_Ref); //store the unboxed value at the argument address
                    }
                }
            }
            if (returnType != typeof(void))
            {
                mIlGen.Emit(OpCodes.Ldloc, resultLb.LocalIndex); //load the result array
                mIlGen.Emit(OpCodes.Ldc_I4, 0); //load the index of the return value. Alway 0
                mIlGen.Emit(OpCodes.Ldelem_Ref); //load the value in the index of the array

                if (returnType.GetTypeInfo().IsValueType)
                {
                    mIlGen.Emit(OpCodes.Unbox, returnType); //unbox it
                    if (returnType.GetTypeInfo().IsPrimitive)        //deal with primitive vs struct value types
                        mIlGen.Emit(ldindOpCodeTypeMap[returnType]);
                    else
                        mIlGen.Emit(OpCodes.Ldobj, returnType);
                }
                else
                    mIlGen.Emit(OpCodes.Castclass, returnType);
            }
            mIlGen.Emit(OpCodes.Ret);
        }
    }
}