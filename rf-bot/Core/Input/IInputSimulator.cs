using System;
using System.Drawing;
using System.Windows.Forms;

namespace bot.Core.Input
{
    public interface IInputSimulator : IDisposable
    {
        void MoveTo(Point position);
        void Click();
        void PressKey(Keys key);
        void KeyDown(Keys key);
        void KeyUp(Keys key);
    }
}