﻿using System;
using System.Buffers;
using System.Buffers.Binary;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

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

        private byte[] _pooledBuffer;

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
            _pooledBuffer = null;
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
            _buffer = _pooledBuffer.AsSpan();
            _offset = 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void Reserve(int length)
        {
            var neededLength = _offset + length;
            if (neededLength <= _buffer.Length)
                return;
            ResizeBuffer(neededLength);
        }

        private void ResizeBuffer(int neededLength)
        {
            // If we can't resize we need to let the user know we are out of space
            if (!_resize)
                throw new OutOfSpaceException(_buffer.Length, _offset, neededLength);

            var resized = _poolingStrategy.Resize(_buffer.Length, neededLength);
            _buffer.CopyTo(resized.AsSpan());
            if (_pooledBuffer != null)
                _poolingStrategy.Free(_pooledBuffer);
            _pooledBuffer = resized;
            _buffer = resized.AsSpan();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public SpanBufferWriter Start()
        {
            _offset = 0;
            return this;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public SpanBufferWriter WriteBool(bool x)
        {
            Reserve(1);
            _buffer[_offset++] = x ? (byte)1 : (byte)0;
            return this;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public SpanBufferWriter WriteInt8(sbyte x)
        {
            Reserve(1);
            _buffer[_offset++] = (byte)x;
            return this;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public SpanBufferWriter WriteUInt8(byte x)
        {
            Reserve(1);
            _buffer[_offset++] = x;
            return this;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public SpanBufferWriter WriteInt16(short x)
        {
            const int size = sizeof(short);
            Reserve(size);
            BinaryPrimitives.WriteInt16LittleEndian(_buffer.Slice(_offset), x);
            _offset += size;
            return this;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public SpanBufferWriter WriteUInt16(ushort x)
        {
            const int size = sizeof(ushort);
            Reserve(size);
            BinaryPrimitives.WriteUInt16LittleEndian(_buffer.Slice(_offset), x);
            _offset += size;
            return this;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public SpanBufferWriter WriteInt32(int x)
        {
            const int size = sizeof(int);
            Reserve(size);
            BinaryPrimitives.WriteInt32LittleEndian(_buffer.Slice(_offset), x);
            _offset += size;
            return this;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public SpanBufferWriter WriteUInt32(uint x)
        {
            const int size = sizeof(uint);
            Reserve(size);
            BinaryPrimitives.WriteUInt32LittleEndian(_buffer.Slice(_offset), x);
            _offset += size;
            return this;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public SpanBufferWriter WriteUInt64(ulong x)
        {
            const int size = sizeof(ulong);
            Reserve(size);
            BinaryPrimitives.WriteUInt64LittleEndian(_buffer.Slice(_offset), x);
            _offset += size;
            return this;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public SpanBufferWriter WriteInt64(long x)
        {
            const int size = sizeof(long);
            Reserve(size);
            BinaryPrimitives.WriteInt64LittleEndian(_buffer.Slice(_offset), x);
            _offset += size;
            return this;
        }

        public SpanBufferWriter WriteFloat32(float x)
        {
            // TODO: big endian support
            if (!BitConverter.IsLittleEndian)
                throw new NotImplementedException();
            
            const int size = sizeof(float);
            Reserve(size);
            MemoryMarshal.Write(_buffer.Slice(_offset), ref x);
            _offset += size;
            return this;
        }

        public SpanBufferWriter WriteFloat64(double x)
        {
            // TODO: big endian support
            if (!BitConverter.IsLittleEndian)
                throw new NotImplementedException();
            
            const int size = sizeof(double);
            Reserve(size);            
            MemoryMarshal.Write(_buffer.Slice(_offset), ref x);
            _offset += size;
            return this;
        }

        public SpanBufferWriter WriteGuid(Guid guid)
        {
            const int size = 16;
            Reserve(size);
#if NETSTANDARD2_1
            _ = guid.TryWriteBytes(_buffer.Slice(_offset));
#else
            guid.ToByteArray().AsSpan().CopyTo(_buffer.Slice(_offset, size));
#endif
            _offset += size;
            return this;
        }

        public SpanBufferWriter WriteString(string str, Encoding encoding)
        {
            var byteCount = encoding.GetByteCount(str);
            
            Reserve(byteCount + 2);
            BinaryPrimitives.WriteUInt16LittleEndian(_buffer.Slice(_offset), (ushort)byteCount);
            _offset += 2;
            
            var bytes = _buffer.Slice(_offset, byteCount);
#if NETSTANDARD2_1
            encoding.GetBytes(str.AsSpan(), bytes);
#else
            encoding.GetBytes(str).AsSpan().CopyTo(bytes);
#endif
            _offset += byteCount;
            return this;
        }
        
        public SpanBufferWriter WriteUTF8String(string str)
            => WriteString(str, Encoding.UTF8);
        
        public SpanBufferWriter WriteUTF16String(string str)
            => WriteString(str, Encoding.Unicode);

        public SpanBufferWriter WriteBytes(ReadOnlySpan<byte> x)
        {
            Reserve(x.Length);
            x.CopyTo(_buffer.Slice(_offset));
            _offset += x.Length;
            return this;
        }

        public Bookmark ReserveBookmark(int size)
        {
            Reserve(size);
            var bookmark = new Bookmark(_offset, size);
            _offset += size;
            return bookmark;
        }

        public SpanBufferWriter WriteBookmark<TState>(in Bookmark bookmark, TState state, SpanAction<byte, TState> output)
        {
            var slice = _buffer.Slice(bookmark.Offset, bookmark.Size);
            output(slice, state);
            return this;
        }

        public SpanBufferWriter PadBytes(int n)
        {
            Reserve(n);
            _offset += n;
            return this;
        }

        public void Dispose()
        {
            if (_pooledBuffer == null)
                return;

            _poolingStrategy.Free(_pooledBuffer);
            _pooledBuffer = null;
            _buffer = Span<byte>.Empty;
            _offset = 0;
        }

        public ReadOnlySpan<byte> Data => _buffer.Slice(0, _offset);

        public int Size => _offset;

        public static implicit operator ReadOnlySpan<byte>(SpanBufferWriter buffer) => buffer.Data;
    }
}
