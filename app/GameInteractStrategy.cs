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
        public GameInteractStrategy(Sender sender, Value player, int playerId) : base(sender, player, playerId)
        {
        }

        public override GameState Start()
        {
            return base.Start(1, 1, 1, 1);
        }

        public override GameState Next(GameState next)
        {
            return Command(CMD, Player, NilList);
        }
    }
}
