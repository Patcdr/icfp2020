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
            var gameState = Join();

            if (gameState == null) return;

            gameState = Initialize((int)gameState.TotalPoints - 3, 0, 0, 1);

            var end = gameState.CurrentTurn + 6;
            for (long i = gameState.CurrentTurn; i < end; i++)
            {
                if (gameState.GameStateVal == 2) return;
                var ship = gameState.GetMyFirstShip();

                var opposite = new Point(Math.Sign(ship.Position.X)*-1, Math.Sign(ship.Position.Y)*-1);
                var ninetyDegrees = new Point(opposite.Y, -opposite.X);
                gameState = new GameState(sender.Send(new Value[] { CMD, Player, UtilityFunctions.MakeList(Thrust(gameState.GetMyFirstShip().ID, ninetyDegrees)) }));
                //Console.WriteLine($"-- Turn {i} --");
                //Console.WriteLine(gameState);
            }

            for (long i = gameState.CurrentTurn; i < gameState.TotalTurns; i++)
            {
                if (gameState.GameStateVal == 2) return;
                gameState = new GameState(sender.Send(new Value[] { CMD, Player, Nil}));
                //Console.WriteLine($"-- Turn {i} --");
                //Console.WriteLine(gameState);
            }
        }
    }
}
