namespace Extrasolar.Rpc.Proxying
{
    public class InstanceGenerator
    {
        internal static TInterface BuildInstance<TInterface>()
        {
            return default(TInterface);
        }
    }
}