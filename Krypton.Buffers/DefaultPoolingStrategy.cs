using System;

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
        
        public Memory<byte> Resize(int size, int neededSize)
        {
            var newLength = size * GrowthFactor;
            while (neededSize > newLength)
                newLength *= GrowthFactor;
            return new byte[newLength];
        }

        public void Free(Memory<byte> data)
        {
        }
    }
}
