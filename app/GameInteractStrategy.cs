using Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static Core.Library;

namespace app
{
    class GameInteractStrategy : BaseInteractStrategy
    {
        public static readonly Value JOIN = new Number(2);
        public static readonly Value START = new Number(3);
        public static readonly Value CMD = new Number(4);
        public readonly Value Player;

        public GameInteractStrategy(Interactor interactor, int playerKey) : base(interactor, playerKey)
        {
            Player = new Number(playerKey);
        }

        public override void Execute()
        {
            var game = Interactor.sender.Send(new Value[] {
                JOIN, Player, NilList
            });

            game = Interactor.sender.Send(new Value[] {
                START, Player, UtilityFunctions.MakeList(new int[] {
                    0, 0, 0, 0
                })
            });

            game = Interactor.sender.Send(new Value[] {
                CMD, Player, NilList
            });

            // while (true)
            for (var i = 0; i < 10; i++)
            {
                game = Interactor.sender.Send(new Value[] {
                    CMD, Player, NilList
                });
            }
        }
    }
}
