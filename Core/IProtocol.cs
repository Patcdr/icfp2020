using System;
using System.Collections.Generic;
using System.Text;

namespace Core
{
    public interface IProtocol
    {
        Response call(object state, object vector);

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
