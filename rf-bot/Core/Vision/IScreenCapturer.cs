using System.Drawing;

namespace bot.Core.Vision
{
    public interface IScreenCapturer : IDisposable
    {
        Bitmap Capture();
        Rectangle GetGameWindowRect();
    }
}
