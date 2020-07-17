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
        public static readonly Value IsNil = new IsNilClass();
        public static readonly Value Add = new AddClass();
        public static readonly Value Mult = new MultClass();
        public static readonly Value Divide = new DivideClass();
        public static readonly Value EqualVal = new EqualsClass();
        public static readonly Value LessThan = new LessThanClass();
        public static readonly Value IfZero = new EqualsConstant(0);
        public static readonly Value SCombo = new SClass();
        public static readonly Value BCombo = new BClass();
        public static readonly Value CCombo = new CClass();
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

    public class IsNilClass : Value
    {
        private static readonly Value EvaluationFunTime = new ConstantFunction(new ConstantFunction(Library.FalseVal));
        
        /// <summary>
        /// Here there be dragons. (if not nil or cons)
        /// </summary>
        public override Value Invoke(Value val)
        {
            return val.Invoke(EvaluationFunTime);
        }
    }

    public class AddClass : Value
    {
        public override Value Invoke(Value val)
        {
            return new AddConstant(val.AsNumber());
        }
    }

    public class AddConstant : Value
    {
        private readonly long n;

        public AddConstant(long n)
        {
            this.n = n;
        }

        public override Value Invoke(Value val)
        {
            return new Number(n + val.AsNumber());
        }
    }

    public class MultClass : Value
    {
        public override Value Invoke(Value val)
        {
            return new MultConstant(val.AsNumber());
        }
    }

    public class MultConstant : Value
    {
        private readonly long n;

        public MultConstant(long n)
        {
            this.n = n;
        }

        public override Value Invoke(Value val)
        {
            return new Number(n * val.AsNumber());
        }
    }

    public class DivideClass : Value
    {
        public override Value Invoke(Value val)
        {
            return new DivideConstant(val.AsNumber());
        }
    }

    public class DivideConstant : Value
    {
        private readonly long n;

        public DivideConstant(long n)
        {
            this.n = n;
        }

        public override Value Invoke(Value val)
        {
            return new Number(n / val.AsNumber());
        }
    }

    public class EqualsClass : Value
    {
        public override Value Invoke(Value val)
        {
            return new EqualsConstant(val.AsNumber());
        }
    }

    public class EqualsConstant : Value
    {
        private readonly long n;

        public EqualsConstant(long n)
        {
            this.n = n;
        }

        public override Value Invoke(Value val)
        {
            return n == val.AsNumber() ? (Value)Library.TrueVal : Library.FalseVal;
        }
    }

    public class LessThanClass : Value
    {
        public override Value Invoke(Value val)
        {
            return new LessThanConstant(val.AsNumber());
        }
    }

    public class LessThanConstant : Value
    {
        private readonly long n;

        public LessThanConstant(long n)
        {
            this.n = n;
        }

        public override Value Invoke(Value val)
        {
            return n < val.AsNumber() ? (Value)Library.TrueVal : Library.FalseVal;
        }
    }

    public class SClass : Value
    {
        public override Value Invoke(Value val1)
        {
            return new SIntermediate1(val1);
        }
    }

    public class SIntermediate1 : Value
    {
        private readonly Value val1;

        public SIntermediate1(Value val1)
        {
            this.val1 = val1;
        }

        public override Value Invoke(Value val2)
        {
            return new SIntermediate2(val1, val2);
        }
    }

    public class SIntermediate2 : Value
    {
        private readonly Value val1;
        private readonly Value val2;

        public SIntermediate2(Value val1, Value val2)
        {
            this.val1 = val1;
            this.val2 = val2;
        }

        public override Value Invoke(Value val3)
        {
            return val1.Invoke(val3).Invoke(val2.Invoke(val3));
        }
    }

    public class CClass : Value
    {
        public override Value Invoke(Value val1)
        {
            return new CIntermediate1(val1);
        }
    }

    public class CIntermediate1 : Value
    {
        private readonly Value val1;

        public CIntermediate1(Value val1)
        {
            this.val1 = val1;
        }

        public override Value Invoke(Value val2)
        {
            return new CIntermediate2(val1, val2);
        }
    }

    public class CIntermediate2 : Value
    {
        private readonly Value val1;
        private readonly Value val2;

        public CIntermediate2(Value val1, Value val2)
        {
            this.val1 = val1;
            this.val2 = val2;
        }

        public override Value Invoke(Value val3)
        {
            return val1.Invoke(val3).Invoke(val2);
        }
    }

    public class BClass : Value
    {
        public override Value Invoke(Value val1)
        {
            return new BIntermediate1(val1);
        }
    }

    public class BIntermediate1 : Value
    {
        private readonly Value val1;

        public BIntermediate1(Value val1)
        {
            this.val1 = val1;
        }

        public override Value Invoke(Value val2)
        {
            return new BIntermediate2(val1, val2);
        }
    }

    public class BIntermediate2 : Value
    {
        private readonly Value val1;
        private readonly Value val2;

        public BIntermediate2(Value val1, Value val2)
        {
            this.val1 = val1;
            this.val2 = val2;
        }

        public override Value Invoke(Value val3)
        {
            return val1.Invoke(val2.Invoke(val3));
        }
    }
}
