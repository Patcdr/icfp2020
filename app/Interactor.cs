using Core;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace app
{
    public class Interactor
    {
        public readonly Sender sender;

        public Interactor(Sender sender)
        {
            this.sender = sender;
        }

        public Result Interact(IProtocol protocol)
        {
            // Start the protocol passing nil as the initial state and (0, 0) as the initial point
            return Interact(protocol, new NilClass(), new ConsIntermediate2(new Number(0), new Number(0)));
        }

        // TODO: it is unclear whether Send will return more than a single point, but all
        // examples show that Interact takes a point as its last argument.
        public Result Interact(IProtocol protocol, Value state, Value point)
        {

            var response = protocol.call(state, point);
            var newState = Modem(response.NewState);

            if (response.Flag == 0)
            {
                var drawFrames = UtilityFunctions.MultipleDraw(response.Data);

                // TODO: maybe remove this once the squiggleizer is done?
                //ConsoleDrawer.MultipleDraw(drawFrames);

                return new Result()
                {
                    NewState = newState,
                    MultiDrawResult = drawFrames,
                    RawData = response.Data,
                    Flag = response.Flag,
                };
            }
            else
            {
                // This is the way it is defined. If we need to we can
                // convert this from tail recursion to a loop.
                return Interact(protocol, newState, sender.Send(response.Data));
            }
        }

        // ap modem x0 = ap dem ap mod x0
        private static T Modem<T>(T state)
        {
            // TODO: Modem is a noop, but should have a side effect of
            // validating that the state is modulatable (i.e. composed only of lists and numbers)
            return state;
        }

        // Result of an interaction
        public class Result
        {
            public Value NewState { get; set; }

            public IList<DrawFrame> MultiDrawResult { get; set; }

            public Value RawData { get; set; }

            public long Flag { get; set; }
        }

    }
}
