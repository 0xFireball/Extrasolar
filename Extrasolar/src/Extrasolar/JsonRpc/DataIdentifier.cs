namespace Extrasolar.JsonRpc
{
    public static class DataIdentifier
    {
        private static int _idCounter = 0;

        public static int NextId
        {
            get
            {
                return ++_idCounter;
            }
        }
    }
}