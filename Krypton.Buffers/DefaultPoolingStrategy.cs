namespace Krypton.Buffers
{
    // Default pooling strategy is no pooling at all
    internal class DefaultPoolingStrategy : IPoolingStrategy
    {
        public static readonly IPoolingStrategy Instance = new DefaultPoolingStrategy();
        
        public const int GrowthFactor = 2;

        private DefaultPoolingStrategy()
        {
        }
        
        public byte[] Resize(int size, int neededLength)
        {
            var newLength = size * GrowthFactor;
            while (neededLength > newLength)
                newLength *= GrowthFactor;
            return new byte[newLength];
        }

        public void Free(byte[] data)
        {
        }
    }
}
