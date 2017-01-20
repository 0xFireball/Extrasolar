namespace Extrasolar.Demo.Loopback.Types
{
    public class Cookie
    {
        public enum CookieFlavor
        {
            Oatmeal,
            Chocolate,
        }

        public CookieFlavor Flavor { get; set; }

        public double Radius { get; set; } = 4;
        public double Thickness { get; set; } = 0.7;
    }
}