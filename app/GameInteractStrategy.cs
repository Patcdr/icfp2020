using Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static Core.Library;

namespace app
{
    public class GameInteractStrategy : BaseInteractStrategy
    {
        public GameInteractStrategy(Interactor interactor, Value player) : base(interactor, player)
        {
        }

        public override Value Start()
        {
            return base.Start(1, 1, 1, 1);
        }

        public override Value Next(GameState next)
        {
            return Command(CMD, Player, NilList);
        }
    }
}
