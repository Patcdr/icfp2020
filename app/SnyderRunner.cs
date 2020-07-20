using Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace app
{
    class SnyderRunner : BaseRunner
    {

        (int lazers, int cooling, int ships) initialStats;
        Dictionary<long, ShipBrain> brains;
        public SnyderRunner(
            Sender sender,
            ShipBrain initialBrain,
            (int lazers, int cooling, int ships) initialStats,
            long player = 0)
            : base(sender, player)
        {
        }

        protected override (int lazers, int cooling, int ships) GetInitialValues(bool isAttacker)
        {
            return initialStats;
        }

        public override void Step()
        {
            if (IsDone) return;

            List<Value> commands = new List<Value>();

            foreach (Ship ship in State.Ships)
            {
                commands.AddRange(brains[ship.ID].Step(ship, State));
            }

            Command(commands.ToArray());
        }
    }
}
