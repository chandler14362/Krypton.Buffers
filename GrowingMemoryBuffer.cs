using System;
using System.Buffers.Binary;
using System.Runtime.CompilerServices;

namespace Krypton.Buffers
{
    public class GrowingMemoryBuffer
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

        private Memory<byte> _buffer;

        private int _offset;

        public GrowingMemoryBuffer(Memory<byte> buffer)
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
            _buffer.Span[_offset++] = x ? (byte)1 : (byte)0;
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteInt8(sbyte x)
        {
            Reserve(1);
            _buffer.Span[_offset++] = (byte)x;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteUInt8(byte x)
        {
            Reserve(1);
            _buffer.Span[_offset++] = x;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteInt16(short x)
        {
            const int size = sizeof(short);
            Reserve(size);
            BinaryPrimitives.WriteInt16LittleEndian(_buffer.Span.Slice(_offset), x);
            _offset += size;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteUInt16(ushort x)
        {
            const int size = sizeof(ushort);
            Reserve(size);
            BinaryPrimitives.WriteUInt16LittleEndian(_buffer.Span.Slice(_offset), x);
            _offset += size;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteInt32(int x)
        {
            const int size = sizeof(int);
            Reserve(size);
            BinaryPrimitives.WriteInt32LittleEndian(_buffer.Span.Slice(_offset), x);
            _offset += size;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteUInt32(uint x)
        {
            const int size = sizeof(uint);
            Reserve(size);
            BinaryPrimitives.WriteUInt32LittleEndian(_buffer.Span.Slice(_offset), x);
            _offset += size;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteUInt64(ulong x)
        {
            const int size = sizeof(ulong);
            Reserve(size);
            BinaryPrimitives.WriteUInt64LittleEndian(_buffer.Span.Slice(_offset), x);
            _offset += size;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteInt64(long x)
        {
            const int size = sizeof(long);
            Reserve(size);
            BinaryPrimitives.WriteInt64LittleEndian(_buffer.Span.Slice(_offset), x);
            _offset += size;
        }

        public void WriteFloat32(float x)
        {
            throw new NotImplementedException();
        }

        public void WriteFloat64(double x)
        {
            throw new NotImplementedException();
        }

        public void WriteString8(string x)
        {
            Reserve(x.Length + 2);
            BinaryPrimitives.WriteUInt16LittleEndian(_buffer.Span.Slice(_offset), (ushort)x.Length);
            _offset += 2;

            var span = _buffer.Span;
            for (var i = 0; i < x.Length; i++)
                span[_offset++] = (byte)x[i];
        }

        public void WriteBytes(ReadOnlySpan<byte> x)
        {
            Reserve(x.Length);
            x.CopyTo(_buffer.Span.Slice(_offset));
            _offset += x.Length;
        }

        public void WriteBlob(ReadOnlySpan<byte> x)
        {
            Reserve(x.Length + 2);
            BinaryPrimitives.WriteUInt16LittleEndian(_buffer.Span.Slice(_offset), (ushort)x.Length);
            x.CopyTo(_buffer.Span.Slice(_offset + 2));
            _offset += x.Length + 2;
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
            output(slice.Span, state);
        }

        public void PadBytes(int n)
        {
            Reserve(n);
            _offset += n;
        }

        public ReadOnlyMemory<byte> Data => _buffer.Slice(0, _offset);

        public int Size => _offset;
    }
}
