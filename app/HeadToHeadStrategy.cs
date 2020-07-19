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


        public HeadToHeadStrategy(Interactor interactor, Action<Value> step) :
               this(interactor, "GameInteractStrategy", "GameInteractStrategy", step)
               {

               }
        public HeadToHeadStrategy(Interactor interactor, string attackAI, string defendAI) :
               this(interactor, attackAI, defendAI, null)
        {}

        public HeadToHeadStrategy(Interactor interactor, string attackAI, string defendAI, Action<Value> step) : base(interactor)
        {
            Step = step;

            AttackAI = "app." + attackAI;
            DefendAI = "app." + defendAI;

            var players = Interactor.sender.Send(new Value[] {
                ASK, NULL
            });

            if (Step != null) Step(players);

            var attack = players.Cdr().Car().Car().Cdr().Car();
            var defend = players.Cdr().Car().Cdr().Car().Cdr().Car();

            AttackBot = (GameInteractStrategy)Activator.CreateInstance(
                Type.GetType(AttackAI),
                new Object[] { Interactor, attack }
            );

            DefendBot = (GameInteractStrategy)Activator.CreateInstance(
                Type.GetType(DefendAI),
                new Object[] { Interactor, defend }
            );
        }

        public HeadToHeadStrategy(Interactor interactor) : this(interactor, "GameInteractStrategy", "GameInteractStrategy")
        {
        }

        public override void Start()
        {
            var attackExecute = Task.Factory.StartNew(() => AttackBot.Start());
            var defendExecute = Task.Factory.StartNew(() => DefendBot.Start());
            attackExecute.Wait();
            defendExecute.Wait();

            if (Step != null) Step(null);
            if (AttackStep != null) AttackStep(AttackBot.Game);
            if (DefendStep != null) DefendStep(DefendBot.Game);
        }

        public override Value Next(GameState state)
        {
            var attackNext = Task.Factory.StartNew(() => AttackBot.Next(null));
            var defendNext = Task.Factory.StartNew(() => DefendBot.Next(null));
            attackNext.Wait();
            defendNext.Wait();

            if (AttackStep != null) AttackStep(AttackBot.Game);
            if (DefendStep != null) DefendStep(DefendBot.Game);

            return null;
        }
    }
}
