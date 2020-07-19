using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using Core;
using static Core.Library;

namespace app
{
    class PatRunner
    {

        public static readonly Value NULL = new Number(0);
        public static readonly Value ASK = new Number(1);
        public static readonly Value JOIN = new Number(2);
        public static readonly Value START = new Number(3);
        public static readonly Value CMD = new Number(4);

        private Sender sender;
        private Number Player;

        public PatRunner(Sender sender, long player=0)
        {
            this.sender = sender;
            this.Player = new Number(player);
        }

        public void SetPlayer(Number player)
        {
            Player = player;
        }

        Value C(Value a, Value b) { return new ConsIntermediate2(a, b); }
        Value N(int a) { return new Number(a); }
        Value N(long a) { return new Number(a); }


        public void Start()
        {
            var gameState = new GameState(sender.Send(new Value[] {JOIN, (Player), NilList}));
            // Extract fuel from gamestate
            if (gameState.GameStateVal == 2) return;

            gameState = new GameState(sender.Send(new Value[] {START, Player, UtilityFunctions.MakeList(new int[] {(int)gameState.TotalPoints - 3, 0, 0, 1})}));

            var end = gameState.CurrentTurn + 6;
            for (long i = gameState.CurrentTurn; i < end; i++)
            {
                if (gameState.GameStateVal == 2) return;
                var ship = gameState.GetShipByPlayerId(gameState.PlayerId);

                var opposite = new Point(Math.Sign(ship.Position.X)*-1, Math.Sign(ship.Position.Y)*-1);
                var ninetyDegrees = new Point(opposite.Y, -opposite.X);
                gameState = new GameState(sender.Send(new Value[] { CMD, Player, UtilityFunctions.MakeList(Thrust(gameState.GetShipByPlayerId(gameState.PlayerId).ID, ninetyDegrees)) }));

            }

            for (long i = gameState.CurrentTurn; i < gameState.TotalTurns; i++)
            {
                if (gameState.GameStateVal == 2) return;
                gameState = new GameState(sender.Send(new Value[] { CMD, Player, Nil}));

            }
            // CMD(THRUST, ()), 

        }

        public Value Thrust(long shipId, Point vector)
        {
            return C(N(0), C(N(shipId), C(C(N(vector.X), N(vector.Y)), Nil)));
        }


    }
}
