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

        public static readonly Value ACCELERATE = new Number(0);
        public static readonly Value DETONATE = new Number(1);
        public static readonly Value SHOOT = new Number(2);
        public static readonly Value SPLIT = new Number(3);

        protected Number Player { get; private set; }
        protected GameState State { get; private set; }

        private readonly Sender Sender;

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

        #region Interactions with Server (changes state)

        protected void Join()
        {
            State = new GameState(Sender.Send(new Value[] { JOIN, (Player), NilList }));
        }

        protected void Initialize(int lazers, int cooling, int ships)
        {
            if (ships < 1) throw new ArgumentException("Ships must be at least 1");

            int health = (int)State.TotalPoints - (4 * lazers) - (12 * cooling) - (2 * ships);

            State = new GameState(Sender.Send(new Value[] { START, Player, UtilityFunctions.MakeList(new int[] { health, lazers, cooling, ships }) }));
        }

        protected void Command(params Value[] commands)
        {
            State = new GameState(Sender.Send(new Value[] { CMD, Player, UtilityFunctions.MakeList(commands) }));
        }

        #endregion

        #region Commands

        protected Value Thrust(long shipId, Point vector)
        {
            return C(ACCELERATE, C(N(shipId), C(C(N(vector.X), N(vector.Y)), Nil)));
        }

        protected Value Detonate(long shipId)
        {
            return C(DETONATE, C(N(shipId), Nil));
        }

        protected Value Shoot(long shipId, Point vector, long hamburger)
        {
            return C(SHOOT, C(N(shipId), C(C(N(vector.X), N(vector.Y)), C(N(hamburger), Nil))));
        }

        //Split: [3, ship_id, (fuel, hamburger, cooling, babies)] (Properties are given in a nil-terminated list and are 0, 1, 2, 4, 8, or 16.)
        protected Value Split(long shipId, int fuel, int hamburger, int cooling, int babies)
        {
            return C(SPLIT, C(N(shipId), UtilityFunctions.MakeList(new int[] { fuel, hamburger, cooling, babies })));
        }

        #endregion

        #region Helper Functions

        protected Value C(Value a, Value b) { return new ConsIntermediate2(a, b); }
        protected Value N(int a) { return new Number(a); }
        protected Value N(long a) { return new Number(a); }

        #endregion
    }
}
