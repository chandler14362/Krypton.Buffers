using System;
using System.Buffers.Binary;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Krypton.Buffers
{
    public ref struct SpanBufferReader
    {
        private ReadOnlySpan<byte> _buffer;

        public int Offset { get; private set; }
        
        public SpanBufferReader(ReadOnlySpan<byte> buffer)
        {
            _buffer = buffer;
            Offset = 0;
        }

        public ReadOnlySpan<byte> ReadBytes(int count)
        {
            var slice = _buffer.Slice(Offset, count);
            Offset += count;
            return slice;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public byte ReadByte()
        {
            return _buffer[Offset++];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public byte ReadUInt8()
        {
            return ReadByte();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public sbyte ReadInt8()
        {
            return (sbyte)ReadByte();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool ReadBool()
        {
            return ReadUInt8() == 1 ? true : false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ushort ReadUInt16()
        {
            const int size = sizeof(ushort);
            var x = BinaryPrimitives.ReadUInt16LittleEndian(_buffer.Slice(Offset));
            Offset += size;
            return x;
        }

        public ReadOnlySpan<ushort> ReadUInt16Slice(int count)
        {
            const int isize = sizeof(ushort);
            var x = MemoryMarshal.Cast<byte, ushort>(_buffer.Slice(Offset));
            Offset += isize * count;

            if (BitConverter.IsLittleEndian)
                return x.Slice(0, count);

            var flipped = new ushort[count];
            for (var i = 0; i < count; i++)
                flipped[i] = BinaryPrimitives.ReverseEndianness(x[i]);

            return flipped;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public short ReadInt16()
        {
            const int size = sizeof(short);
            var x = BinaryPrimitives.ReadInt16LittleEndian(_buffer.Slice(Offset));
            Offset += size;
            return x;
        }
        
        public ReadOnlySpan<short> ReadInt16Slice(int count)
        {
            const int isize = sizeof(short);
            var x = MemoryMarshal.Cast<byte, short>(_buffer.Slice(Offset));
            Offset += isize * count;

            if (BitConverter.IsLittleEndian)
                return x.Slice(0, count);

            var flipped = new short[count];
            for (var i = 0; i < count; i++)
                flipped[i] = BinaryPrimitives.ReverseEndianness(x[i]);

            return flipped;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public uint ReadUInt32()
        {
            const int size = sizeof(uint);
            var x = BinaryPrimitives.ReadUInt32LittleEndian(_buffer.Slice(Offset));
            Offset += size;
            return x;
        }

        public ReadOnlySpan<uint> ReadUInt32Slice(int count)
        {
            const int isize = sizeof(uint);
            var x = MemoryMarshal.Cast<byte, uint>(_buffer.Slice(Offset));
            Offset += isize * count;

            if (BitConverter.IsLittleEndian)
                return x.Slice(0, count);

            var flipped = new uint[count];
            for (var i = 0; i < count; i++)
                flipped[i] = BinaryPrimitives.ReverseEndianness(x[i]);

            return flipped;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int ReadInt32()
        {
            const int size = sizeof(int);
            var x = BinaryPrimitives.ReadInt32LittleEndian(_buffer.Slice(Offset));
            Offset += size;
            return x;
        }
        
        public ReadOnlySpan<int> ReadInt32Slice(int count)
        {
            const int isize = sizeof(int);
            var x = MemoryMarshal.Cast<byte, int>(_buffer.Slice(Offset));
            Offset += isize * count;

            if (BitConverter.IsLittleEndian)
                return x.Slice(0, count);

            var flipped = new int[count];
            for (var i = 0; i < count; i++)
                flipped[i] = BinaryPrimitives.ReverseEndianness(x[i]);

            return flipped;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ulong ReadUInt64()
        {
            const int size = sizeof(ulong);
            var x = BinaryPrimitives.ReadUInt64LittleEndian(_buffer.Slice(Offset));
            Offset += size;
            return x;
        }

        public ReadOnlySpan<ulong> ReadUInt64Slice(int count)
        {
            const int isize = sizeof(ulong);
            var x = MemoryMarshal.Cast<byte, ulong>(_buffer.Slice(Offset));
            Offset += isize * count;

            if (BitConverter.IsLittleEndian)
                return x.Slice(0, count);

            var flipped = new ulong[count];
            for (var i = 0; i < count; i++)
                flipped[i] = BinaryPrimitives.ReverseEndianness(x[i]);

            return flipped;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public long ReadInt64()
        {
            const int size = sizeof(long);
            var x = BinaryPrimitives.ReadInt64LittleEndian(_buffer.Slice(Offset));
            Offset += size;
            return x;
        }

        public ReadOnlySpan<long> ReadInt64Slice(int count)
        {
            const int isize = sizeof(long);
            var x = MemoryMarshal.Cast<byte, long>(_buffer.Slice(Offset));
            Offset += isize * count;

            if (BitConverter.IsLittleEndian)
                return x.Slice(0, count);

            var flipped = new long[count];
            for (var i = 0; i < count; i++)
                flipped[i] = BinaryPrimitives.ReverseEndianness(x[i]);

            return flipped;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float ReadFloat32()
        {
            // TODO: big endian support
            if (!BitConverter.IsLittleEndian)
                throw new NotImplementedException();

            const int size = sizeof(float);
            var x = MemoryMarshal.Read<float>(_buffer.Slice(Offset));
            Offset += size;
            return x;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public double ReadFloat64()
        {
            // TODO: big endian support
            if (!BitConverter.IsLittleEndian)
                throw new NotImplementedException();
            
            const int size = sizeof(double);
            var x = MemoryMarshal.Read<double>(_buffer.Slice(Offset));
            Offset += size;
            return x;
        }

        public Guid ReadGuid()
        {
            const int size = 16;
            var guid = new Guid(_buffer.Slice(Offset, size));
            Offset += size;    
            return guid;
        }

        public string ReadString8()
        {
            var size = ReadUInt16();
            Span<char> str = size < 256 ? stackalloc char[size] : new char[size];
            for (var i = 0; i < size; i++)
                str[i] = (char)this.ReadByte();
            return new string(str);
        }

        public void SkipBytes(int bytes)
        {
            Offset += bytes;
        }

        public ReadOnlySpan<byte> RemainingData => _buffer.Slice(Offset);

        /// <summary>
        /// Gets the amount of remaining bytes in the <see cref="SpanBufferReader"/>.
        /// </summary>
        public readonly int RemainingSize => _buffer.Length - Offset;
    }
}
