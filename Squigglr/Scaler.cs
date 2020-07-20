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

        public static Point ConvertGridToScreen(double x, double y)
        {
            return new Point(x * Scale + RealWidth / 2 - Scale / 2 + PanShiftWidth * Scale,
                             y * Scale + RealHeight / 2 - Scale / 2 + PanShiftHeight * Scale);
        }

        public static IntPoint ConvertScreenToGrid(double x, double y)
        {
            return new IntPoint((int)Math.Round((x - RealWidth / 2 - PanShiftWidth * Scale) / Scale),
                                (int)Math.Round((y - RealHeight / 2 - PanShiftHeight * Scale) / Scale));
        }

        public static void ResizeRectangle(Rectangle r, double radius)
        {
            r.Width = Scale * radius;
            r.Height = Scale * radius;
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
