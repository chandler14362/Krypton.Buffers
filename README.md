# Krypton.Buffers

## Types

The library ships with four types: 
 * 2 Readers: SpanBufferReader and MemoryBufferReader (both sharing the same API)
 * 2 Writers: GrowingSpanBuffer and GrowingMemoryBuffer (both sharing the same API)

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
 * String8 (string in utf8)
 * Bytes (ReadOnlySpan\<byte\>/ReadOnlyMemory\<byte\>)
 * Guid (System.Guid)

There is a corresponding Read/Write method for each. Data is written in little endian

## Features

### Safe Allocation Free Buffer Writing with GrowingSpanBuffer
Example 1:
```cs
var buf = new GrowingSpanBuffer(stackalloc byte[64]); // initial buffer exists on the stack
buf.WriteUInt64(0);
buf.WriteString8("test");
Socket.Write(buf.Data);
```

Example 2:
```cs
var buf = new GrowingSpanBuffer(stackalloc byte[8]); // initial buffer exists on the stack
buf.WriteUInt64(0);
buf.WriteUInt64(0); // we resize on the heap here
Socket.Write(buf.Data);
```

### Writer Bookmarks

Bookmarks are used for reserving a set number of bytes and writing to them later

Example:
```cs
var buf = new GrowingSpanBuffer(stackalloc byte[64]);

// strs is an IEnumerable<string>. Lets write the count after we enumerate through it
ushort count = 0;
var countBookmark = buf.ReserveBookmark(sizeof(ushort));
foreach (var str in strs)
{
    buf.WriteString8(str);
    count += 1;
}

// Now we can write the count
buf.WriteBookmark(countBookmark, count, BinaryPrimitives.WriteUInt16LittleEndian);
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
var buf = new SpanBufferReader(data);

// Read 6 uint32s from the buffer
ReadOnlySpan<uint> ids = buf.ReadUInt32Slice(6);
```
