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

        public override void Start()
        {
            Join();

            if (IsDone) return;

            Initialize((int)State.TotalPoints - 3, 0, 0, 1);

            var end = State.CurrentTurn + 6;
            for (long i = State.CurrentTurn; i < end; i++)
            {
                if (IsDone) return;

                var ship = State.GetMyFirstShip();

                var opposite = new Point(Math.Sign(ship.Position.X)*-1, Math.Sign(ship.Position.Y)*-1);
                var ninetyDegrees = new Point(opposite.Y, -opposite.X);
                
                Command(UtilityFunctions.MakeList(Thrust(State.GetMyFirstShip().ID, ninetyDegrees)));
                
                //Console.WriteLine($"-- Turn {i} --");
                //Console.WriteLine(State);
            }

            for (long i = State.CurrentTurn; i < State.TotalTurns; i++)
            {
                if (IsDone) return;

                Command(UtilityFunctions.MakeList(Nil));

                //Console.WriteLine($"-- Turn {i} --");
                //Console.WriteLine(State);
            }
        }
    }
}
