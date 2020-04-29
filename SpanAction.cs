using System;

namespace Krypton.Buffers
{
    public delegate void SpanAction<T, in TArg>(Span<T> span, TArg arg);
}
