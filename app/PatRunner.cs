using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using Core;
using static Core.Library;

namespace app
{
    public class PatRunner : BaseRunner
    {
        public PatRunner(Sender sender, long player=0)
            : base(sender, player)
        {
        }

        protected override (int lazers, int cooling, int ships) GetInitialValues(bool isAttacker)
        {
            return (0, 0, 1);
        }

        private int burnSteps = 0;

        public override void Step()
        {
            // Is the game over?
            if (IsDone)
            {
                return;
            }

            if (burnSteps < 6)
            {
                burnSteps++;

                var ship = State.GetMyFirstShip();

                var opposite = new Point(Math.Sign(ship.Position.X)*-1, Math.Sign(ship.Position.Y)*-1);
                var ninetyDegrees = new Point(opposite.Y, -opposite.X);
                
                Command(Thrust(State.GetMyFirstShip().ID, ninetyDegrees));
            }
            else
            {
                Command(Nil);
            }
        }
    }
}
