using System.Threading.Tasks;

namespace Extrasolar.Demo.Client
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Task.Delay(-1).GetAwaiter().GetResult();
        }
    }
}