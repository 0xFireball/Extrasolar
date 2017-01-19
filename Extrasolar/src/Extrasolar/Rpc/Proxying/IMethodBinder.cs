using System.Reflection;

namespace Extrasolar.Rpc.Proxying
{
    public interface IMethodBinder
    {
        bool InvokeMethod(MethodInfo methodInfo, out object result, params object[] args);
    }
}