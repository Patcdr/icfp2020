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

        public Action<Value> AttackStep;
        public Action<Value> DefendStep;

        public override bool IsStarted() { return AttackBot.IsStarted() && DefendBot.IsStarted(); }
        public override bool IsRunning() { return AttackBot.IsRunning() && DefendBot.IsRunning(); }
        public override bool IsFinished() { return AttackBot.IsFinished() && DefendBot.IsFinished(); }


        public HeadToHeadStrategy(Sender sender, Action<Value> step) :
               this(sender, "GameInteractStrategy", "GameInteractStrategy", step)
               {

               }
        public HeadToHeadStrategy(Sender sender, string attackAI, string defendAI) :
               this(sender, attackAI, defendAI, null)
        {}

        public HeadToHeadStrategy(Sender sender, string attackAI, string defendAI, Action<Value> step) : base(sender)
        {
            Step = step;

            AttackAI = "app." + attackAI;
            DefendAI = "app." + defendAI;

            var players = Ask();

            if (Step != null) Step(players);

            var attack = players.Car();
            var defend = players.Cdr();

            AttackBot = (GameInteractStrategy)Activator.CreateInstance(
                Type.GetType(AttackAI),
                new Object[] { sender, attack }
            );

            DefendBot = (GameInteractStrategy)Activator.CreateInstance(
                Type.GetType(DefendAI),
                new Object[] { sender, defend }
            );
        }

        public HeadToHeadStrategy(Sender sender) : this(sender, "GameInteractStrategy", "GameInteractStrategy")
        {
        }

        public override Value Start()
        {
            var attack = Task<Value>.Factory.StartNew(AttackBot.Start);
            var defend = Task<Value>.Factory.StartNew(DefendBot.Start);
            attack.Wait();
            defend.Wait();

            AttackBot.Game = attack.Result;
            DefendBot.Game = defend.Result;

            if (Step != null) Step(attack.Result);
            if (AttackStep != null) AttackStep(attack.Result);
            if (DefendStep != null) DefendStep(defend.Result);

            Game = attack.Result;

            return attack.Result;
        }

        public override Value Next(GameState state)
        {
            var attack = Task<Value>.Factory.StartNew(() => {return AttackBot.Next(null); });
            var defend = Task<Value>.Factory.StartNew(() => {return DefendBot.Next(null); });
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
