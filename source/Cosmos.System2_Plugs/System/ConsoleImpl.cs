using IL2CPU.API.Attribs;
using Cosmos.HAL.Drivers.Video;
using Cosmos.System.Graphics;
using Cosmos.System;
using System.Text;
using Cosmos.HAL.BlockDevice;
using Cosmos.System.IO;

namespace Cosmos.System_Plugs.System
{
    [Plug(Target = typeof (global::System.Console))]
    public static class ConsoleImpl
    {
        #region Properties

        public static bool TreatControlCAsInput => throw new NotImplementedException("Not implemented: TreatControlCAsInput");

        public static int LargestWindowHeight => throw new NotImplementedException("Not implemented: LargestWindowHeight");

        public static int LargestWindowWidth => throw new NotImplementedException("Not implemented: LargestWindowWidth");

        public static string Title => throw new NotImplementedException("Not implemented: Title");

        public static int BufferHeight => throw new NotImplementedException("Not implemented: BufferHeight");

        public static int BufferWidth => throw new NotImplementedException("Not implemented: BufferWidth");

        public static int WindowLeft => throw new NotImplementedException("Not implemented: WindowLeft");

        public static int WindowTop => throw new NotImplementedException("Not implemented: WindowTop");

        public static Encoding OutputEncoding => consoleOutputEncoding;

        public static Encoding InputEncoding => consoleInputEncoding;

        public static bool KeyAvailable => KeyboardManager.KeyAvailable;

        public static bool NumberLock => Global.NumLock;

        public static bool CapsLock => Global.CapsLock;

        public static ConsoleColor ForegroundColor
        {
            get => foreGround;
            set
            {
                foreGround = value;

                if (GetConsole() != null)
                {
                    GetConsole().Foreground = value;
                }
            }
        }

        public static ConsoleColor BackgroundColor
        {
            get => backGround;
            set
            {
                backGround = value;

                if (GetConsole() != null)
                {
                    GetConsole().Background = value;
                }
            }
        }

        public static bool CursorVisible
        {
            get
            {
                var xConsole = GetConsole();
                if (xConsole == null)
                {
                    return false;
                }
                return GetConsole().CursorVisible;
            }
            set
            {
                var xConsole = GetConsole();
                if (xConsole == null)
                {
                    // for now:
                    return;
                }
                xConsole.CursorVisible = value;
            }
        }

        public static int CursorSize
        {
            get
            {
                var xConsole = GetConsole();
                if (xConsole == null)
                {
                    // for now:
                    return 0;
                }
                return xConsole.CursorSize;
            }
            set
            {
                var xConsole = GetConsole();
                if (xConsole == null)
                {
                    // for now:
                    return;
                }
                xConsole.CursorSize = value;
            }
        }

        public static int CursorLeft
        {
            get
            {
                var xConsole = GetConsole();
                if (xConsole == null)
                {
                    // for now:
                    return 0;
                }
                return GetConsole().X;
            }
            set
            {
                var xConsole = GetConsole();
                if (xConsole == null)
                {
                    // for now:
                    return;
                }

                if (value < 0)
                {
                    throw new ArgumentException("The value must be at least 0!", nameof(value));
                }

                if (value < WindowWidth)
                {
                    xConsole.X = value;
                }
                else
                {
                    throw new ArgumentException("The value must be lower than the console width!", nameof(value));
                }
            }
        }

        public static int CursorTop
        {
            get
            {
                var xConsole = GetConsole();
                if (xConsole == null)
                {
                    // for now:
                    return 0;
                }
                return GetConsole().Y;
            }
            set
            {
                var xConsole = GetConsole();
                if (xConsole == null)
                {
                    // for now:
                    return;
                }

                if (value < 0)
                {
                    throw new ArgumentException("The value must be at least 0!", nameof(value));
                }

                if (value < WindowHeight)
                {
                    xConsole.Y = value;
                }
                else
                {
                    throw new ArgumentException("The value must be lower than the console height!", nameof(value));
                }
            }
        }

        public static int WindowHeight
        {
            get
            {
                var xConsole = GetConsole();
                if (xConsole == null)
                {
                    // for now:
                    return 25;
                }
                return GetConsole().Rows;
            }
            set => throw new NotImplementedException("Not implemented: set_WindowHeight");
        }

        public static int WindowWidth
        {
            get
            {
                var xConsole = GetConsole();
                if (xConsole == null)
                {
                    // for now:
                    return 85;
                }
                return GetConsole().Cols;
            }
            set => throw new NotImplementedException("Not implemented: set_WindowWidth");
        }

        public static TextReader In => @in ??= GetOrCreateReader();

        public static TextWriter Out => @out ??= CreateOutputWriter(OpenStandardOutput());

        public static TextWriter Error => err ??= CreateOutputWriter(OpenStandardError());

        public static bool IsOutputRedirected => Cosmos.System.Console.IsStdOutRedirected();

        public static bool IsInputRedirected => Cosmos.System.Console.IsStdInRedirected();

        public static bool IsErrorRedirected => Cosmos.System.Console.IsStdErrorRedirected();
        #endregion

        #region Methods
        public static Stream OpenStandardInput() => Global.Console.OpenStandardInput();

        public static Stream OpenStandardOutput() => Global.Console.OpenStandardOutput();

