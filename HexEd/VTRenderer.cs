using Microsoft.Win32.SafeHandles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace HexEd
{
    public class VTRenderer : IRenderer
    {
        [DllImport("kernel32.dll", SetLastError = true)]
        static extern IntPtr GetStdHandle(int nStdHandle);
        private static IntPtr stdHandle;
        private static SafeFileHandle h;

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

        [DllImport("kernel32.dll", SetLastError = true)]
        //static extern bool WriteConsole(SafeFileHandle hConsoleOutput, CharInfo[] lpBuffer, int nNumberOfCharsToWrite, ref int lpNumberOfCharsWritten);
        static extern bool WriteConsole(SafeFileHandle hConsoleOutput, string lpBuffer, int nNumberOfCharsToWrite, ref int lpNumberOfCharsWritten);

        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        [DllImport("kernel32.dll")]
        static extern bool GetConsoleMode(IntPtr hConsoleHandle, out uint lpMode);



        [DllImport("Kernel32.dll", ExactSpelling = true)]
        private static extern int SetConsoleMode(IntPtr hWnd, int wFlag);

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
        private string _buffer;
        private static SmallRect rect;

        public void EndDraw()
        {
            var written = 0;
            WriteConsole(h, _buffer, _buffer.Length, ref written);
        }

        public void BeginDraw()
        {
            _buffer = string.Empty;
            //stdHandle = GetStdHandle(-11);
            //h = new SafeFileHandle(stdHandle, true);
            h = CreateFile("CONOUT$", 0x40000000, 2, IntPtr.Zero, FileMode.Open, 0, IntPtr.Zero);

            IntPtr consoleHandle = GetStdHandle(-10);
            uint consoleMode;
            if (!GetConsoleMode(consoleHandle, out consoleMode))
            {
                // ERROR: Unable to get console mode.
                return;
            }

            // Clear the quick edit bit in the mode flags
            consoleMode &= ~(uint)0x0040;

            // set the new mode
            SetConsoleMode(consoleHandle, (int)consoleMode);
            //{
            //    // ERROR: Unable to set console mode
            //    return false;
            //}


            var consoleWidth = System.Console.WindowWidth;
            var consoleHeight = System.Console.WindowHeight;

            if (!h.IsInvalid)
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
            _buffer += $"\x1b[{y};{x}H{text}";
            //for (int i = 0; i < text.Length; i++)
            //{
                
            //}
        }

        public void PrintAt(int x, int y, string text, int color)
        {
            _buffer += $"\x1b[{y};{x}H\x1b[38;5;{color}m{text}";
        }


        public void PrintAt(int x, int y, string text, ConsoleColor foreColor, ConsoleColor backColor = ConsoleColor.Black)
        {
            _buffer += $"\x1b[{y};{x}H\x1b[38;5;{(byte)foreColor}m\x1b[48;5;{(byte)backColor}m{text}";
        }
    }
}
