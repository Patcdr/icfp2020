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

        public BaseRunner Attacker;
        public BaseRunner Defender;

        public DoubleRunner(Sender sender, BaseRunner attacker, BaseRunner defender)
        {
            var players = sender.Send(new Value[] {ASK, NULL});

            UtilityFunctions.PrettyPrint(players);

            Attacker = attacker;
            Attacker.SetPlayer((Number)UtilityFunctions.Addr("daada", players));
            Defender = defender;
            Defender.SetPlayer((Number)UtilityFunctions.Addr("dadada", players));
        }

        public GameState Join()
        {
            return WaitOnBoth(Attacker.Join, Defender.Join);
        }

        public GameState Step()
        {
            return WaitOnBoth(Attacker.Step, Defender.Step);
        }

        private GameState WaitOnBoth(Action atkAction, Action defAction)
        {
            var attack = Task.Factory.StartNew(atkAction);
            var defend = Task.Factory.StartNew(defAction);

            attack.Wait();
            defend.Wait();

            return Attacker.State;
        }
    }
}
