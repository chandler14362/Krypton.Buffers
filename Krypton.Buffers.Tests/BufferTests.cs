using System;
using System.Text;
using NUnit.Framework;
using Krypton.Buffers;

namespace Krypton.Buffers.Tests
{
    public class BufferTests
    {
        [Test]
        public void TestWriteFixedSizeBuffer()
        {
            using var bufferWriter = new SpanBufferWriter(stackalloc byte[4], resize: false);
            bufferWriter.WriteUInt32(4);
            
            try
            {
                bufferWriter.WriteString("ahhh", Encoding.UTF8);
            }
            catch (OutOfSpaceException)
            {
                Assert.Pass();
                return;
            }
            Assert.Fail();
        }

        [Test]
        public void TestReadEndOfBuffer()
        {
            using var bufferWriter = new SpanBufferWriter(stackalloc byte[64]);
            bufferWriter.WriteString("test", Encoding.UTF8);
            
            var bufferReader = new SpanBufferReader(bufferWriter);
            _ = bufferReader.ReadString(Encoding.UTF8);
            
            try
            {
                bufferReader.ReadByte();
            }
            catch (EndOfBufferException)
            {
                Assert.Pass();
                return;
            }
            Assert.Fail();
        }
    }
}
