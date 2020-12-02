using System;
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

        private Memory<byte> _pooledBuffer;
        
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
            _pooledBuffer = Memory<byte>.Empty;
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

        private void Reserve(int length)
        {
            if (_offset + length < _buffer.Length)
                return;
            
            // If we can't resize we need to let the user know we are out of space
            if (!_resize)
                throw new OutOfSpaceException(_buffer.Length, _offset, _offset + length);

            var resized = _poolingStrategy.Resize(_buffer.Length, _offset + length);
            _buffer.CopyTo(resized);
            if (!_pooledBuffer.IsEmpty)
                _poolingStrategy.Free(_pooledBuffer);
            _pooledBuffer = resized;
            _buffer = resized;
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
            // TODO: big endian support
            if (!BitConverter.IsLittleEndian)
                throw new NotImplementedException();
            
            const int size = sizeof(float);
            Reserve(size);
            MemoryMarshal.Write(_buffer.Span.Slice(_offset), ref x);
            _offset += size;
        }

        public void WriteFloat64(double x)
        {
            // TODO: big endian support
            if (!BitConverter.IsLittleEndian)
                throw new NotImplementedException();
            
            const int size = sizeof(double);
            Reserve(size);            
            MemoryMarshal.Write(_buffer.Span.Slice(_offset), ref x);
            _offset += size;
        }
        
        public void WriteGuid(Guid guid)
        {
            const int size = 16;
            Reserve(16);
            _ = guid.TryWriteBytes(_buffer.Slice(_offset).Span);
            _offset += size;
        }

        public void WriteString(string str, Encoding encoding)
        {
            var byteCount = encoding.GetByteCount(str);
            
            Reserve(byteCount + 2);
            BinaryPrimitives.WriteUInt16LittleEndian(_buffer.Slice(_offset).Span, (ushort)byteCount);
            _offset += 2;
            
            var bytes = byteCount < 512 ? stackalloc byte[byteCount] : new byte[byteCount];
            encoding.GetBytes(str.AsSpan(), bytes);
            bytes.CopyTo(_buffer.Slice(_offset, byteCount).Span);
            _offset += byteCount;
        }
        
        public void WriteUTF8String(string str)
            => WriteString(str, Encoding.UTF8);
        
        public void WriteUTF16String(string str)
            => WriteString(str, Encoding.Unicode);

        public void WriteBytes(ReadOnlySpan<byte> x)
        {
            Reserve(x.Length);
            x.CopyTo(_buffer.Span.Slice(_offset));
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
            output(slice.Span, state);
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
            _buffer = Memory<byte>.Empty;
            _offset = 0;
        }
        
        public ReadOnlyMemory<byte> Data => _buffer.Slice(0, _offset);

        public int Size => _offset;

        public static implicit operator ReadOnlyMemory<byte>(MemoryBufferWriter buffer) => buffer.Data;

        public static implicit operator ReadOnlySpan<byte>(MemoryBufferWriter buffer) => buffer.Data.Span;
    }
}
