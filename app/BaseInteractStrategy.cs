using Core;
using static Core.Library;
using System;
using System.Collections.Generic;
using System.Text;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;

namespace app
{
    public abstract class BaseInteractStrategy
    {
        public Sender Sender { get; }
        public Value Player { get; }
        public GameState Game { get; set; }
        public IList<DrawFrame> Frames;
        public int PlayerId;

        public static readonly Value NULL = new Number(0);
        public static readonly Value CREATE = new Number(1);
        public static readonly Value JOIN = new Number(2);
        public static readonly Value START = new Number(3);
        public static readonly Value CMD = new Number(4);


        public virtual bool IsStarted() { return Game.GameStateVal > 0; }
        public virtual bool IsRunning() { return Game.GameStateVal == 1; }
        public virtual bool IsFinished() { return Game.GameStateVal == 2; }
        public long Turn { get { return Game.CurrentTurn; } }

        public BaseInteractStrategy(Sender sender) : this(sender, null, 0)
        {
        }

        public BaseInteractStrategy(Sender sender, Value player, int playerId)
        {
            Sender = sender;
            Player = player;
            PlayerId = playerId;
        }

        public virtual int Run()
        {
            Game = Start();

            // Play 256 rounds or until the game is over.
            for (var i = 0; i < 256; i++)
            {
                Console.WriteLine($"Turn {Turn}");
                Console.WriteLine(Game);

                if (!IsRunning()) {
                    Console.WriteLine("Game OVER!");
                    return i;
                }

                Game = Next(Game);
            }
            return 256;
        }

        public virtual IList<DrawFrame> TakeStep()
        {
            Game = Next(Game);
            return Frames;
        }

        public virtual GameState Start(int a, int b, int c, int d)
        {
            Game = Join();

            var vals = new Value[] { new Number(a), new Number(b), new Number(c), new Number(d) };
            var gameState = Sender.Send(new Value[] { START, Player, UtilityFunctions.MakeList(vals) });

            return new GameState(gameState);
        }

        public virtual GameState Start()
        {
            // TODO: Be smarter about choosing values
            return Start(1, 1, 1, 1);
        }

        public abstract GameState Next(GameState state);

        // Returns (player1Key, player2Key)
        public Tuple<Value, Value> Create()
        {
            // CREATE
            var createResponse = Sender.Send(new Value[] { CREATE, new Number(0) });

            return new Tuple<Value, Value>(
                UtilityFunctions.Addr("cdaadar", createResponse),
                UtilityFunctions.Addr("cdadadar", createResponse)
            );
        }

        public GameState Join()
        {
            // JOIN
            var gameState = Sender.Send(new Value[] { JOIN, Player, NilList });
            return new GameState(gameState);
        }

        public GameState Command(params Value[] command)
        {
            var gameState = Sender.Send(command, Player);
            return new GameState(gameState);
        }

    }
}
