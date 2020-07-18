using Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace app
{
    // Probably going to have to redefine these
    using Point = Tuple<long, long>;
    using PointsList = IList<Tuple<long, long>>;

    class Drawer
    {
        private const int X_OFFSET = 150;
        private const int Y_OFFSET = 60;
        private const int WIDTH = X_OFFSET * 2;
        private const int HEIGHT = Y_OFFSET * 2;
        public static bool drawing = false;

        public static int index = 0;

        public static bool[,] DrawCons(Value head)
        {
            if (!Drawer.drawing)
                return null;
            PointsList points = new List<Point>();

            foreach (Value point in UtilityFunctions.ListAsEnumerable(head, null))
            {
                var x = point.Invoke(Library.TrueVal, null).AsNumber();
                var y = point.Invoke(Library.FalseVal, null).AsNumber();
                points.Add(new Point(x, y));
            }

            return Draw(points);
        }

        public static IList<bool[,]> MultipleDraw(Value head)
        {
            var result = new List<bool[,]>();

            foreach (Value points in UtilityFunctions.ListAsEnumerable(head, null))
            {
                result.Add(DrawCons(points));
            }



            Console.WriteLine("────────────────────────────────────");
            Console.WriteLine("────────────────────────────────────");

            return result;
        }

        public static bool[,] Draw(PointsList points)
        {
            // Convert the list of points to a grid
            bool[,] grid = new bool[WIDTH, HEIGHT];

            foreach (Point point in points)
            {
                long x = point.Item1 + X_OFFSET;
                long y = point.Item2 + Y_OFFSET;

                if (x < 0 || x >= WIDTH || y < 0 || y >= HEIGHT)
                {
                    throw new ArgumentException($"Coordinate out of bounds: ({point.Item1}, {point.Item2}) -> ({x}, {y})");
                }

                grid[x, y] = true;
            }

            var lines = new List<string>();
            StringBuilder line;

            // Draw the grid

            // line = new StringBuilder();
            // line.Append("   ");
            // for (int x = 0; x < WIDTH; x++)
            // {
            //     line.Append($"{x%100:00} ");
            // }
            // line.Append("   ");
            // lines.Add(line.ToString());

            for (int y = 0; y < HEIGHT; y++)
            {
                line = new StringBuilder();
                line.Append($"{y%100:00} ");

                for (int x = 0; x < WIDTH; x++)
                {
                    line.Append(grid[x, y] ? "█" : " ");
                }

                line.Append($"{y%100:00} ");
                lines.Add(line.ToString());
            }

            // line = new StringBuilder();
            // line.Append("   ");
            // for (int x = 0; x < WIDTH; x++)
            // {
            //     line.Append($"{x%100!:00} ");
            // }
            // line.Append("   ");
            // lines.Add(line.ToString());

            // System.IO.File.WriteAllLines($"grid{index}.txt", lines);
            // index += 1;

            foreach (var l in lines) Console.WriteLine(l);
            Console.WriteLine("======================================");

            // Not sure what to return, but the spec has Draw
            // returning the resulting pictures somehow.
            return grid;
        }
    }
}
