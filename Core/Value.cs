using System;

namespace Core
{
    public abstract class Value : Node
    {
        public Value Invoke(Value val)
        {
            throw new NotImplementedException();
        }

        public long AsNumber()
        {
            throw new NotImplementedException();
        }
    }
}
