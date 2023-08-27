using System;
using System.IO;
using Cosmos.Core;
using Cosmos.System.IO;

namespace Cosmos.System
{
    public partial class Console
    {
        internal class CosmosConsoleStream : ConsoleStream
        {
            internal CosmosConsoleStream(FileAccess access) : base(access)
            {
            }

            public override void Flush() {}
            public override void Write(ReadOnlySpan<byte> buffer)
            {
                Global.Console.Write(buffer.ToArray());
            }

            public override int Read(Span<byte> buffer) => Global.Console.StdInReader.ReadLine(buffer);
        }

        private int Read(Span<byte> buffer) => throw new NotImplementedException();
    }
}

