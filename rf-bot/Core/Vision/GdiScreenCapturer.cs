using System;
using System.Drawing;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace bot.Core.Vision
{
    public class GdiScreenCapturer : IScreenCapturer
    {
        private readonly IntPtr _windowHandle;
        private readonly Rectangle _windowRect;

        public GdiScreenCapturer(string processName)
        {
            var process = Process.GetProcessesByName(processName).FirstOrDefault();
            _windowHandle = process?.MainWindowHandle ?? throw new Exception("Game window not found");
            _windowRect = GetWindowRect(_windowHandle);
        }

        public Bitmap Capture()
        {
            var bmp = new Bitmap(_windowRect.Width, _windowRect.Height);
            using (var g = Graphics.FromImage(bmp))
            {
                g.CopyFromScreen(
                    _windowRect.Location, 
                    Point.Empty, 
                    _windowRect.Size);
            }
            return bmp;
        }

        public Rectangle GetGameWindowRect() => _windowRect;

        [DllImport("user32.dll")]
        private static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

        private Rectangle GetWindowRect(IntPtr hWnd)
        {
            GetWindowRect(hWnd, out var rect);
            return new Rectangle(
                rect.Left, 
                rect.Top, 
                rect.Right - rect.Left, 
                rect.Bottom - rect.Top);
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct RECT
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }
    }
}