        public static Stream OpenStandardError() => Global.Console.OpenStandardError();

        public static void SetIn(TextReader newIn)
        {
            ArgumentNullException.ThrowIfNull(newIn, nameof(newIn));
            newIn = SyncTextReader.GetSynchronizedTextReader(newIn);
            @in = newIn;
        }

        public static void SetOut(TextWriter newOut)
        {
            ArgumentNullException.ThrowIfNull(newOut, nameof(newOut));
            @out = newOut;
        }

        public static void SetError(TextWriter newError)
        {
            ArgumentNullException.ThrowIfNull(newError, nameof(newError));
            err = newError;
        }

        public static TextReader GetOrCreateReader() => Global.Console.GetOrCreateReader(@in is null);

        public static TextWriter CreateOutputWriter(Stream stream) => Global.Console.CreateOutputWriter(stream);

        public static void SetBufferSize(int width, int height)
        {
            throw new NotImplementedException("Not implemented: SetBufferSize");
        }

        public static void SetCursorPosition(int left, int top)
        {
            Global.Console.CachedX = left;
            Global.Console.CachedY = top;
            Global.Console.UpdateCursorFromCache();
        }

        public static void SetWindowPosition(int left, int top)
        {
            throw new NotImplementedException("Not implemented: SetWindowPosition");
        }

        public static void SetWindowSize(int width, int height)
        {
            if (width == 40 && height == 25)
            {
                Global.Console.mText.Cols = 40;
                Global.Console.mText.Rows = 25;
                VGAScreen.SetTextMode(VGADriver.TextSize.Size40x25);
            }
            else if (width == 40 && height == 50)
            {
                Global.Console.mText.Cols = 40;
                Global.Console.mText.Rows = 50;
                VGAScreen.SetTextMode(VGADriver.TextSize.Size40x50);
            }
            else if (width == 80 && height == 25)
            {
                Global.Console.mText.Cols = 80;
                Global.Console.mText.Rows = 25;
                VGAScreen.SetTextMode(VGADriver.TextSize.Size80x25);
            }
            else if (width == 80 && height == 50)
            {
                Global.Console.mText.Cols = 80;
                Global.Console.mText.Rows = 50;
                VGAScreen.SetTextMode(VGADriver.TextSize.Size80x50);
            }
            else if (width == 90 && height == 30)
            {
                Global.Console.mText.Cols = 90;
                Global.Console.mText.Rows = 30;
                VGAScreen.SetTextMode(VGADriver.TextSize.Size90x30);
            }
            else if (width == 90 && height == 60)
            {
                Global.Console.mText.Cols = 90;
                Global.Console.mText.Rows = 60;
                VGAScreen.SetTextMode(VGADriver.TextSize.Size90x60);
            }
            else
            {
                throw new Exception("Invalid text size.");
            }

            Global.Console.Cols = Global.Console.mText.Cols;
            Global.Console.Rows = Global.Console.mText.Rows;

            ((HAL.TextScreen)Global.Console.mText).UpdateWindowSize();

            Clear();
        }

        public static (int Left, int Top) GetCursorPosition()
        {
            return (CursorLeft, CursorTop);
        }

        //  MoveBufferArea(int, int, int, int, int, int) is pure CIL
        public static void MoveBufferArea(int sourceLeft, int sourceTop, int sourceWidth, int sourceHeight, int targetLeft, int targetTop, char sourceChar, ConsoleColor sourceForeColor, ConsoleColor sourceBackColor)
        {
            throw new NotImplementedException("Not implemented: MoveBufferArea");
        }

        private static Cosmos.System.Console GetConsole()
        {
            return Global.Console;
        }

        // ReadKey() pure CIL
        public static ConsoleKeyInfo ReadKey(bool intercept)
        {
            return Global.Console.ReadKey(intercept);
        }

        public static ConsoleKeyInfo ReadKey()
        {
            return ReadKey(false);
        }

        public static void ResetColor()
        {
            BackgroundColor = ConsoleColor.Black;
            ForegroundColor = ConsoleColor.White;
        }

        public static void Beep(int frequency, int duration)
        {
            PCSpeaker.Beep((uint)frequency, (uint)duration);
        }

        /// <summary>
        /// Beep() is pure CIL
        /// Default implementation beeps for 200 milliseconds at 800 hertz
        /// In Cosmos, these are Cosmos.System.Duration.Default and Cosmos.System.Notes.Default respectively,
        /// and are used when there are no params
        /// https://docs.microsoft.com/en-us/dotnet/api/system.console.beep?view=netcore-2.0
        /// </summary>
        public static void Beep()
        {
            PCSpeaker.Beep();
        }

        //TODO: Console uses TextWriter - intercept and plug it instead
        public static void Clear()
        {
            Global.Console.Clear();
        }

        #endregion

        #region Fields

        private static TextWriter? @out;
        private static TextWriter? err;
        private static TextReader? @in;
        private static Encoding consoleOutputEncoding = Encoding.ASCII;
        private static Encoding consoleInputEncoding = Encoding.ASCII;
        private static ConsoleColor foreGround = ConsoleColor.White;
        private static ConsoleColor backGround = ConsoleColor.Black;

        #endregion
    }
}
