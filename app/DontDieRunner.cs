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
                
                // Am I on a diagonal?
                if (ship.Position.X == ship.Position.Y)
                {
                    int xDir = ship.Position.X > 0 ? -1 : 1;
                    int yDir = ship.Position.Y > 0 ? -1 : 1;
                    Command(Thrust(ship.ID, new Point(xDir, yDir)));
                }
                else
                {
                    // Not on a diagonal - fire in the direction where I'm closest to the planet.
                    if (Math.Abs(ship.Position.X) > Math.Abs(ship.Position.Y))
                    {
                        int yDir = ship.Position.Y > 0 ? -1 : 1;
                        Command(Thrust(ship.ID, new Point(0, yDir)));
                    }
                    else
                    {
                        int yDir = ship.Position.Y > 0 ? -1 : 1;
                        Command(Thrust(ship.ID, new Point(0, yDir)));
                    }
                }
            }
        }
    }
}
