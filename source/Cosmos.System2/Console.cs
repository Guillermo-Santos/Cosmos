﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using Cosmos.HAL;
using Cosmos.System.IO;
using static Cosmos.System.Console;

namespace Cosmos.System
{
    /// <summary>
    /// Represents the standard console output stream.
    /// </summary>
    public partial class Console
    {
        private const byte LineFeed = (byte)'\n';
        private const byte CarriageReturn = (byte)'\r';
        private const byte Tab = (byte)'\t';
        private const byte Space = (byte)' ';
        private const int ReadBufferSize = 4096;
        private const int WriteBufferSize = 256;

        private SyncTextReader _stdInReader;

        /// <summary>
        /// The underlying X cursor location field.
        /// </summary>
        protected int mX = 0;

        /// <summary>
        /// The text cursor location in the X (horizontal) axis.
        /// </summary>
        public int X
        {
            get => mX;
            set
            {
                mX = value;
                UpdateCursor();
            }
        }

        /// <summary>
        /// The underlying Y cursor location field.
        /// </summary>
        protected int mY = 0;

        /// <summary>
        /// Get and set cursor location on Y axis.
        /// </summary>
        public int Y
        {
            get => mY;
            set
            {
                mY = value;
                UpdateCursor();
            }
        }

        /// <summary>
        /// Get window width.
        /// </summary>
        public int Cols
        {
            get => mText.Cols;
            set { }
        }

        /// <summary>
        /// Get window height.
        /// </summary>
        public int Rows
        {
            set { }
            get => mText.Rows;
        }

        /// <summary>
        /// Text screen.
        /// </summary>
        public HAL.TextScreenBase mText;

        /// <summary>
        /// Constructs a new instance of the <see cref="Console"/> class.
        /// </summary>
        /// <param name="textScreen">The device to direct text output to.</param>
        public Console(TextScreenBase textScreen)
        {
            if (textScreen == null)
            {
                mText = new TextScreen();
            }
            else
            {
                mText = textScreen;
            }
        }

        /// <summary>
        /// Clears the console, and changes the cursor location to (0, 0).
        /// </summary>
        public void Clear()
        {
            if (!IsStdOutRedirected())
            {
                mText.Clear();
                mX = 0;
                mY = 0;
                UpdateCursor();
            }
        }

        //TODO: This is slow, batch it and only do it at end of updates
        /// <summary>
        /// Update cursor position.
        /// </summary>
        protected void UpdateCursor()
        {
            mText.SetCursorPos(mX, mY);
        }

        /// <summary>
        /// Scrolls the console up and moves the cursor to the start of the line.
        /// </summary>
        private void DoLineFeed()
        {
            mY++;
            mX = 0;
            if (mY == mText.Rows)
            {
                mText.ScrollUp();
                mY--;
            }
            UpdateCursor();
        }

        /// <summary>
        /// Moves the cursor to the start of the line.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void DoCarriageReturn()
        {
            mX = 0;
            UpdateCursor();
        }

        /// <summary>
        /// Print a tab character to the console.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void DoTab()
        {
            Write(Space);
            Write(Space);
            Write(Space);
            Write(Space);
        }

        /// <summary>
        /// Write char to the console.
        /// </summary>
        /// <param name="aChar">A char to write</param>
        public void Write(byte aChar)
        {
            mText[mX, mY] = aChar;
            mX++;
            if (mX == mText.Cols)
            {
                DoLineFeed();
            }
            UpdateCursor();
        }

        //TODO: Optimize this
        /// <summary>
        /// Writes the given sequence of ASCII characters in the form of a byte
        /// array to the console.
        /// </summary>
        /// <param name="aText">The byte array to write to the console.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write(byte[] aText)
        {
            if (aText == null)
            {
                return;
            }

            for (int i = 0; i < aText.Length; i++)
            {
                switch (aText[i])
                {
                    case LineFeed:
                        DoLineFeed();
                        break;

                    case CarriageReturn:
                        DoCarriageReturn();
                        break;

                    case Tab:
                        DoTab();
                        break;

                    /* Normal characters, simply write them */
                    default:
                        Write(aText[i]);
                        break;
                }
            }
        }

