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
        // Sending:  (1 ,  (0 , nil) )
        public static readonly Value NULL = new Number(0);
        public static readonly Value ASK = new Number(1);
        public static readonly Value JOIN = new Number(2);
        public static readonly Value START = new Number(3);
        public static readonly Value CMD = new Number(4);

        BaseInteractStrategy attackBot;
        BaseInteractStrategy defendBot;

        public HeadToHeadStrategy(Interactor interactor) : base(interactor)
        {
        }

        public override void Execute()
        {
            var players = Interactor.sender.Send(new Value[] {
                ASK, NULL
            });

            var attack = players.Cdr().Car().Car().Cdr().Car();
            var defend = players.Cdr().Car().Cdr().Car().Cdr().Car();

            attackBot = new GameInteractStrategy(Interactor, attack);
            defendBot = new GameInteractStrategy(Interactor, defend);

            var attackExecute = Task.Factory.StartNew(() => attackBot.Execute());
            var defendExecute = Task.Factory.StartNew(() => defendBot.Execute());

            attackExecute.Wait();
            defendExecute.Wait();

            // while (true)
            for (var i = 0; i < 10; i++)
            {
                Console.ReadLine();
                Next();
            }
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
