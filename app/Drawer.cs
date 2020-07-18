using Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace app
{
    class Drawer
    {
        private const int X_OFFSET = 200;
        private const int Y_OFFSET = 150;
        private const int WIDTH = X_OFFSET * 2;
        private const int HEIGHT = Y_OFFSET * 2;
        public static bool drawing = false;

        public static int index = 0;

        public static bool[,] DrawCons(Value head)
        {
            if (!Drawer.drawing)
                return null;
            

            return Draw(new DrawFrame(head));
        }

        public static IList<bool[,]> MultipleDraw(Value head)
        {
            var result = new List<bool[,]>();

            foreach (Value points in UtilityFunctions.ListAsEnumerable(head, null))
            {
                result.Add(DrawCons(points));
            }
            if (Drawer.drawing) {
                Console.WriteLine("────────────────────────────────────");
                Console.WriteLine("────────────────────────────────────");
            }

            return result;
        }

        public static bool[,] Draw(DrawFrame drawFrame)
        {
            // Convert the list of points to a grid
            bool[,] grid = new bool[WIDTH, HEIGHT];

            foreach (System.Drawing.Point point in drawFrame.Points)
            {
                int x = point.X + X_OFFSET;
                int y = point.Y + Y_OFFSET;

                if (x < 0 || x >= WIDTH || y < 0 || y >= HEIGHT)
                {
                    throw new ArgumentException($"Coordinate out of bounds: ({point.X}, {point.Y}) -> ({x}, {y})");
                }

                grid[x, y] = true;
            }

            var lines = new List<string>();
            StringBuilder line;

            // Draw the grid
            line = new StringBuilder();
            line.Append("   ");
            for (int x = 0; x < WIDTH; x++)
            {
                line.Append($"{x/10%10}");
            }
            line.Append("   ");
            lines.Add(line.ToString());

            line = new StringBuilder();
            line.Append("   ");
            for (int x = 0; x < WIDTH; x++)
            {
                line.Append($"{x%10}");
            }
            line.Append("   ");
            lines.Add(line.ToString());

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

            line = new StringBuilder();
            line.Append("   ");
            for (int x = 0; x < WIDTH; x++)
            {
                line.Append($"{x/10%10}");
            }
            line.Append("   ");
            lines.Add(line.ToString());

            line = new StringBuilder();
            line.Append("   ");
            for (int x = 0; x < WIDTH; x++)
            {
                line.Append($"{x%10}");
            }
            line.Append("   ");
            lines.Add(line.ToString());

            if (Drawer.drawing) {
                System.IO.File.WriteAllLines($"grid{index}.txt", lines);
                index += 1;

                foreach (var l in lines) Console.WriteLine(l);
                Console.WriteLine("======================================");
            }

            // Not sure what to return, but the spec has Draw
            // returning the resulting pictures somehow.
            return grid;
        }
    }
}
