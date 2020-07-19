using System;
using System.Collections.Generic;
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

        public PatRunner(Sender sender, long player)
        {
            this.sender = sender;
            this.Player = new Number(player);
        }

        Value C(Value a, Value b) { return new ConsIntermediate2(a, b); }
        Value N(int a) { return new Number(a); }
        Value N(long a) { return new Number(a); }


        public void Start()
        {
            var gameState = new GameState(sender.Send(new Value[] {JOIN, (Player), NilList}));
            // Extract fuel from gamestate

            gameState = new GameState(sender.Send(new Value[] {START, Player, UtilityFunctions.MakeList(new int[] {1, 1, 1, 1})}));
             
            gameState = new GameState(sender.Send(new Value[] { CMD, Player, C(N(0), C(N(gameState.GetShipByPlayerId(gameState.PlayerId).ID), C(N(-1), N(0)))) }));

            for(long i = gameState.CurrentTurn; i < gameState.TotalTurns; i++)
            {
                gameState = new GameState(sender.Send(new Value[] { CMD, Player, Nil }));

            }

        }


    }
}
