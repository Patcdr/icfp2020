using System;
using Core;
using static Core.Library;
using System.Threading.Tasks;

namespace app
{
    class DoubleRunner
    {
        public static readonly Value NULL = new Number(0);
        public static readonly Value ASK = new Number(1);

        private BaseRunner Attacker;
        private BaseRunner Defender;

        public DoubleRunner(Sender sender, BaseRunner attacker, BaseRunner defender)
        {
            var players = sender.Send(new Value[] {ASK, NULL});

            UtilityFunctions.PrettyPrint(players);

            Attacker = attacker;
            Attacker.SetPlayer((Number)UtilityFunctions.Addr("daada", players));
            Defender = defender;
            Defender.SetPlayer((Number)UtilityFunctions.Addr("dadada", players));
        }

        public GameState Start()
        {
            var attack = Task.Factory.StartNew(Attacker.Start);
            var defend = Task.Factory.StartNew(Defender.Start);

            attack.Wait();
            defend.Wait();

            return Attacker.State;
        }
    }
}
