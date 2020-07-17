using Core;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace app
{
    class Interactor
    {

        public static void Interact(IProtocol protocol)
        {
            // Start the protocol passing nil as the initial state and(0, 0) as the initial point.
            object state = null;
            object vector = new List<int> { 0, 0 };

            // Then iterate the protocol passing new points along with states obtained from the previous execution.
            do
            {
                Result result = Interact(protocol, state, vector);
                state = result.NewState;
                // TODO: what is the new vector???

                // TODO: when do we stop???
            } while (true);
        }

        public static Result Interact(IProtocol protocol, object state, object vector)
        {
            var response = protocol.call(state, vector);

            if (response.Flag == 0)
            {
                return new Result()
                {
                    NewState = Modem(response.NewState),
                    MultiDrawResult = Drawer.MultipleDraw(response.Data),
                };
            }
            else
            {
                // This is the way it is defined. If we need to we can convert
                // convert this from tail recursion to a loop.
                return Interact(protocol, Modem(response.NewState), Send(response.Data));
            }
        }

        // ap modem x0 = ap dem ap mod x0
        private static object Modem(object state)
        {
            // What does modem do??? It looks like a noop...does it have side effects?
            return state;
        }

        private static object Send(IList<IList<IList<int>>> data)
        {
            // TODO: replace this function with the real version
            return null;
        }

        // Result of an interaction
        public class Result
        {
            public object NewState { get; set; }

            public IList<object> MultiDrawResult { get; set; }
        }

    }
}
