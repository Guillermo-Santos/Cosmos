using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.


#nullable enable
namespace Cosmos.System.IO
{
    // Heavily modified to be used on cosmos
    internal sealed class StdInReader : TextReader
    {
        private readonly StringBuilder _readLineSB; // SB that holds readLine output.  This is a field simply to enable reuse; it's only used in ReadLine.
        private readonly Stack<KeyEvent> _tmpKeys = new Stack<KeyEvent>(); // temporary working stack; should be empty outside of ReadLine
        private readonly Stack<KeyEvent> _availableKeys = new Stack<KeyEvent>(); // a queue of already processed key infos available for reading
        private readonly Encoding encoding;
        private const int BytesToBeRead = 1024; // No. of bytes to be read from the stream at a time.
        private List<string> _keys;
        internal StdInReader(Encoding encoding)
        {
            _readLineSB = new StringBuilder();
            //_keys = new List<string>(BytesToBeRead); 
            this.encoding = encoding;
        }

        /// <summary>
        /// Try to intercept the key pressed.
        /// </summary>
        public ConsoleKeyInfo ReadKey(out bool previouslyProcessed)
        {
            if (_availableKeys.Count > 0)
            {
                previouslyProcessed = true;
                var key = _availableKeys.Pop();

                bool xShift = (key.Modifiers & ConsoleModifiers.Shift) == ConsoleModifiers.Shift;
                bool xAlt = (key.Modifiers & ConsoleModifiers.Alt) == ConsoleModifiers.Alt;
                bool xControl = (key.Modifiers & ConsoleModifiers.Control) == ConsoleModifiers.Control;

                return new ConsoleKeyInfo(key.KeyChar, key.Key.ToConsoleKey(), xShift, xAlt, xControl);
            }

            previouslyProcessed = false;
            return ReadKey();
        }
        private static ConsoleKeyInfo ReadKey()
        {
            var key = KeyboardManager.ReadKey();

            //TODO: Plug HasFlag and use the next 3 lines instead of the 3 following lines

            //bool xShift = key.Modifiers.HasFlag(ConsoleModifiers.Shift);
            //bool xAlt = key.Modifiers.HasFlag(ConsoleModifiers.Alt);
            //bool xControl = key.Modifiers.HasFlag(ConsoleModifiers.Control);

            bool xShift = (key.Modifiers & ConsoleModifiers.Shift) == ConsoleModifiers.Shift;
            bool xAlt = (key.Modifiers & ConsoleModifiers.Alt) == ConsoleModifiers.Alt;
            bool xControl = (key.Modifiers & ConsoleModifiers.Control) == ConsoleModifiers.Control;

            return new ConsoleKeyInfo(key.KeyChar, key.Key.ToConsoleKey(), xShift, xAlt, xControl);
        }

        public override string? ReadLine()
        {
            bool isEnter = ReadLineCore(consumeKeys: true);
            string? line = null;
            if (isEnter || _readLineSB.Length > 0)
            {
                line = _readLineSB.ToString();
                _readLineSB.Clear();
            }
            return line;
        }

