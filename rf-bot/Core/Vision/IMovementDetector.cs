using System.Drawing;

namespace bot.Core.Vision
{
    public interface IMovementDetector : IDisposable
    {
        Point? FindLargestMovement(Bitmap frame1, Bitmap frame2);
        bool IsCursorRed(Point position);
        bool TemplateMatch(Bitmap scene, Bitmap template);
    }
}