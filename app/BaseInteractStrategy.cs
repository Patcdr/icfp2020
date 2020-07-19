using Core;
using static Core.Library;
using System;
using System.Collections.Generic;
using System.Text;

namespace app
{
    public abstract class BaseInteractStrategy
    {
        public Interactor Interactor { get; }
        public readonly IProtocol Protocol;

        public Action<Value> Step;

        public Value Player;
        public Value Game;
        public Value Local;
        public IList<DrawFrame> Frames;

        public static readonly Value NULL = new Number(0);
        public static readonly Value ASK = new Number(1);
        public static readonly Value JOIN = new Number(2);
        public static readonly Value START = new Number(3);
        public static readonly Value CMD = new Number(4);


        public virtual bool IsStarted() { return Game.Cdr().Car().AsNumber() > 0; }
        public virtual bool IsRunning() { return Game.Cdr().Car().AsNumber() == 1; }
        public virtual bool IsFinished() { return Game.Cdr().Car().AsNumber() == 2; }
        public Value Turn { get { return UtilityFunctions.Addr("dddaa", Game); } }

        public BaseInteractStrategy(Sender sender) : this(sender, null)
        {
        }

        public BaseInteractStrategy(Sender sender, Value player)
        {
            Interactor = new Interactor(sender);
            Player = player;
            Protocol = new GalaxyProtocol();
            Boot();
        }

        public virtual int Run()
        {
            Game = Start();

            // Play 256 rounds or until the game is over.
            for (var i = 0; i < 256; i++)
            {
                Console.WriteLine($"Turn {Turn} {Game}");

                if (!IsRunning()) {
                    Console.WriteLine("Game OVER!");
                    return i;
                }

                Game = Next(new GameState(Game));

                if (Step != null) Step(Game);
            }
            return 256;
        }

        public virtual IList<DrawFrame> TakeStep()
        {
            Game = Next(new GameState(Game));
            return Frames;
        }

        public virtual Value Start(int a, int b, int c, int d)
        {
            var player = Command(JOIN, Player, NilList);

            var started = Command(START, Player,
                UtilityFunctions.MakeList(new int[] {a, b, c, d}));

            if (Step != null) Step(started);

            return started;
        }

        public virtual Value Start()
        {
            // TODO: Be smarter about choosing values
            return Start(1, 1, 1, 1);
        }

        public abstract Value Next(GameState state);

        public Value Ask()
        {
            Interact(36, 0);
            Interact(44, 0);
            return C(
                UtilityFunctions.Addr("dadddadaada", Local),
                UtilityFunctions.Addr("dadddadadada", Local)
            );
        }

        public Value Command(params Value[] command)
        {
            var result = Interactor.sender.Send(command, Player);
            return Interact(result);
        }

        public Value Interact(int x, int y)
        {
            return Interact(C(N(x), N(y)));
        }

        public Value Interact(Value next)
        {
            Console.WriteLine("Next: " + next);
            var result = Interactor.Interact(Protocol, Local, next);
            Local = result.NewState;
            Frames = result.MultiDrawResult;
            // UtilityFunctions.PrettyPrint(Local, null);
            return Interactor.LastResponse;
        }

        private void Boot() {
            Local = C(N(5), C(C(N(2), C(N(0), C(Nil, C(Nil, C(Nil, C(Nil, C(Nil, C(N(38273), Nil)))))))), C(N(9), C(Nil, Nil))));
        }
        Value C(Value a, Value b) { return new ConsIntermediate2(a, b); }
        Value N(int a) { return new Number(a); }
    }
}