        public bool ReadLineCore(bool consumeKeys)
        {

            // _availableKeys either contains a line that was already read,
            // or we need to read a new line from stdin.
            bool freshKeys = _availableKeys.Count == 0;

            // Don't carry over chars from previous ReadLine call.
            _readLineSB.Clear();
            KeyEvent current;
            int currentCount = 0;

            try
            {
                while (true)
                {
                    current = freshKeys ? KeyboardManager.ReadKey() : _availableKeys.Pop();

                    if (!consumeKeys && current.Key != ConsoleKeyEx.Backspace) // backspace is the only character not written out below.
                    {
                        _tmpKeys.Push(current);
                    }

                    if (current.Key is ConsoleKeyEx.Enter or ConsoleKeyEx.NumEnter)
                    {
                        if (freshKeys)
                        {
                            global::System.Console.WriteLine();
                        }
                        return true;
                    }

                    //Check for "special" keys
                    switch (current.Key)
                    {
                        case ConsoleKeyEx.Backspace:
                            {
                                bool removed = false;
                                if (consumeKeys)
                                {
                                    int len = _readLineSB.Length;
                                    if (len > 0)
                                    {
                                        removed = true;
                                    }
                                }
                                else
                                {
                                    removed = _tmpKeys.TryPop(out _);
                                }

                                if (removed && freshKeys && currentCount > 0)
                                {
                                    currentCount--;
                                    _readLineSB.Remove(currentCount, 1);
                                    //_readLineSB[currentCount] = '\0';
                                    var tempX = Global.Console.X;
                                    Global.Console.X--;
                                    //Move characters to the left
                                    for (int x = currentCount; x < _readLineSB.Length; x++)
                                    {
                                        global::System.Console.Write(_readLineSB[x]);
                                    }

                                    global::System.Console.Write('\0');

                                    Global.Console.X = tempX - 1;
                                }
                                continue;
                            }

                        case ConsoleKeyEx.LeftArrow:
                            if (currentCount > 0)
                            {
                                Global.Console.X--;
                                currentCount--;
                            }
                            continue;
                        case ConsoleKeyEx.RightArrow:
                            if (currentCount < _readLineSB.Length)
                            {
                                Global.Console.X++;
                                currentCount++;
                            }
                            continue;
                    }

                    if (current.KeyChar == '\0')
                    {
                        continue;
                    }

                    //Write the character to the screen
                    if (currentCount == _readLineSB.Length)
                    {
                        _readLineSB.Append(current.KeyChar);
                        global::System.Console.Write(current.KeyChar);
                        currentCount++;
                    }
                    else
                    {
                        _readLineSB.Insert(currentCount, current.KeyChar);

                        //Shift the characters to the right
                        for (int x = currentCount; x < _readLineSB.Length; x++)
                        {
                            global::System.Console.Write(_readLineSB[x]);
                        }

                        Global.Console.X -= _readLineSB.Length - currentCount - 1;
                        currentCount++;
                    }
                }
            }
            finally
            {
                // If we're not consuming the read input, make the keys available for a future read
                while (_tmpKeys.Count > 0)
                {
                    _availableKeys.Push(_tmpKeys.Pop());
                }
            }
        }
        public override int Read()
        {
            if (KeyboardManager.TryReadKey(out KeyEvent result))
            {
                return result.KeyChar;
            }
            else
            {
                return -1;
            }
        }

        public override int Peek()
        {
            // If there aren't any keys in our processed keys stack, read a line to populate it.
            if (_availableKeys.Count == 0)
            {
                ReadLineCore(consumeKeys: false);
            }

            // Now if there are keys, use the first.
            if (_availableKeys.Count > 0)
            {
                KeyEvent keyInfo = _availableKeys.Peek();
                if (!IsEol(keyInfo.KeyChar))
                {
                    return keyInfo.KeyChar;
                }
            }

            // EOL
            return -1;
        }

        private static bool IsEol(char keyChar) => keyChar != '\0';
        internal int ReadLine(Span<byte> buffer)
        {

            if (buffer.IsEmpty)
            {
                return 0;
            }

            // Don't read a new line if there are remaining characters in the StringBuilder.
            if (_readLineSB.Length == 0)
            {
                bool isEnter = ReadLineCore(consumeKeys: true);
                if (isEnter)
                {
                    _readLineSB.Append('\n');
                }
            }

            // Encode line into buffer.
            Encoder encoder = encoding.GetEncoder();
            int bytesUsedTotal = 0;
            int charsUsedTotal = 0;
            foreach (ReadOnlyMemory<char> chunk in _readLineSB.GetChunks())
            {
                encoder.Convert(chunk.Span, buffer, flush: false, out int charsUsed, out int bytesUsed, out bool completed);
                buffer = buffer.Slice(bytesUsed);
                bytesUsedTotal += bytesUsed;
                charsUsedTotal += charsUsed;

                if (!completed || buffer.IsEmpty)
                {
                    break;
                }
            }
            _readLineSB.Remove(0, charsUsedTotal);
            return bytesUsedTotal;
        }

        public static bool StdinReady => Global.Console is not null;
    }
}