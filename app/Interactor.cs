using Core;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace app
{
    class Interactor
    {
        public static void Interact(IProtocol protocol, object state, object vector)
        {
            var response = protocol.call(state, vector);

            if (response.Flag == 0)
            {
                //   then(modem(response.NewState), multipledraw(response.Data))
            }
            else
            {
                //   else interact(protocol, Modem(response.NewState), send(response.Data))
            }
        }

        private static object Modem(object state)
        {
            // What does modem do??? It looks like a noop...
            return state;
        }

        public static async Task<int> xMain(string[] args)
        {
            return 0;
        }
    }
}
