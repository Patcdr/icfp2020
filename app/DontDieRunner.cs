using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using Core;
using static Core.Library;

namespace app
{
    public class DontDieRunner : BaseRunner
    {
        public DontDieRunner(Sender sender, long player = 0)
            : base(sender, player)
        {
        }

        public override void Start()
        {
            Join();

            if (IsDone) return;

            Initialize(0, 8, 1);

            // Game loop
            for (long i = State.CurrentTurn; i < State.TotalTurns; i++)
            {
                // Is the game over?
                if (IsDone)
                {
                    return;
                }

                Ship ship = State.GetMyFirstShip();
                bool thrusted = false;

                // If we would die by not thrusting
                if (WillDieSoon(6, new Point(0, 0))) {
                    foreach (Point p in ActionHandler.AllDirections)
                    {
                        // If thrusting would stop us from dying, do it!
                        if (!WillDieSoon(6, p))
                        {
                            Command(Thrust(ship.ID, p));
                            thrusted = true;
                            break;
                        }
                    }
                }
                
                if (!thrusted)
                {
                    Command();
                }
            }
        }

        private bool WillDieSoon(int lookAheadTurns, Point thrust)
        {
            for (int i = 1; i <= lookAheadTurns; i++)
            {
                Point futureLocation = ShipPositionSimulator.FuturePosition(State.GetMyFirstShip(), i);
                if (IsDeadLocation(futureLocation))
                {
                    return true;
                }
            }

            return false;
        }

        private bool IsDeadLocation(Point location)
        {
            return Math.Abs(location.X) <= State.StarSize ||
                Math.Abs(location.X) >= State.ArenaSize ||
                Math.Abs(location.Y) <= State.StarSize ||
                Math.Abs(location.Y) >= State.ArenaSize;

        }

        private void HoverOnStartStrategy()
        {
            Ship ship = State.GetMyFirstShip();
            Point thrust = new Point(0, 0);

            // Am I on a diagonal?
            if (ship.Position.X == ship.Position.Y)
            {
                int xDir = ship.Position.X > 0 ? -1 : 1;
                int yDir = ship.Position.Y > 0 ? -1 : 1;
                thrust = new Point(xDir, yDir);
            }
            else
            {
                // Not on a diagonal - fire in the direction where I'm furthest from the planet.
                if (Math.Abs(ship.Position.X) < Math.Abs(ship.Position.Y))
                {
                    int yDir = ship.Position.Y > 0 ? -1 : 1;
                    thrust = new Point(0, yDir);
                }
                else
                {
                    int xDir = ship.Position.X > 0 ? -1 : 1;
                    thrust = new Point(xDir, 0);
                }
            }

            Command(Thrust(ship.ID, thrust));
        }
    }
}
