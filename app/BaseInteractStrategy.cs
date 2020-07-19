using Core;
using static Core.Library;
using System;
using System.Collections.Generic;
using System.Text;

namespace app
{
    public abstract class BaseInteractStrategy
    {
        protected Interactor Interactor { get; }

        public Action<Value> Step;

        public Value Player;
        public Value Game;

        public Value State
        {
            get {
                return Game.Cdr().Cdr().Cdr();
            }
        }

        public static readonly Value NULL = new Number(0);
        public static readonly Value ASK = new Number(1);
        public static readonly Value JOIN = new Number(2);
        public static readonly Value START = new Number(3);
        public static readonly Value CMD = new Number(4);


        public virtual bool IsStarted() { return Game.Cdr().Car().AsNumber() > 0; }
        public virtual bool IsRunning() { return Game.Cdr().Car().AsNumber() == 1; }
        public virtual bool IsFinished() { return Game.Cdr().Car().AsNumber() == 2; }

        public BaseInteractStrategy(Interactor interactor)
        {
            Interactor = interactor;
        }

        public BaseInteractStrategy(Interactor interactor, Value player)
        {
            Interactor = interactor;
            Player = player;
        }

        public virtual int Run()
        {
            Start();

            // Play 256 rounds or until the game is over.
            for (var i = 0; i < 300; i++)
            {
                if (!IsRunning()) {
                    Console.WriteLine("Game OVER!");
                    return i;
                }

                Console.WriteLine($"Turn {i}");
                Game = Next();

                if (Step != null) Step(Game);
            }
            return 256;
        }

        public virtual void Start(int a, int b, int c, int d)
        {
            Game = Interactor.sender.Send(new Value[] {
                JOIN, Player, NilList
            }, Player);

            Game = Interactor.sender.Send(new Value[] {
                START, Player, UtilityFunctions.MakeList(new int[] {
                    a, b, c, d
                })
            }, Player);

            if (Step != null) Step(Game);
        }

        public virtual void Start()
        {
            // TODO: Be smarter about choosing values
            Start(1, 1, 1, 1);
        }

        public abstract Value Next();
    }
}
