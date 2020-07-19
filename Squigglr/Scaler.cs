using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Shapes;
using IntPoint = System.Drawing.Point;

namespace Squigglr
{
    public static class Scaler
    {
        private static double Scale = 2.5;
        private static double RealWidth = 10;
        private static double RealHeight = 10;
        private static int PanShiftWidth = 0;
        private static int PanShiftHeight = 0;

        public static Point Convert(IntPoint p)
        {
            return new Point(p.X * Scale + RealWidth / 2 - Scale / 2 + PanShiftWidth * Scale,
                             p.Y * Scale + RealHeight / 2 - Scale / 2 + PanShiftHeight * Scale);
        }

        public static IntPoint Convert(Point p)
        {
            return new IntPoint((int)Math.Round((p.X - RealWidth / 2 - PanShiftWidth * Scale) / Scale),
                                (int)Math.Round((p.Y - RealHeight / 2 - PanShiftHeight * Scale) / Scale));
        }

        public static void ResizeRectangle(Rectangle r)
        {
            r.Width = Scale;
            r.Height = Scale;
        }

        public static void ResizeWindow(Size size)
        {
            RealWidth = size.Width;
            RealHeight = size.Height;
        }

        public static void ShiftView(bool? vertical = null, bool? horizontal = null)
        {
            if (vertical.HasValue)
            {
                PanShiftHeight += vertical.Value ? 1 : -1;
            }

            if (horizontal.HasValue)
            {
                PanShiftWidth += horizontal.Value ? 1 : -1;
            }
        }

        public static void Zoom(bool isIn)
        {
            Scale += isIn ? 0.5 : -0.5;

            if (Scale < 0.2)
            {
                Scale = 0.2;
            }
        }
    }
}
