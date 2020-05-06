using System;
using System.Buffers.Binary;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Krypton.Buffers
{
    public class MemoryBufferReader
    {
        private ReadOnlyMemory<byte> _buffer;

        public int Offset { get; private set; }
        
        public MemoryBufferReader(ReadOnlyMemory<byte> buffer)
        {
            _buffer = buffer;
            Offset = 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ReadOnlyMemory<byte> ReadBytes(int count)
        {
            var mem = _buffer.Slice(Offset, count);
            Offset += count;
            return mem;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public byte ReadByte()
        {
            return _buffer.Span[Offset++];
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
            var x = BinaryPrimitives.ReadUInt16LittleEndian(_buffer.Slice(Offset).Span);
            Offset += size;
            return x;
        }

        public ReadOnlySpan<ushort> ReadUInt16Slice(int count)
        {
            const int isize = sizeof(ushort);
            var x = MemoryMarshal.Cast<byte, ushort>(_buffer.Slice(Offset).Span);
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
            var x = BinaryPrimitives.ReadInt16LittleEndian(_buffer.Slice(Offset).Span);
            Offset += size;
            return x;
        }
        
        public ReadOnlySpan<short> ReadInt16Slice(int count)
        {
            const int isize = sizeof(short);
            var x = MemoryMarshal.Cast<byte, short>(_buffer.Slice(Offset).Span);
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
            var x = BinaryPrimitives.ReadUInt32LittleEndian(_buffer.Slice(Offset).Span);
            Offset += size;
            return x;
        }

        public ReadOnlySpan<uint> ReadUInt32Slice(int count)
        {
            const int isize = sizeof(uint);
            var x = MemoryMarshal.Cast<byte, uint>(_buffer.Slice(Offset).Span);
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
            var x = BinaryPrimitives.ReadInt32LittleEndian(_buffer.Slice(Offset).Span);
            Offset += size;
            return x;
        }
        
        public ReadOnlySpan<int> ReadInt32Slice(int count)
        {
            const int isize = sizeof(int);
            var x = MemoryMarshal.Cast<byte, int>(_buffer.Slice(Offset).Span);
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
            var x = BinaryPrimitives.ReadUInt64LittleEndian(_buffer.Slice(Offset).Span);
            Offset += size;
            return x;
        }

        public ReadOnlySpan<ulong> ReadUInt64Slice(int count)
        {
            const int isize = sizeof(ulong);
            var x = MemoryMarshal.Cast<byte, ulong>(_buffer.Slice(Offset).Span);
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
            var x = BinaryPrimitives.ReadInt64LittleEndian(_buffer.Slice(Offset).Span);
            Offset += size;
            return x;
        }

        public ReadOnlySpan<long> ReadInt64Slice(int count)
        {
            const int isize = sizeof(long);
            var x = MemoryMarshal.Cast<byte, long>(_buffer.Slice(Offset).Span);
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
            var x = MemoryMarshal.Read<float>(_buffer.Slice(Offset).Span);
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
            var x = MemoryMarshal.Read<double>(_buffer.Slice(Offset).Span);
            Offset += size;
            return x;
        }
        
        public Guid ReadGuid()
        {
            const int size = 16;
            var guid = new Guid(_buffer.Slice(Offset, size).Span);
            Offset += size;    
            return guid;
        }

        public string ReadString8()
        {
            var size = ReadUInt16();
            return string.Create(size, this, (str, reader) =>
            {
                for (var i = 0; i < str.Length; i++)
                    str[i] = (char) reader.ReadUInt8();
            });
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SkipBytes(int n)
        {
            Offset += (ushort)n;
        }

        public ReadOnlyMemory<byte> RemainingData => _buffer.Slice(Offset);

        public int RemainingSize => _buffer.Length - Offset;
    }
}
