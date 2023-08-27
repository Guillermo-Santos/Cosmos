using System;
using System.IO;

#nullable enable
namespace Cosmos.System.IO
{
    public sealed partial class SyncTextReader : TextReader
    {
        public bool KeyAvailable => KeyboardManager.KeyAvailable && Global.Console is not null;
        public bool IsStdIn => Inner is not null;
        internal StdInReader? Inner => _in as StdInReader;
        public ConsoleKeyInfo ReadKey(out bool previouslyProcessed) => Inner!.ReadKey(out previouslyProcessed);
        public int ReadLine(Span<byte> buffer) => Inner!.ReadLine(buffer);
    }
}