        /// <summary>
        /// The foreground color of the displayed text.
        /// </summary>
        public ConsoleColor Foreground
        {
            get => (ConsoleColor)(mText.GetColor() ^ (byte)((byte)Background << 4));
            set => mText.SetColors(value, Background);
        }

        /// <summary>
        /// The background color of the displayed text.
        /// </summary>
        public ConsoleColor Background
        {
            get => (ConsoleColor)(mText.GetColor() >> 4);
            set => mText.SetColors(Foreground, value);
        }

        /// <summary>
        /// The size of the cursor, in the range of 1 to 100.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when trying to set value out of range.</exception>
        public int CursorSize
        {
            get => mText.GetCursorSize();
            set
            {
                // Value should be a percentage from [1, 100].
                if (value is < 1 or > 100)
                {
                    throw new ArgumentOutOfRangeException(nameof(value), value, "The given CursorSize value " + value + " is out of range (1 - 100).");
                }

                mText.SetCursorSize(value);
            }
        }

        public Stream OpenStandardInput()
        {
            return new CosmosConsoleStream(FileAccess.Read);
        }

        public Stream OpenStandardOutput()
        {
            return new CosmosConsoleStream(FileAccess.Write);
        }

        public Stream OpenStandardError()
        {
            return new CosmosConsoleStream(FileAccess.Write);
        }

        public static bool IsStdInRedirected()
        {

            return global::System.Console.In switch
            {
                null => false,
                SyncTextReader sync => !sync.IsStdIn,
                StreamReader streamReader => streamReader.BaseStream is not Console.CosmosConsoleStream,
                _ => true
            };
        }

        public static bool IsStdOutRedirected()
        {
            return !(global::System.Console.Out is StreamWriter streamWriter
                    && streamWriter.BaseStream is Console.CosmosConsoleStream);
        }

        public static bool IsStdErrorRedirected()
        {
            return !(global::System.Console.Error is StreamWriter streamWriter
                    && streamWriter.BaseStream is Console.CosmosConsoleStream);
        }

        public TextReader GetOrCreateReader(bool firstTime = false)
        {
            if (!firstTime && global::System.Console.IsInputRedirected)
            {
                var inputStream = OpenStandardInput();
                return SyncTextReader.GetSynchronizedTextReader(
                    inputStream == Stream.Null
                    ? StreamReader.Null
                    : new StreamReader(
                        stream: inputStream,
                        encoding: global::System.Console.InputEncoding,
                        detectEncodingFromByteOrderMarks: false,
                        bufferSize: ReadBufferSize,
                        leaveOpen: true
                        ));
            }
            else
            {
                return StdInReader;
            }
        }
        public TextWriter CreateOutputWriter(Stream outputStream) => outputStream == Stream.Null ?
               TextWriter.Null :
               (new StreamWriter(
                   stream: outputStream,
                   encoding: global::System.Console.OutputEncoding.RemovePreamble(), // This ensures no prefix is written to the stream.
                   bufferSize: WriteBufferSize,
                   leaveOpen: true)
                {
                    AutoFlush = true
                });
        public ConsoleKeyInfo ReadKey(bool intercept)
        {
            if (global::System.Console.IsInputRedirected)
            {
                throw new InvalidOperationException("Can not read console keys as input is redirected.");
            }

            global::System.Console.WriteLine("check passed");

            ConsoleKeyInfo keyInfo = StdInReader.ReadKey(out bool previouslyProcessed);

            if (!intercept && !previouslyProcessed && keyInfo.KeyChar != '\0')
            {
                global::System.Console.Write(keyInfo.KeyChar);
            }
            return keyInfo;
        }

        internal SyncTextReader StdInReader => _stdInReader ??= SyncTextReader
                    .GetSynchronizedTextReader(new StdInReader(global::System.Console.InputEncoding));


        /// <summary>
        /// Get or sets the visibility of the cursor.
        /// </summary>
        public bool CursorVisible
        {
            get => mText.GetCursorVisible();
            set => mText.SetCursorVisible(value);
        }
    }
    internal static class EncodingExtensions
    {
        public static Encoding RemovePreamble(this Encoding encoding)
        {
            if (encoding.Preamble.Length == 0)
            {
                return encoding;
            }
            return new ConsoleEncoding(encoding);
        }
    }
}
