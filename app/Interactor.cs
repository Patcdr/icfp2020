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

        public static Result Interact(IProtocol protocol)
        {
            // Start the protocol passing nil as the initial state and (0, 0) as the initial point
            return Interact(protocol, null, new List<int> { 0, 0 });
        }

        // TODO: it is unclear whether Send will return more than a single point, but all
        // examples show that Interact takes a point as its last argument.
        public static Result Interact(IProtocol protocol, object state, IList<int> point)
        {
            var response = protocol.call(state, point);
            var newState = Modem(response.NewState);

            if (response.Flag == 0)
            {
                return new Result()
                {
                    NewState = newState,
                    MultiDrawResult = Drawer.MultipleDraw(response.Data),
                };
            }
            else
            {
                // This is the way it is defined. If we need to we can convert
                // convert this from tail recursion to a loop.
                return Interact(protocol, newState, Send(response.Data));
            }
        }

        // ap modem x0 = ap dem ap mod x0
        private static T Modem<T>(T state)
        {
            // What does modem do??? It looks like a noop...does it have side effects?
            return state;
        }

        private static IList<int> Send<T>(T data)
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
