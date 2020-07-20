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
        Random r = new Random();
        public DontDieRunner(Sender sender, long player = 0)
            : base(sender, player, 0, 8, 1)
        {
        }

        public override void Step()
        {
            if (IsDone) return;

            if (State.CurrentTurn < 6)
            {
                StartOrbitStrategy();
            }
            else
            {
                AvoidDeathStrategy();
            }
        }

        private void StartOrbitStrategy()
        {
            var ship = State.GetMyFirstShip();
            var opposite = new Point(Math.Sign(ship.Position.X) * -1, Math.Sign(ship.Position.Y) * -1);
            var ninetyDegrees = new Point(opposite.Y, -opposite.X);
            Command(Thrust(State.GetMyFirstShip().ID, ninetyDegrees));
        }

        private void AvoidDeathStrategy()
        {
            int lookaheadTurns = 8;
            Ship ship = State.GetMyFirstShip();
            bool thrusted = false;

            // If we would die by not thrusting
            if (TurnsTilDeath(lookaheadTurns, new Point(0, 0)) != int.MaxValue)
            {
                int longestLife = 0;
                Point thrust = new Point(0, 0);
                foreach (Point p in ActionHandler.AllDirections)
                {
                    int currentLife = TurnsTilDeath(lookaheadTurns, p);
                    if (currentLife > longestLife)
                    {
                        longestLife = currentLife;
                        thrust = p;
                    }
                    else if (currentLife == longestLife)
                    {
                        // If it's a tie, prefer the thrust that is most in the direction we're currently going.
                        int bestDotProduct = ship.Velocity.X * thrust.X + ship.Velocity.Y * thrust.Y;
                        int currentDotProduct = ship.Velocity.X * p.X + ship.Velocity.Y * p.Y;
                        if (currentDotProduct > bestDotProduct)
                        {
                            longestLife = currentLife;
                            thrust = p;
                        }
                    }
                }

                if (thrust != new Point(0, 0))
                {
                    Command(Thrust(ship.ID, thrust));
                    thrusted = true;
                }
            }

            if (!thrusted)
            {
                // Note: this means I think I've got no way to live longer.
                Command();
            }
        }

        private int TurnsTilDeath(int lookAheadTurns, Point thrust)
        {
            for (int i = 1; i <= lookAheadTurns; i++)
            {
                Point futureLocation = ShipPositionSimulator.FuturePosition(State.GetMyFirstShip(), i, thrust);
                if (IsDeadLocation(futureLocation))
                {
                    return i;
                }
            }

            return int.MaxValue;
        }

        private bool IsDeadLocation(Point location)
        {
            return (Math.Abs(location.X) <= State.StarSize && Math.Abs(location.Y) <= State.StarSize) ||
                Math.Abs(location.X) >= State.ArenaSize ||
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
