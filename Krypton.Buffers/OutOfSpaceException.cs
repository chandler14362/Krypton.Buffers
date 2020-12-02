using System;

namespace Krypton.Buffers
{
    public class OutOfSpaceException : Exception
    {
        public int Size { get; }
        public int Offset { get; }
        public int NeededSize { get; }
        
        public OutOfSpaceException(int size, int offset, int neededSize) 
            : base($"Size: {size}, Offset: {offset}, Needed Size: {neededSize}")
        {
            Size = size;
            Offset = offset;
            NeededSize = neededSize;
        }
    }
}
