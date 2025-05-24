using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace bot.Core.Input
{
    public class WindowsInputSimulator : IInputSimulator
    {
        [DllImport("user32.dll")]
        private static extern bool SetCursorPos(int x, int y);

        [DllImport("user32.dll")]
        private static extern void mouse_event(uint dwFlags, int dx, int dy, uint dwData, int dwExtraInfo);

        [DllImport("user32.dll")]
        private static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, int dwExtraInfo);

        private const uint MOUSEEVENTF_LEFTDOWN = 0x02;
        private const uint MOUSEEVENTF_LEFTUP = 0x04;
        private const uint KEYEVENTF_KEYDOWN = 0x0000;
        private const uint KEYEVENTF_KEYUP = 0x0002;

        public void MoveTo(Point position)
        {
            SetCursorPos(position.X, position.Y);
        }

        public void Click()
        {
            var pos = Cursor.Position;
            mouse_event(MOUSEEVENTF_LEFTDOWN | MOUSEEVENTF_LEFTUP, pos.X, pos.Y, 0, 0);
        }

        public void PressKey(Keys key)
        {
            KeyDown(key);
            KeyUp(key);
        }

        public void KeyDown(Keys key)
        {
            keybd_event((byte)key, 0, KEYEVENTF_KEYDOWN, 0);
        }

        public void KeyUp(Keys key)
        {
            keybd_event((byte)key, 0, KEYEVENTF_KEYUP, 0);
        }

        public void Dispose()
        {
            // Освобождение ресурсов, если необходимо
        }
    }
}