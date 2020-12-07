using System;
using System.Buffers.Binary;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

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
        private void ThrowIfEndOfBuffer(int neededSize)
        {
            if (Offset + neededSize > _buffer.Length)
                throw new EndOfBufferException(_buffer.Length, Offset, neededSize);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ReadOnlyMemory<byte> ReadBytes(int count)
        {
            ThrowIfEndOfBuffer(count);
            var mem = _buffer.Slice(Offset, count);
            Offset += count;
            return mem;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public byte ReadByte()
        {
            ThrowIfEndOfBuffer(sizeof(byte));
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
            ThrowIfEndOfBuffer(size);
            
            var x = BinaryPrimitives.ReadUInt16LittleEndian(_buffer.Slice(Offset).Span);
            Offset += size;
            return x;
        }

        public ReadOnlySpan<ushort> ReadUInt16Slice(int count)
        {
            const int isize = sizeof(ushort);
            ThrowIfEndOfBuffer(isize * count);
            
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
            ThrowIfEndOfBuffer(size);
            
            var x = BinaryPrimitives.ReadInt16LittleEndian(_buffer.Slice(Offset).Span);
            Offset += size;
            return x;
        }
        
        public ReadOnlySpan<short> ReadInt16Slice(int count)
        {
            const int isize = sizeof(short);
            ThrowIfEndOfBuffer(isize * count);
            
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
            ThrowIfEndOfBuffer(size);
            
            var x = BinaryPrimitives.ReadUInt32LittleEndian(_buffer.Slice(Offset).Span);
            Offset += size;
            return x;
        }

        public ReadOnlySpan<uint> ReadUInt32Slice(int count)
        {
            const int isize = sizeof(uint);
            ThrowIfEndOfBuffer(isize * count);
            
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
            ThrowIfEndOfBuffer(size);
            
            var x = BinaryPrimitives.ReadInt32LittleEndian(_buffer.Slice(Offset).Span);
            Offset += size;
            return x;
        }
        
        public ReadOnlySpan<int> ReadInt32Slice(int count)
        {
            const int isize = sizeof(int);
            ThrowIfEndOfBuffer(isize * count);
            
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
            ThrowIfEndOfBuffer(size);
            
            var x = BinaryPrimitives.ReadUInt64LittleEndian(_buffer.Slice(Offset).Span);
            Offset += size;
            return x;
        }

        public ReadOnlySpan<ulong> ReadUInt64Slice(int count)
        {
            const int isize = sizeof(ulong);
            ThrowIfEndOfBuffer(isize * count);
            
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
            ThrowIfEndOfBuffer(size);
            
            var x = BinaryPrimitives.ReadInt64LittleEndian(_buffer.Slice(Offset).Span);
            Offset += size;
            return x;
        }

        public ReadOnlySpan<long> ReadInt64Slice(int count)
        {
            const int isize = sizeof(long);
            ThrowIfEndOfBuffer(isize * count);
            
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
            ThrowIfEndOfBuffer(size);
            
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
            ThrowIfEndOfBuffer(size);
            
            var x = MemoryMarshal.Read<double>(_buffer.Slice(Offset).Span);
            Offset += size;
            return x;
        }
        
        public Guid ReadGuid()
        {
            const int size = 16;
            ThrowIfEndOfBuffer(size);
#if NETSTANDARD2_1
            var guid = new Guid(_buffer.Slice(Offset, size).Span);
#else
            var guid = new Guid(_buffer.Slice(Offset, size).ToArray());
#endif
            Offset += size;    
            return guid;
        }

        public string ReadString(Encoding encoding)
        {
            var length = ReadUInt16();
            var bytes = ReadBytes(length);
#if NETSTANDARD2_1
            return encoding.GetString(bytes.Span);
#else
            return encoding.GetString(bytes.ToArray());
#endif
        }

        public string ReadUTF8String()
            => ReadString(Encoding.UTF8);

        public string ReadUTF16String()
            => ReadString(Encoding.Unicode);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SkipBytes(int count)
        {
            ThrowIfEndOfBuffer(count);
            Offset += (ushort)count;
        }

        public ReadOnlyMemory<byte> RemainingData => _buffer.Slice(Offset);

        public int RemainingSize => _buffer.Length - Offset;
    }
}
