using System.IO;

namespace Cosmos.System.IO
{
    public sealed partial class SyncTextReader : TextReader
    {
        internal readonly TextReader _in;

        internal SyncTextReader(TextReader t)
        {
            _in = t;
        }

        public override int Peek() => _in.Peek();
        public override int Read() => _in.Read();
        public override int Read(char[] buffer, int index, int count) => _in.Read(buffer, index, count);
        public override int ReadBlock(char[] buffer, int index, int count) => _in.ReadBlock(buffer, index, count);
        public override string? ReadLine() => _in.ReadLine();
        public override string ReadToEnd() => _in.ReadToEnd();
        public static SyncTextReader GetSynchronizedTextReader(TextReader reader)
        {
            return reader as SyncTextReader ??
                new SyncTextReader(reader);
        }
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _in.Dispose();
            }
        }
    }
}
