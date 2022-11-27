using Microsoft.Win32.SafeHandles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace HexEd
{
    public class Renderer : IRenderer
    {
        [DllImport("kernel32.dll", SetLastError = true)]
        static extern IntPtr GetStdHandle(int nStdHandle);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool GetConsoleMode(IntPtr hConsoleHandle, out uint lpMode);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool SetConsoleMode(IntPtr hConsoleHandle, uint dwMode);

        [Flags]
        private enum ConsoleModes : uint
        {
            ENABLE_PROCESSED_INPUT = 0x0001,
            ENABLE_LINE_INPUT = 0x0002,
            ENABLE_ECHO_INPUT = 0x0004,
            ENABLE_WINDOW_INPUT = 0x0008,
            ENABLE_MOUSE_INPUT = 0x0010,
            ENABLE_INSERT_MODE = 0x0020,
            ENABLE_QUICK_EDIT_MODE = 0x0040,
            ENABLE_EXTENDED_FLAGS = 0x0080,
            ENABLE_AUTO_POSITION = 0x0100,
            ENABLE_VIRTUAL_TERMINAL_INPUT = 0x0200,

            ENABLE_PROCESSED_OUTPUT = 0x0001,
            ENABLE_WRAP_AT_EOL_OUTPUT = 0x0002,
            ENABLE_VIRTUAL_TERMINAL_PROCESSING = 0x0004,
            DISABLE_NEWLINE_AUTO_RETURN = 0x0008,
            ENABLE_LVB_GRID_WORLDWIDE = 0x0010
        }

        private static SafeFileHandle _handle;

        [DllImport("Kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        static extern SafeFileHandle CreateFile(string fileName,
        [MarshalAs(UnmanagedType.U4)] uint fileAccess,
        [MarshalAs(UnmanagedType.U4)] uint fileShare,
        IntPtr securityAttributes,
        [MarshalAs(UnmanagedType.U4)] FileMode creationDisposition,
        [MarshalAs(UnmanagedType.U4)] int flags,
        IntPtr template);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool WriteConsoleOutput(SafeFileHandle hConsoleOutput, CharInfo[] lpBuffer, Coord dwBufferSize, Coord dwBufferCoord, ref SmallRect lpWriteRegion);

        [StructLayout(LayoutKind.Sequential)]
        public struct Coord
        {
            public short X;
            public short Y;

            public Coord(short X, short Y)
            {
                this.X = X;
                this.Y = Y;
            }
        };

        [StructLayout(LayoutKind.Explicit)]
        public struct CharUnion
        {
            [FieldOffset(0)] public char UnicodeChar;
            [FieldOffset(0)] public byte AsciiChar;
        }

        [StructLayout(LayoutKind.Explicit)]
        public struct CharInfo
        {
            [FieldOffset(0)] public CharUnion Char;
            [FieldOffset(2)] public short Attributes;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct SmallRect
        {
            public short Left;
            public short Top;
            public short Right;
            public short Bottom;
        }

        private static CharInfo[] buf;
        private static SmallRect rect;

        public void EndDraw()
        {
            WriteConsoleOutput(_handle, buf, new Coord() { X = rect.Right, Y = rect.Bottom }, new Coord() { X = 0, Y = 0 }, ref rect);
        }

        public void BeginDraw()
        {
            var stdHandle = GetStdHandle(-10);

            if (GetConsoleMode(stdHandle, out uint modes))
            {
                modes = modes & ~(uint)ConsoleModes.ENABLE_QUICK_EDIT_MODE;
                modes |= (uint)ConsoleModes.ENABLE_EXTENDED_FLAGS;
                if (!SetConsoleMode(stdHandle, (uint)(ConsoleModes.ENABLE_EXTENDED_FLAGS)))
                {

                }
            }

            _handle = CreateFile("CONOUT$", 0x40000000, 2, IntPtr.Zero, FileMode.Open, 0, IntPtr.Zero);

            var consoleWidth = Console.WindowWidth;
            var consoleHeight = Console.WindowHeight;

            if (!_handle.IsInvalid)
            {
                buf = new CharInfo[consoleWidth * consoleHeight];
                rect = new SmallRect() { Left = 0, Top = 0, Right = (short)consoleWidth, Bottom = (short)consoleHeight };
            }

            System.Console.CursorVisible = false;
        }

        public void Clear()
        {
            System.Console.Clear();
        }

        public void PrintAt(int x, int y, string text)
        {
            if (y >= rect.Bottom)
            {
                return;
            }
            for (int i = 0; i < text.Length; i++)
            {
                if (x + i >= rect.Right)
                {
                    break;
                }
                buf[x + y * rect.Right + i].Char.UnicodeChar = text[i];
                buf[x + y * rect.Right + i].Attributes = 7;

            }
        }

        public void PrintAt(int x, int y, string text, ConsoleColor foreColor, ConsoleColor backColor = ConsoleColor.Black)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return;
            }

            if (y >= rect.Bottom)
            {
                return;
            }

            for (int i = 0; i < text.Length; i++)
            {
                if (x + i >= rect.Right)
                {
                    break;
                }
                buf[x + y * rect.Right + i].Char.UnicodeChar = text[i];
                buf[x + y * rect.Right + i].Attributes = (short)(((short)backColor << 4) + (short)foreColor);
            }
        }
    }
}
