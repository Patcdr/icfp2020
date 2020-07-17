using System;

namespace Core
{
    public class Library
    {
        public static readonly FalseClass FalseVal = new FalseClass();
        public static readonly TrueClass TrueVal = new TrueClass();
        public static readonly IncrementClass Increment = new IncrementClass();
        public static readonly DecrementClass Decrement = new DecrementClass();
        public static readonly NegateClass Negate = new NegateClass();
        public static readonly Power2Class Power2 = new Power2Class();
        public static readonly IdentityClass Identity = new IdentityClass();
        public static readonly Value Nil = new ConstantFunction(TrueVal);
    }

    public abstract class Value : Node
    {
        public virtual Value Invoke(Value val)
        {
            throw new NotImplementedException();
        }

        public virtual long AsNumber()
        {
            throw new NotImplementedException();
        }
    }

    public class Number : Value
    {
        private long n;

        public Number(long n)
        {
            this.n = n;
        }

        public override long AsNumber()
        {
            return n;
        }
    }

    public class ConstantFunction : Value
    {
        private Value val;

        public ConstantFunction(Value val)
        {
            this.val = val;
        }

        public override Value Invoke(Value _)
        {
            return val;
        }
    }

    public class TrueClass : Value
    {
        public override Value Invoke(Value val)
        {
            return new ConstantFunction(val);
        }
    }

    public class FalseClass : Value
    {
        public override Value Invoke(Value _)
        {
            return Library.Identity;
        }
    }

    public class IncrementClass : Value
    {
        public override Value Invoke(Value val)
        {
            return new Number(val.AsNumber() + 1);
        }
    }

    public class DecrementClass : Value
    {
        public override Value Invoke(Value val)
        {
            return new Number(val.AsNumber() - 1);
        }
    }

    public class NegateClass : Value
    {
        public override Value Invoke(Value val)
        {
            return new Number(-val.AsNumber());
        }
    }

    public class Power2Class : Value
    {
        /// <summary>
        /// There is an assumption that val is an int
        /// </summary>
        public override Value Invoke(Value val)
        {
            return new Number(1L << (int)val.AsNumber());
        }
    }

    public class IdentityClass : Value
    {
        public override Value Invoke(Value val)
        {
            return val;
        }
    }
}
