﻿using System;
using System.Buffers;
using System.Buffers.Binary;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace Krypton.Buffers
{
    public class MemoryBufferWriter : IDisposable
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
        
        private Memory<byte> _buffer;

        private int _offset;

        /// <summary>
        /// Creates a new MemoryBufferWriter that is based off an existing buffer
        /// </summary>
        /// <param name="buffer">The buffer</param>
        /// <param name="resize">If the buffer can resize</param>
        /// <param name="poolingStrategy">The pooling strategy used when resizing the buffer</param>
        public MemoryBufferWriter(Memory<byte> buffer, bool resize = true, IPoolingStrategy poolingStrategy = null)
        {
            _resize = resize;
            _poolingStrategy = poolingStrategy ?? DefaultPoolingStrategy.Instance;
            _pooledBuffer = null;
            _buffer = buffer;
            _offset = 0;
        }

        /// <summary>
        /// Creates a new MemoryBufferWriter that allocates its initial buffer from a pool
        /// </summary>
        /// <param name="size">The initial buffer size</param>
        /// <param name="resize">If the buffer can resize</param>
        /// <param name="poolingStrategy">The pooling strategy used when resizing the buffer</param>
        public MemoryBufferWriter(int size, bool resize = true, IPoolingStrategy poolingStrategy = null)
        {
            _resize = resize;
            _poolingStrategy = poolingStrategy ?? DefaultPoolingStrategy.Instance;
            _pooledBuffer = _poolingStrategy.Resize(1, size);
            _buffer = _pooledBuffer;
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
            _buffer.CopyTo(resized);
            if (_pooledBuffer != null)
                _poolingStrategy.Free(_pooledBuffer);
            _pooledBuffer = resized;
            _buffer = resized;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public MemoryBufferWriter Start()
        {
            _offset = 0;
            return this;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public MemoryBufferWriter WriteBool(bool x)
        {
            Reserve(1);
            _buffer.Span[_offset++] = x ? (byte)1 : (byte)0;
            return this;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public MemoryBufferWriter WriteInt8(sbyte x)
        {
            Reserve(1);
            _buffer.Span[_offset++] = (byte)x;
            return this;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public MemoryBufferWriter WriteUInt8(byte x)
        {
            Reserve(1);
            _buffer.Span[_offset++] = x;
            return this;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public MemoryBufferWriter WriteInt16(short x)
        {
            const int size = sizeof(short);
            Reserve(size);
            BinaryPrimitives.WriteInt16LittleEndian(_buffer.Span.Slice(_offset), x);
            _offset += size;
            return this;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public MemoryBufferWriter WriteUInt16(ushort x)
        {
            const int size = sizeof(ushort);
            Reserve(size);
            BinaryPrimitives.WriteUInt16LittleEndian(_buffer.Span.Slice(_offset), x);
            _offset += size;
            return this;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public MemoryBufferWriter WriteInt32(int x)
        {
            const int size = sizeof(int);
            Reserve(size);
            BinaryPrimitives.WriteInt32LittleEndian(_buffer.Span.Slice(_offset), x);
            _offset += size;
            return this;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public MemoryBufferWriter WriteUInt32(uint x)
        {
            const int size = sizeof(uint);
            Reserve(size);
            BinaryPrimitives.WriteUInt32LittleEndian(_buffer.Span.Slice(_offset), x);
            _offset += size;
            return this;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public MemoryBufferWriter WriteUInt64(ulong x)
        {
            const int size = sizeof(ulong);
            Reserve(size);
            BinaryPrimitives.WriteUInt64LittleEndian(_buffer.Span.Slice(_offset), x);
            _offset += size;
            return this;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public MemoryBufferWriter WriteInt64(long x)
        {
            const int size = sizeof(long);
            Reserve(size);
            BinaryPrimitives.WriteInt64LittleEndian(_buffer.Span.Slice(_offset), x);
            _offset += size;
            return this;
        }

        public MemoryBufferWriter WriteFloat32(float x)
        {
            // TODO: big endian support
            if (!BitConverter.IsLittleEndian)
                throw new NotImplementedException();
            
            const int size = sizeof(float);
            Reserve(size);
            MemoryMarshal.Write(_buffer.Span.Slice(_offset), ref x);
            _offset += size;
            return this;
        }

        public MemoryBufferWriter WriteFloat64(double x)
        {
            // TODO: big endian support
            if (!BitConverter.IsLittleEndian)
                throw new NotImplementedException();
            
            const int size = sizeof(double);
            Reserve(size);            
            MemoryMarshal.Write(_buffer.Span.Slice(_offset), ref x);
            _offset += size;
            return this;
        }
        
        public MemoryBufferWriter WriteGuid(Guid guid)
        {
            const int size = 16;
            Reserve(16);
#if NETSTANDARD2_1
            _ = guid.TryWriteBytes(_buffer.Slice(_offset).Span);
#else
            guid.ToByteArray().AsSpan().CopyTo(_buffer.Slice(_offset, size).Span);
#endif
            _offset += size;
            return this;
        }

        public MemoryBufferWriter WriteString(string str, Encoding encoding)
        {
            var byteCount = encoding.GetByteCount(str);
            
            Reserve(byteCount + 2);
            BinaryPrimitives.WriteUInt16LittleEndian(_buffer.Slice(_offset).Span, (ushort)byteCount);
            _offset += 2;

            var bytes = _buffer.Slice(_offset, byteCount).Span;
#if NETSTANDARD2_1
            encoding.GetBytes(str.AsSpan(), bytes);
#else
            encoding.GetBytes(str).AsSpan().CopyTo(bytes);
#endif
            _offset += byteCount;
            return this;
        }
        
        public MemoryBufferWriter WriteUTF8String(string str)
            => WriteString(str, Encoding.UTF8);
        
        public MemoryBufferWriter WriteUTF16String(string str)
            => WriteString(str, Encoding.Unicode);

        public MemoryBufferWriter WriteBytes(ReadOnlySpan<byte> x)
        {
            Reserve(x.Length);
            x.CopyTo(_buffer.Span.Slice(_offset));
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

        public MemoryBufferWriter WriteBookmark<TState>(in Bookmark bookmark, TState state, SpanAction<byte, TState> output)
        {
            var slice = _buffer.Slice(bookmark.Offset, bookmark.Size);
            output(slice.Span, state);
            return this;
        }

        public MemoryBufferWriter PadBytes(int n)
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
            _buffer = Memory<byte>.Empty;
            _offset = 0;
        }
        
        public ReadOnlyMemory<byte> Data => _buffer.Slice(0, _offset);

        public int Size => _offset;

        public static implicit operator ReadOnlyMemory<byte>(MemoryBufferWriter buffer) => buffer.Data;

        public static implicit operator ReadOnlySpan<byte>(MemoryBufferWriter buffer) => buffer.Data.Span;
    }
}
