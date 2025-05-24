using OpenCvSharp;
using OpenCvSharp.Extensions;
using System;
using System.Drawing;

namespace bot.Core.Vision
{
    public class OpenCvMovementDetector : IMovementDetector
    {
        private const int MovementThreshold = 25;
        private const double TemplateMatchThreshold = 0.7;

        public Point? FindLargestMovement(Bitmap frame1, Bitmap frame2)
        {
            using var mat1 = frame1.ToMat();
            using var mat2 = frame2.ToMat();
            
            // 1. Конвертируем в grayscale
            using var gray1 = new Mat();
            using var gray2 = new Mat();
            Cv2.CvtColor(mat1, gray1, ColorConversionCodes.BGR2GRAY);
            Cv2.CvtColor(mat2, gray2, ColorConversionCodes.BGR2GRAY);

            // 2. Находим разницу
            using var diff = new Mat();
            Cv2.Absdiff(gray1, gray2, diff);
            Cv2.Threshold(diff, diff, MovementThreshold, 255, ThresholdTypes.Binary);

            // 3. Ищем контуры
            var contours = Cv2.FindContoursAsArray(
                diff, 
                RetrievalModes.External, 
                ContourApproximationModes.ApproxSimple);

            if (contours.Length == 0) 
                return null;

            // 4. Находим наибольший контур
            var largestContour = contours.MaxBy(Cv2.ContourArea);
            var moments = Cv2.Moments(largestContour);

            return new Point(
                (int)(moments.M10 / moments.M00), 
                (int)(moments.M01 / moments.M00));
        }

        public bool IsCursorRed(Point position)
        {
            using var bmp = new Bitmap(1, 1);
            using var g = Graphics.FromImage(bmp);
            g.CopyFromScreen(position, Point.Empty, new Size(1, 1));

            var pixel = bmp.GetPixel(0, 0);
            return pixel.R > 200 && pixel.G < 50 && pixel.B < 50;
        }

        public bool TemplateMatch(Bitmap scene, Bitmap template)
        {
            using var sceneMat = scene.ToMat();
            using var templateMat = template.ToMat();
            using var result = new Mat();

            Cv2.MatchTemplate(
                sceneMat.CvtColor(ColorConversionCodes.BGR2GRAY),
                templateMat.CvtColor(ColorConversionCodes.BGR2GRAY),
                result,
                TemplateMatchModes.CCoeffNormed);

            Cv2.MinMaxLoc(result, out _, out double maxVal, out _, out _);
            return maxVal >= TemplateMatchThreshold;
        }

        public void Dispose()
        {
            // Освобождаем ресурсы OpenCV при необходимости
        }
    }
}