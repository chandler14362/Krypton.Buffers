# Krypton.Buffers

## Types

The library ships with four types: 
 * 2 Readers: SpanBufferReader and MemoryBufferReader (both sharing the same API)
 * 2 Writers: SpanBufferWriter and MemoryBufferWriter (both sharing the same API)

Currently the readers/writers support the following types:

 * Bool (bool stored as a byte)
 * Int8 (byte)
 * UInt8 (sbyte)
 * Int16 (short)
 * UInt16 (ushort)
 * Int32 (int)
 * UInt32 (uint)
 * Int64 (long)
 * UInt64 (ulong)
 * Float32 (float)
 * Float64 (double)
 * String (any encoding, UTF8 and UTF16 helpers, shown in examples)
 * Bytes (ReadOnlySpan\<byte\>/ReadOnlyMemory\<byte\>)
 * Guid (System.Guid)

There is a corresponding Read/Write method for each. Data is written in little endian

## Buffer Options

By default all buffers are set to resize. The default pooling strategy is no pooling at all, each time the writer needs to resize a new byte array is allocated.
If you want to write to a fixed size buffer without any resizing you can do so.

```cs
try 
{
    using var bufferWriter = new SpanBufferWriter(someFixedSizeBuffer, resize: false);
    bufferWriter.WriteUTF8String("I hope there is enough space for this");
}
catch (OutOfSpaceException)
{
    // Looks like there wasnt...
}
```

There is also an exception you can handle when reading from a buffer

```cs
try 
{
    var bufferReader = new SpanBufferReader(someBuffer);
    var str = bufferReader.ReadString(Encoding.Unicode);
    var randomBytes = bufferReader.ReadBytes(462); // I hope there are 462 bytes to read
}
catch (EndOfBufferException)
{
    // Looks like there wasnt...
}
```

There is more info on the pooling strategies below.

## Features

### Safe Allocation Free Buffer Writing with SpanBufferWriter
Example 1:
```cs
using var bufferWriter = new SpanBufferWriter(stackalloc byte[64]); // initial buffer exists on the stack
bufferWriter.WriteUInt64(0);
bufferWriter.WriteUTF8String("test");
Socket.Write(bufferWriter.Data);
```

Example 2:
```cs
using var bufferWriter = new SpanBufferWriter(stackalloc byte[8]); // initial buffer exists on the stack
bufferWriter.WriteUInt64(0);
bufferWriter.WriteUInt64(0); // we resize on the heap here
Socket.Write(bufferWriter); // implicit ReadOnlySpan<byte> cast
```

### Writer Bookmarks

Bookmarks are used for reserving a set number of bytes and writing to them later

Example:
```cs
using var bufferWriter = new SpanBufferWriter(stackalloc byte[64]);

// strs is an IEnumerable<string>. Lets write the count after we enumerate through it
ushort count = 0;
var countBookmark = bufferWriter.ReserveBookmark(sizeof(ushort));
foreach (var str in strs)
{
    bufferWriter.WriteString(str, Encoding.Unicode);
    count += 1;
}

// Now we can write the count
bufferWriter.WriteBookmark(countBookmark, count, BinaryPrimitives.WriteUInt16LittleEndian);
```

### Reading Int Slices

The readers support reading slices of the following types:

 * Int16
 * UInt16
 * Int32
 * UInt32
 * Int64
 * UInt64

There is a Read{type}Slice method for each.

This method is allocation free on little endian machines.

Example:
```cs
var bufferReader = new SpanBufferReader(data);

// Read 6 uint32s from the buffer
ReadOnlySpan<uint> ids = bufferReader.ReadUInt32Slice(6);
```

### Configurable Array Pooling

Here is an example implementation of a pooling strategy that will use ArrayPool whenever the buffer writer needs to resize.

```cs
public class ExamplePoolingStrategy : IPoolingStrategy
{
    public const int GrowthFactor = 2;

    public static readonly IPoolingStrategy Instance = new ExamplePoolingStrategy();
    
    private readonly ArrayPool<byte> _arrayPool = = ArrayPool<byte>.Create();

    private ExamplePoolingStrategy()
    {
    }
    
    // This gets called whenever the writer needs to resize
    public byte[] Resize(int size, int neededSize)
    {
        var newLength = size * GrowthFactor;
        while (neededSize > newLength)
            newLength *= GrowthFactor;
        return _arrayPool.Rent(newLength);
    }

    // This gets called whenever the writer is done with the array it rented
    public void Free(byte[] rented)
    {
        _arrayPool.Return(rented);
    }
}

using var bufferWriter = new SpanBufferWriter(stackalloc byte[4], poolingStrategy: ExamplePoolingStrategy.Instance);
bufferWriter.WriteUInt32(4); // this gets written to the initial buffer that exists on the stack
bufferWriter.WriteUTF16String("hello heap!"); // the buffer resizes using ArrayPool and the data gets relocated on to the heap
```
