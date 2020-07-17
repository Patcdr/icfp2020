using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace app
{
    class Drawer
    {
        private const int WIDTH = 17;
        private const int HEIGHT = 13;

        // Expects a list of points of the form { x1, y1, x2, y2, ... }
        public static void Draw(int[] points)
        {
            if (points.Length % 2 != 0)
            {
                throw new ArgumentException("Expected pairs of coordinates");
            }

            // Convert the list of points to a grid
            bool[,] grid = new bool[WIDTH, HEIGHT];

            for (int i = 0; i < points.Length; i += 2)
            {
                int x = points[i];
                int y = points[i + 1];

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
        }

        public static async Task<int> xMain(string[] args)
        {
            int[] points = { 1, 1, 2, 2, 0, 5 };
            Draw(points);

            return 0;
        }

    }
}
