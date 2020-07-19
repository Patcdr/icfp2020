using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using Core;
using static Core.Library;

namespace app
{
    public abstract class BaseRunner
    {
        public static readonly Value NULL = new Number(0);
        public static readonly Value ASK = new Number(1);
        public static readonly Value JOIN = new Number(2);
        public static readonly Value START = new Number(3);
        public static readonly Value CMD = new Number(4);

        protected Number Player;
        protected GameState State;

        private Sender Sender;

        public BaseRunner(Sender sender, long player = 0)
        {
            this.Sender = sender;
            this.Player = new Number(player);
        }

        public abstract void Start();

        public void SetPlayer(Number player)
        {
            Player = player;
        }

        // Extract fuel from gamestate
        protected bool IsDone { get { return State.GameStateVal == 2; } }

        protected void Join()
        {
            State = new GameState(Sender.Send(new Value[] { JOIN, (Player), NilList }));
        }

        protected void Initialize(int health, int lazers, int cooling, int babies)
        {
            State = new GameState(Sender.Send(new Value[] { START, Player, UtilityFunctions.MakeList(new int[] { health, lazers, cooling, babies }) }));
        }

        protected void Command(Value commands)
        {
            State = new GameState(Sender.Send(new Value[] { CMD, Player, commands }));
        }

        protected Value C(Value a, Value b) { return new ConsIntermediate2(a, b); }
        protected Value N(int a) { return new Number(a); }
        protected Value N(long a) { return new Number(a); }

        protected Value Thrust(long shipId, Point vector)
        {
            return C(N(0), C(N(shipId), C(C(N(vector.X), N(vector.Y)), Nil)));
        }
    }
}
