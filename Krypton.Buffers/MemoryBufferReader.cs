using System;
using System.Buffers.Binary;
using System.Runtime.CompilerServices;

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
        public sbyte ReadInt8()
        {
            return (sbyte)ReadByte();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public byte ReadUInt8()
        {
            return ReadByte();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public short ReadInt16()
        {
            return BinaryPrimitives.ReadInt16LittleEndian(ReadBytes(2).Span);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ushort ReadUInt16()
        {
            return BinaryPrimitives.ReadUInt16LittleEndian(ReadBytes(2).Span);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int ReadInt32()
        {
            return BinaryPrimitives.ReadInt32LittleEndian(ReadBytes(4).Span);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public uint ReadUInt32()
        {
            return BinaryPrimitives.ReadUInt32LittleEndian(ReadBytes(4).Span);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public long ReadInt64()
        {
            return BinaryPrimitives.ReadInt64LittleEndian(ReadBytes(8).Span);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ulong ReadUInt64()
        {
            return BinaryPrimitives.ReadUInt64LittleEndian(ReadBytes(8).Span);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float ReadFloat32()
        {
            return BinaryPrimitives.ReadUInt32LittleEndian(ReadBytes(4).Span);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public double ReadFloat64()
        {
            return BinaryPrimitives.ReadUInt64LittleEndian(ReadBytes(8).Span);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public char ReadChar()
        {
            return (char)_buffer.Span[Offset++];
        }

        public string ReadString8()
        {
            var size = ReadUInt16();
            return string.Create(size, this, (str, reader) =>
            {
                for (var i = 0; i < str.Length; i++)
                {
                    str[i] = (char)reader.ReadUInt8();
                }
            });
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ReadOnlyMemory<byte> ReadBlob()
        {
            var size = ReadUInt16();
            return ReadBytes(size);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool ReadBool()
        {
            return ReadUInt8() == 1;
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
