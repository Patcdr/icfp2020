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

        public DrawFrame(Value consList)
            : this(ConsListToPoints(consList))
        {
        }

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

        private static IEnumerable<Point> ConsListToPoints(Value consList)
        {
            foreach (Value point in UtilityFunctions.ListAsEnumerable(consList, null))
            {
                int x = (int)point.Invoke(Library.TrueVal, null).AsNumber();
                int y = (int)point.Invoke(Library.FalseVal, null).AsNumber();
                yield return new Point(x, y);
            }
        }
    }
}
