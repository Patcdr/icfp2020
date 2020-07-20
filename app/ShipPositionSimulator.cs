using Core;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace app
{
    public class ShipPositionSimulator
    {
        public static Point FuturePosition(Ship ship, int numTurns, Point thrust)
        {
            return FuturePositionList(ship, numTurns, thrust).Last();
        }

        public static List<Point> FuturePositionList(Ship ship, int numTurns, Point thrust)
        {
            List<Point> positions = new List<Point>();
            Point currentVelocity = new Point(ship.Velocity.X - thrust.X, ship.Velocity.Y - thrust.Y);
            Point currentPosition = ship.Position;
            for (int i = 0; i < numTurns; i++)
            {
                Point gravity = Gravity(currentPosition);
                currentVelocity = new Point(currentVelocity.X + gravity.X, currentVelocity.Y + gravity.Y);
                currentPosition = new Point(currentPosition.X + currentVelocity.X, currentPosition.Y + currentVelocity.Y);
                positions.Add(currentPosition);
            }

            return positions;
        }

        public static Point FuturePosition(Ship ship, int numTurns)
        {
            return FuturePosition(ship, numTurns, new Point(0, 0));
        }

        private static Point Gravity(Point position)
        {
            // Am I on a diagonal?
            if (position.X == position.Y)
            {
                int xDir = position.X > 0 ? -1 : 1;
                int yDir = position.Y > 0 ? -1 : 1;
                return new Point(xDir, yDir);
            }
            else
            {
                if (Math.Abs(position.X) < Math.Abs(position.Y))
                {
                    int yDir = position.Y > 0 ? -1 : 1;
                    return new Point(0, yDir);
                }
                else
                {
                    int xDir = position.X > 0 ? -1 : 1;
                    return new Point(xDir, 0);
                }
            }
        }
    }
}
