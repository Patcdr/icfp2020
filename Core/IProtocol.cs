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
            public int Flag { get; set; }

            public object NewState { get; set; }

            public object Data { get; set; }
        }
    }
}
