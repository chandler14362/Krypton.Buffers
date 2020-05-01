using System;
using System.Buffers;
using System.Buffers.Binary;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Krypton.Buffers
{
    public ref struct GrowingSpanBuffer
    {
        public readonly struct Bookmark
        {
            public readonly int Offset;
            public readonly int Size;

            public Bookmark(int offset, int size)
            {
                Offset = offset;
                Size = size;
            }
        }
        
        public static int GROWTH_FACTOR = 2;

        private Span<byte> _buffer;

        private int _offset;

        public GrowingSpanBuffer(Span<byte> buffer)
        {
            _buffer = buffer;
            _offset = 0;
        }

        private void Reserve(int length)
        {
            if (_offset + length < _buffer.Length)
                return;

            var newLength = _buffer.Length * GROWTH_FACTOR;
            while (_offset + length > newLength)
                newLength *= GROWTH_FACTOR;

            var newBuffer = new byte[newLength];
            _buffer.CopyTo(newBuffer);
            _buffer = newBuffer;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteBool(bool x)
        {
            Reserve(1);
            _buffer[_offset++] = x ? (byte)1 : (byte)0;
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteInt8(sbyte x)
        {
            Reserve(1);
            _buffer[_offset++] = (byte)x;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteUInt8(byte x)
        {
            Reserve(1);
            _buffer[_offset++] = x;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteInt16(short x)
        {
            const int size = sizeof(short);
            Reserve(size);
            BinaryPrimitives.WriteInt16LittleEndian(_buffer.Slice(_offset), x);
            _offset += size;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteUInt16(ushort x)
        {
            const int size = sizeof(ushort);
            Reserve(size);
            BinaryPrimitives.WriteUInt16LittleEndian(_buffer.Slice(_offset), x);
            _offset += size;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteInt32(int x)
        {
            const int size = sizeof(int);
            Reserve(size);
            BinaryPrimitives.WriteInt32LittleEndian(_buffer.Slice(_offset), x);
            _offset += size;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteUInt32(uint x)
        {
            const int size = sizeof(uint);
            Reserve(size);
            BinaryPrimitives.WriteUInt32LittleEndian(_buffer.Slice(_offset), x);
            _offset += size;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteUInt64(ulong x)
        {
            const int size = sizeof(ulong);
            Reserve(size);
            BinaryPrimitives.WriteUInt64LittleEndian(_buffer.Slice(_offset), x);
            _offset += size;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteInt64(long x)
        {
            const int size = sizeof(long);
            Reserve(size);
            BinaryPrimitives.WriteInt64LittleEndian(_buffer.Slice(_offset), x);
            _offset += size;
        }

        public void WriteFloat32(float x)
        {
            // TODO: big endian support
            if (!BitConverter.IsLittleEndian)
                throw new NotImplementedException();
            
            const int size = sizeof(float);
            Reserve(size);
            MemoryMarshal.Write(_buffer.Slice(_offset), ref x);
            _offset += size;
        }

        public void WriteFloat64(double x)
        {
            // TODO: big endian support
            if (!BitConverter.IsLittleEndian)
                throw new NotImplementedException();
            
            const int size = sizeof(double);
            Reserve(size);            
            MemoryMarshal.Write(_buffer.Slice(_offset), ref x);
            _offset += size;
        }

        public void WriteString8(string x)
        {
            Reserve(x.Length + 2);
            BinaryPrimitives.WriteUInt16LittleEndian(_buffer.Slice(_offset), (ushort)x.Length);
            _offset += 2;

            for (var i = 0; i < x.Length; i++)
                _buffer[_offset++] = (byte)x[i];
        }

        public void WriteBytes(ReadOnlySpan<byte> x)
        {
            Reserve(x.Length);
            x.CopyTo(_buffer.Slice(_offset));
            _offset += x.Length;
        }

        public Bookmark ReserveBookmark(int size)
        {
            Reserve(size);
            var bookmark = new Bookmark(_offset, size);
            _offset += size;
            return bookmark;
        }

        public void WriteBookmark<TState>(in Bookmark bookmark, TState state, SpanAction<byte, TState> output)
        {
            var slice = _buffer.Slice(bookmark.Offset, bookmark.Size);
            output(slice, state);
        }

        public void PadBytes(int n)
        {
            Reserve(n);
            _offset += n;
        }

        public ReadOnlySpan<byte> Data => _buffer.Slice(0, _offset);

        public int Size => _offset;
    }
}
