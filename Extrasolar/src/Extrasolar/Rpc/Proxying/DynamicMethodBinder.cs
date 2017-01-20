namespace Extrasolar.Rpc.Proxying
{
    /// <summary>
    /// Helps to dynamically bind to and intercept methods generated with Emit to call existing code
    /// </summary>
    public abstract class DynamicMethodBinder
    {
        public const string InvokeMethodName = nameof(InvokeMethod);
        public abstract object Target { get; }

        /// <summary>
        /// Binds to a method invocation, processes it, and returns the result.
        /// The metadata contains the method signature info, the parameters
        /// array contains the parameters, and the first element of the return
        /// array holds the return value, and subsequent elements hold ByRef
        /// parameters
        /// </summary>
        /// <param name="metadata">
        /// Contains the method signature info. It is of the format `Namespace.ReturnType|MethodName|Namespace.ArgType1|Namespace.ArgType2`.
        /// It is the return type, the method name, and the types of arguments separated by the `|` character.
        /// </param>
        /// <param name="parameters">
        /// Contains all the parameters passed to the method invocation
        /// </param>
        /// <returns>
        /// The first element contains the return value,
        /// and subsequent elements contain ByRef parameters.
        /// </returns>
        public abstract object[] InvokeMethod(string metadata, params object[] parameters);
    }
}