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
        private const int OFFSET = 30;
        private const int WIDTH = 50 + OFFSET;
        private const int HEIGHT = 50 + OFFSET;
        public static bool drawing = false;

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
                long x = point.Item1 + OFFSET;
                long y = point.Item2 + OFFSET;

                if (x < 0 || x >= WIDTH || y < 0 || y >= HEIGHT)
                {
                    continue;
                    throw new ArgumentException($"Coordinate out of bounds: ({x}, {y})");

                }

                grid[x, y] = true;
            }

            // Draw the grid
            Console.Write("   ");
            for (int x = 0; x < WIDTH; x++)
            {
                Console.Write($"{x:00} ");
            }
            Console.WriteLine("   ");

            for (int y = 0; y < HEIGHT; y++)
            {
                Console.Write($"{y:00} ");

                for (int x = 0; x < WIDTH; x++)
                {
                    Console.Write(grid[x, y] ? "███" : "   ");
                }

                Console.WriteLine($"{y:00} ");
            }

            Console.Write("   ");
            for (int x = 0; x < WIDTH; x++)
            {
                Console.Write($"{x:00} ");
            }
            Console.WriteLine("   ");

            // Not sure what to return, but the spec has Draw
            // returning the resulting pictures somehow.
            return grid;
        }
    }
}
