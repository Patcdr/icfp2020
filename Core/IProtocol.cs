using System;
using System.Collections.Generic;
using System.Text;

namespace Core
{
    /// <summary>
    /// A protocol is a function that takes an opaque state that should initially be null
    /// and a point. It then returns a result consisting of a Flag, NewState, and Data.
    /// 
    /// The Flag indicates whether the protocol should be called again (0 means don't call again).
    /// The NewState is the state that should be passed to the protocol on the next invocation.
    /// The Data is a set of points that should be drawn when Flag is 0, or passed to Send
    /// otherwise. Send performs some unknown operation on the Data and returns a point (probably).
    /// </summary>
    public interface IProtocol
    {
        /// <summary>
        /// Invokes the protocol. The state is opaque outside of the protocol and
        /// is just a way for the protocol to pass data to later invocations of itself
        /// if desired.
        /// </summary>
        /// <param name="state">The protocol-dependent state</param>
        /// <param name="point">The (x, y) point to input</param>
        /// <returns>A Response containing the protocol's result</returns>
        Response call(object state, IList<int> point);

        public class Response
        {
            // A flag that when 0, terminates interact and causes it to draw all Data
            public int Flag { get; set; }

            public object NewState { get; set; }

            // A vector of vector of vectors, where the most inner vector are tuples
            // representing points. In other words, a list of lists of points.
            // Each list of points will be drawn on a separate
            // This entire value will be passed to Drawer.MultipleDraw when Flag == 0.
            public IList<IList<IList<int>>> Data { get; set; }
        }
    }
}
