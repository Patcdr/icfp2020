using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace Core
{
    public class DrawFrame
    {
        public IList<Point> Points { get; }
        public int MinX { get; }
        public int MaxX { get; }
        public int MinY { get; }
        public int MaxY { get; }
        public int CenterX { get { return (MinX + MaxX) / 2; } }
        public int CenterY { get { return (MinY + MaxY) / 2; } }

        public DrawFrame(IEnumerable<Point> points)
        {
            Points = new List<Point>(points).AsReadOnly();

            // Figure out the bounds
            MinX = int.MaxValue;
            MaxX = int.MinValue;
            MinY = int.MaxValue;
            MaxY = int.MinValue;

            foreach (Point p in Points)
            {
                MinX = Math.Min(MinX, p.X);
                MaxX = Math.Max(MaxX, p.X);
                MinY = Math.Min(MinY, p.Y);
                MaxY = Math.Max(MaxY, p.Y);
            }
        }

        private DrawFrame(List<Point> points, int minX, int maxX, int minY, int maxY)
        {
            Points = points.AsReadOnly();
            MinX = minX;
            MaxX = maxX;
            MinY = minY;
            MaxY = maxY;
        }

    }
}
