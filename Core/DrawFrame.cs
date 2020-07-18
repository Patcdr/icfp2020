using System;
using System.Collections.Generic;
using System.Text;

namespace Core
{
    using Point = Tuple<long, long>;

    public class DrawFrame
    {
        public IList<Point> Points { get; }
        public long MinX { get; }
        public long MaxX { get; }
        public long MinY { get; }
        public long MaxY { get; }
        public long CenterX { get { return (MinX + MaxX) / 2; } }
        public long CenterY { get { return (MinY + MaxY) / 2; } }

        public DrawFrame(IEnumerable<Point> points)
        {
            Points = new List<Point>(points).AsReadOnly();

            // Figure out the bounds
            MinX = long.MaxValue;
            MaxX = long.MinValue;
            MinY = long.MaxValue;
            MaxY = long.MinValue;

            foreach (Point p in Points)
            {
                long x = p.Item1;
                long y = p.Item2;

                MinX = Math.Min(MinX, x);
                MaxX = Math.Max(MaxX, x);
                MinY = Math.Min(MinY, y);
                MaxY = Math.Max(MaxY, y);
            }
        }

        private DrawFrame(List<Point> points, long minX, long maxX, long minY, long maxY)
        {
            Points = points.AsReadOnly();
            MinX = minX;
            MaxX = maxX;
            MinY = minY;
            MaxY = maxY;
        }

        // Returns a new DrawFrame that is the union of this and the other
        public DrawFrame Overlay(DrawFrame other)
        {
            var union = new List<Point>();
            union.AddRange(Points);
            union.AddRange(other.Points);

            return new DrawFrame(union,
                Math.Min(MinX, other.MinX),
                Math.Max(MaxX, other.MaxX),
                Math.Min(MinY, other.MinY),
                Math.Max(MaxY, other.MaxY));
        }
    }
}
