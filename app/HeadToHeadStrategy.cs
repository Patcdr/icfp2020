using Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static Core.Library;
using System.Threading.Tasks;

namespace app
{
    class HeadToHeadStrategy : BaseInteractStrategy
    {
        string AttackAI;
        string DefendAI;
        GameInteractStrategy attackBot;
        GameInteractStrategy defendBot;

        public override bool IsStarted() { return attackBot.IsStarted() && defendBot.IsStarted(); }
        public override bool IsRunning() { return attackBot.IsRunning() && defendBot.IsRunning(); }
        public override bool IsFinished() { return attackBot.IsFinished() && defendBot.IsFinished(); }

        public HeadToHeadStrategy(Interactor interactor, string attackAI, string defendAI) : base(interactor)
        {
            AttackAI = "app." + attackAI;
            DefendAI = "app." + defendAI;
        }

        public HeadToHeadStrategy(Interactor interactor) : this(interactor, "GameInteractStrategy", "GameInteractStrategy")
        {
        }

        public override void Start()
        {
            var players = Interactor.sender.Send(new Value[] {
                ASK, NULL
            });

            var attack = players.Cdr().Car().Car().Cdr().Car();
            var defend = players.Cdr().Car().Cdr().Car().Cdr().Car();


            attackBot = (GameInteractStrategy)Activator.CreateInstance(
                Type.GetType(AttackAI),
                new Object[] { Interactor, attack }
            );
            defendBot = (GameInteractStrategy)Activator.CreateInstance(
                Type.GetType(DefendAI),
                new Object[] { Interactor, defend }
            );

            var attackExecute = Task.Factory.StartNew(() => attackBot.Start());
            var defendExecute = Task.Factory.StartNew(() => defendBot.Start());

            attackExecute.Wait();
            defendExecute.Wait();
        }

        public override void Next()
        {
            var attackNext = Task.Factory.StartNew(() => attackBot.Next());
            var defendNext = Task.Factory.StartNew(() => defendBot.Next());
            attackNext.Wait();
            defendNext.Wait();
        }
    }
}
