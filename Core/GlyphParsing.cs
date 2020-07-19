using System;
using System.Collections.Generic;
using System.Text;
using Point = System.Drawing.Point;

using Image = System.Collections.Generic.HashSet<System.Drawing.Point>;

namespace Core
{
    public struct NumberLocation
    {
        public readonly Point p;
        public readonly long num;

        public NumberLocation(Point p, long num)
        {
            this.p = p;
            this.num = num;
        }
    }

    public class GlyphParsing
    {

        public static List<NumberLocation> ExtractNumbers(IEnumerable<Point> image)
        {
            var im = new Image(image);
            var result = new List<NumberLocation>();

            foreach (Point topStart in im)
            {
                int x = topStart.X - 1;
                int y = topStart.Y;

                if (TryParse(im, x, y, out long num))
                {
                    result.Add(new NumberLocation(new Point(x, y), num));
                }
            }

            return result;
        }

        private static bool TryParse(HashSet<Point> im, int x, int y, out long num)
        {
            num = 0;

            if (InImage(im, x, y) || !InImage(im, x + 1, y) || !InImage(im, x, y + 1))
            {
                return false;
            }

            int width = 0;
            while (InImage(im, x + width + 1, y)) width++;

            int height = 0;
            while (InImage(im, x, y + height + 1)) height++;

            bool isNeg = height == width + 1;
            if (height != width && !isNeg)
            {
                return false;
            }

            // Check for a black border
            if (!LineIsClear(im, x - 1, y - 1, 1, 0, width + 3)
                || !LineIsClear(im, x - 1, y - 1, 0, 1, height + 3)
                || !LineIsClear(im, x - 1, y + height + 1, 1, 0, width + 3)
                || !LineIsClear(im, x + width + 1, y - 1, 0, 1, height + 3))
            {
                return false;
            }

            if (isNeg && !LineIsClear(im, x + 1, y + height, 1, 0, width))
            {
                return false;
            }

            long bit = 1;
            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    if (InImage(im, x + j + 1, y + i + 1))
                    {
                        num |= bit;
                    }
                    bit <<= 1;
                }
            }

            return true;
        }

        private static bool LineIsClear(Image im, int x, int y, int dx, int dy, int n)
        {
            for (; n > 0; n--, x += dx, y += dy)
            {
                if (InImage(im, x, y)) return false;
            }

            return true;
        }

        private static bool InImage(Image image, int x, int y)
        {
            return image.Contains(new Point(x, y));
        }
    }
}
