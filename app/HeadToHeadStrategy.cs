using Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static Core.Library;
using System.Threading.Tasks;

namespace app
{
    public class HeadToHeadStrategy : BaseInteractStrategy
    {
        string AttackAI;
        string DefendAI;
        public GameInteractStrategy AttackBot;
        public GameInteractStrategy DefendBot;

        public Action<GameState> AttackStep;
        public Action<GameState> DefendStep;

        public override bool IsStarted() { return AttackBot.IsStarted() && DefendBot.IsStarted(); }
        public override bool IsRunning() { return AttackBot.IsRunning() && DefendBot.IsRunning(); }
        public override bool IsFinished() { return AttackBot.IsFinished() && DefendBot.IsFinished(); }

        public HeadToHeadStrategy(Sender sender, string attackAI, string defendAI) : base(sender)
        {
            AttackAI = "app." + attackAI;
            DefendAI = "app." + defendAI;

            var players = Create();

            var attack = players.Item1;
            var defend = players.Item2;

            AttackBot = (GameInteractStrategy)Activator.CreateInstance(
                Type.GetType(AttackAI),
                new Object[] { sender, attack, 0 }
            );

            DefendBot = (GameInteractStrategy)Activator.CreateInstance(
                Type.GetType(DefendAI),
                new Object[] { sender, defend, 1 }
            );
        }

        public HeadToHeadStrategy(Sender sender) : this(sender, "GameInteractStrategy", "GameInteractStrategy")
        {
        }

        public override GameState Start()
        {
            var attack = Task<GameState>.Factory.StartNew(AttackBot.Start);
            var defend = Task<GameState>.Factory.StartNew(DefendBot.Start);
            attack.Wait();
            defend.Wait();

            AttackBot.Game = attack.Result;
            DefendBot.Game = defend.Result;

            if (AttackStep != null) AttackStep(attack.Result);
            if (DefendStep != null) DefendStep(defend.Result);

            Game = attack.Result;

            return attack.Result;
        }

        public override GameState Next(GameState state)
        {
            var attack = Task<GameState>.Factory.StartNew(() => {return AttackBot.Next(state); });
            var defend = Task<GameState>.Factory.StartNew(() => {return DefendBot.Next(state); });
            attack.Wait();
            defend.Wait();

            AttackBot.Game = attack.Result;
            DefendBot.Game = defend.Result;

            if (AttackStep != null) AttackStep(attack.Result);
            if (DefendStep != null) DefendStep(defend.Result);

            return attack.Result;
        }
    }
}
