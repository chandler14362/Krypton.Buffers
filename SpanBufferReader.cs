﻿using System;
using System.Buffers.Binary;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Krypton.Buffers
{
    public ref struct SpanBufferReader
    {
        private ReadOnlySpan<byte> _buffer;

        private int _offset;

        public SpanBufferReader(ReadOnlySpan<byte> buffer)
        {
            _buffer = buffer;
            _offset = 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public byte ReadByte()
        {
            return _buffer[_offset++];
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
            var x = BinaryPrimitives.ReadUInt16LittleEndian(_buffer.Slice(_offset));
            _offset += size;
            return x;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public short ReadInt16()
        {
            const int size = sizeof(short);
            var x = BinaryPrimitives.ReadInt16LittleEndian(_buffer.Slice(_offset));
            _offset += size;
            return x;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public uint ReadUInt32()
        {
            const int size = sizeof(uint);
            var x = BinaryPrimitives.ReadUInt32LittleEndian(_buffer.Slice(_offset));
            _offset += size;
            return x;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int ReadInt32()
        {
            const int size = sizeof(int);
            var x = BinaryPrimitives.ReadInt32LittleEndian(_buffer.Slice(_offset));
            _offset += size;
            return x;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ulong ReadUInt64()
        {
            const int size = sizeof(ulong);
            var x = BinaryPrimitives.ReadUInt64LittleEndian(_buffer.Slice(_offset));
            _offset += size;
            return x;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public long ReadInt64()
        {
            const int size = sizeof(long);
            var x = BinaryPrimitives.ReadInt64LittleEndian(_buffer.Slice(_offset));
            _offset += size;
            return x;
        }

        // TODO: Document the functionality of this method
        public ReadOnlySpan<ulong> ReadUInt64Array(int count)
        {
            var x = MemoryMarshal.Cast<byte, ulong>(_buffer.Slice(_offset));
            _offset += 8 * count;

            if (BitConverter.IsLittleEndian)
                return x.Slice(0, count);

            var flipped = new ulong[count];
            for (var i = 0; i < count; i++)
                flipped[i] = BinaryPrimitives.ReverseEndianness(x[i]);

            return flipped;
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
            _offset += bytes;
        }

        public ReadOnlySpan<byte> RemainingData => _buffer.Slice(_offset);

        /// <summary>
        /// Gets the amount of remaining bytes in the <see cref="SpanBufferReader"/>.
        /// </summary>
        public readonly int RemainingSize => _buffer.Length - _offset;

        public int Offset => _offset;
    }
}
