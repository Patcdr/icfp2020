using Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static Core.Library;

namespace app
{
    class GameInteractStrategy : HeadToHeadStrategy
    {
        public Value Player;
        public Value Game;

        public GameInteractStrategy(Interactor interactor, Value player) : base(interactor)
        {
            Player = player;
        }

        public override void Execute()
        {
            Game = Interactor.sender.Send(new Value[] {
                JOIN, Player, NilList
            }, Player);

            Game = Interactor.sender.Send(new Value[] {
                START, Player, UtilityFunctions.MakeList(new int[] {
                    0, 0, 0, 0
                })
            }, Player);
        }

        public override void Next()
        {
            Game = Interactor.sender.Send(new Value[] {
                CMD, Player, NilList
            }, Player);
        }
    }
}
