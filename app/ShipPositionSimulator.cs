using Core;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using Priority_Queue;

namespace app
{
    public class FutureState
    {
        public Point position;
        public Point velocity;
        public Point thrust;
        public FutureState last;

        public FutureState(Point p, Point v, Point t, FutureState l)
        {
            position = p; velocity = v; thrust = t;
            last = l;
        }
        public FutureState(Point p, Point v, Point t): this(p, v, t, null)
        {
        }

        public FutureState(Ship s): this(s.Position, s.Velocity, s.Thrust, null)
        {
        }

        public FutureState Play(Point thrust)
        {
            Point gravity = ShipPositionSimulator.Gravity(position);
            var nextVelocity = new Point(velocity.X + gravity.X - thrust.X, velocity.Y + gravity.Y - thrust.Y);
            var nextPosition = new Point(position.X + velocity.X, position.Y + velocity.Y);
            return new FutureState(nextPosition, nextVelocity, thrust, this);
        }

        public FutureState Play(Point[] thrusts)
        {
            var next = this;
            foreach (var thrust in thrusts) next = next.Play(thrust);
            return next;
        }

        public FutureState[] Plays()
        {
            var nexts = new FutureState[9];
            for (var x = -1; x <= 1; x++)
                for (var y = -1; y <= 1; y++)
                    nexts[((x + 1) * 3) + y + 1] = Play(new Point(x, y));
            return nexts;
        }

        public bool IsDeath(long star, long arena)
        {
            var death =
                Math.Abs(position.X) <= star &&
                Math.Abs(position.Y) <= star ||
                Math.Abs(position.X) >= arena ||
                Math.Abs(position.Y) >= arena;

            return death || last != null && last.IsDeath(star, arena);
        }

        public double Distance(Point target)
        {
            return Math.Sqrt(
                (position.X - target.X) *
                (position.X - target.X) +
                (position.Y - target.Y) *
                (position.Y - target.Y)
            );
        }

        public double Speed()
        {
            return Math.Sqrt(
                velocity.X * velocity.X +
                velocity.Y * velocity.Y
            );
        }

        public int Turns(Point target)
        {
            return (int)(Distance(target) / Speed());
        }

        public FutureState Hunt(Point target, long star, long arena, int max=3)
        {
            FutureState closest = this;
            int closeness = Turns(target);

            var open = new SimplePriorityQueue<FutureState>();
            open.Enqueue(this, Turns(target));

            var dists = new Dictionary<FutureState, int>();
            dists.Add(this, 0);

            var guesses = new Dictionary<FutureState, int>();
            guesses.Add(this, Turns(target));

            while(open.Count > 0)
            {
                var check = open.Dequeue();
                // Console.WriteLine($"Open {check.position} {check.velocity} {check.thrust}");
                if (check.position == target)
                {
                    return check;
                }

                var plays = Plays();
                foreach (var next in plays)
                {
                    var dist = dists[check] + 1;

                    if (dist >= max || check.IsDeath(star, arena)) continue;

                    if (!dists.ContainsKey(next) || dist < dists[next])
                    {
                        dists[next] = dist;

                        guesses[next] = dist + next.Turns(target);
                        if (guesses[next] < closeness)
                        {
                            closest = next;
                            closeness = guesses[next];
                        }

                        open.Enqueue(next, dist);
                    }
                }
            }

            return closest;
        }
    }

    public class ShipPositionSimulator
    {
        public static Point[] Thrusts = new Point[] {
            new Point(-1, -1),
            new Point(0, -1),
            new Point(1, -1),
            new Point(-1, 0),
            new Point(1, 0),
            new Point(-1, 1),
            new Point(0, 1),
            new Point(1, 1),
        };

        public static Point FuturePosition(Ship ship, int numTurns, Point thrust)
        {
            return FuturePositionList(ship, numTurns, thrust).Last();
        }

        public static List<Point> FuturePositionList(Ship ship, int numTurns, Point thrust)
        {
            return FuturePositionList(ship, numTurns, new List<Point>(new Point[] { thrust }));
        }
        public static List<Point> FuturePositionList(Ship ship, int numTurns, List<Point> thrusts)
        {
            List<Point> positions = new List<Point>();
            Point currentVelocity = ship.Velocity;
            Point currentPosition = ship.Position;
            for (int i = 0; i < numTurns; i++)
            {
                Point gravity = Gravity(currentPosition);
                var thrust = (i < thrusts.Count ? thrusts[i] : new Point(0, 0));
                currentVelocity = new Point(currentVelocity.X + gravity.X - thrust.X, currentVelocity.Y + gravity.Y - thrust.Y);
                currentPosition = new Point(currentPosition.X + currentVelocity.X, currentPosition.Y + currentVelocity.Y);
                positions.Add(currentPosition);
            }

            return positions;
        }

        public static Point FuturePosition(Ship ship, int numTurns)
        {
            return FuturePosition(ship, numTurns, new Point(0, 0));
        }

        public static Point Gravity(Point position)
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
