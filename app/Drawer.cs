using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace app
{
    // Probably going to have to redefine these
    using Point = IList<int>;
    using PointsList = IList<IList<int>>;
    using MultiPointsList = IList<IList<IList<int>>>;

    class Drawer
    {
        private const int WIDTH = 17;
        private const int HEIGHT = 13;

        public static IList<object> MultipleDraw(MultiPointsList pointsList)
        {
            var result = new List<object>();

            foreach(PointsList points in pointsList)
            {
                result.Add(Draw(points));
            }

            return result;
        }

        public static object Draw(PointsList points)
        {
            // Convert the list of points to a grid
            bool[,] grid = new bool[WIDTH, HEIGHT];

            foreach (Point point in points)
            {
                int x = point[0];
                int y = point[1];

                if (x < 0 || x >= WIDTH || y < 0 || y >= HEIGHT)
                {
                    throw new ArgumentException($"Coordinate out of bounds: ({x}, {y})");
                }

                grid[x, y] = true;
            }

            // Draw the grid
            Console.WriteLine("┌──────────────────────────────────┐");

            for (int y = 0; y < HEIGHT; y++)
            {
                Console.Write("│");

                for (int x = 0; x < WIDTH; x++)
                {

                    Console.Write(grid[x, y] ? "██" : "  ");
                }

                Console.WriteLine("│");
            }

            Console.WriteLine("└──────────────────────────────────┘");

            // Not sure what to return, but the spec has Draw
            // returning the resulting pictures somehow.
            return null;
        }
    }
}
