using System;
using System.Buffers;
using System.Buffers.Binary;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Krypton.Buffers
{
    public ref struct SpanBufferWriter
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

        private readonly bool _resize;
        
        private readonly IPoolingStrategy _poolingStrategy;

        private Memory<byte> _pooledBuffer;

        private Span<byte> _buffer;

        private int _offset;

        /// <summary>
        /// Creates a new SpanBufferWriter that is based off an existing buffer
        /// </summary>
        /// <param name="buffer">The buffer</param>
        /// <param name="resize">If the buffer can resize</param>
        /// <param name="poolingStrategy">The pooling strategy used when resizing the buffer</param>
        public SpanBufferWriter(Span<byte> buffer, bool resize = true, IPoolingStrategy poolingStrategy = null)
        {
            _resize = resize;
            _poolingStrategy = poolingStrategy ?? DefaultPoolingStrategy.Instance;
            _pooledBuffer = Memory<byte>.Empty;
            _buffer = buffer;
            _offset = 0;
        }

        /// <summary>
        /// Creates a new SpanBufferWriter that allocates its initial buffer from a pool
        /// </summary>
        /// <param name="size">The initial buffer size</param>
        /// <param name="resize">If the buffer can resize</param>
        /// <param name="poolingStrategy">The pooling strategy used when resizing the buffer</param>
        public SpanBufferWriter(int size, bool resize = true, IPoolingStrategy poolingStrategy = null)
        {
            _resize = resize;
            _poolingStrategy = poolingStrategy ?? DefaultPoolingStrategy.Instance;
            _pooledBuffer = _poolingStrategy.Resize(1, size);
            _buffer = _pooledBuffer.Span;
            _offset = 0;
        }

        private void Reserve(int length)
        {
            if (_offset + length < _buffer.Length)
                return;
            
            // If we can't resize we need to let the user know we are out of space
            if (!_resize)
                throw new OutOfSpaceException(_buffer.Length, _offset, _offset + length);

            var resized = _poolingStrategy.Resize(_buffer.Length, _offset + length);
            _buffer.CopyTo(resized.Span);
            if (!_pooledBuffer.IsEmpty)
                _poolingStrategy.Free(_pooledBuffer);
            _pooledBuffer = resized;
            _buffer = resized.Span;
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

        public void WriteGuid(Guid guid)
        {
            const int size = 16;
            Reserve(16);
            _ = guid.TryWriteBytes(_buffer.Slice(_offset));
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

        public void Dispose()
        {
            if (_pooledBuffer.IsEmpty)
                return;

            _poolingStrategy.Free(_pooledBuffer);
            _pooledBuffer = Memory<byte>.Empty;
            _buffer = Span<byte>.Empty;
            _offset = 0;
        }

        public ReadOnlySpan<byte> Data => _buffer.Slice(0, _offset);

        public int Size => _offset;

        public static implicit operator ReadOnlySpan<byte>(SpanBufferWriter buffer) => buffer.Data;
    }
}
