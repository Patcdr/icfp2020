using System;
using System.Collections.Generic;

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
        public static readonly Value Nil = new NilClass();
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
        public static readonly Value Cons = new ConsClass();
        public static readonly Value Vec = new ConsClass();
        public static readonly Value Car = new CarClass();
        public static readonly Value Cdr = new CdrClass();
    }

    public abstract class Value : Node
    {
        public static readonly Dictionary<string, Node> EMPTY_ENV = new Dictionary<string, Node>();
        public virtual Value Invoke(Node val, Dictionary<string, Node> environment)
        {
            throw new NotImplementedException();
        }

        /*
        public Value Invoke(Node val)
        {
            return Invoke(val, EMPTY_ENV);
        }*/

        public virtual long AsNumber()
        {
            throw new NotImplementedException();
        }

        public virtual bool IsNumber()
        {
            return false;
        }

        public override Value Evaluate(Dictionary<string, Node> environment)
        {
            return this;
        }
    }

    public class Number : Value
    {
        private readonly long n;

        public Number(long n)
        {
            this.n = n;
        }

        public override long AsNumber()
        {
            return n;
        }

        public override bool IsNumber()
        {
            return true;
        }

        public override string ToString()
        {
            return n.ToString();
        }
    }

    public class ConstantFunction : Value
    {
        private readonly Node val;

        public ConstantFunction(Node val)
        {
            this.val = val;
        }

        private Value evaluateCache = null;

        public override Value Invoke(Node _, Dictionary<string, Node> environment)
        {
            if (evaluateCache == null)
            {
                evaluateCache = val.Evaluate(environment);
            }

            return evaluateCache;
        }

        public override string ToString()
        {
            return "(ap t " + val + ")";
        }
    }

    public class TrueClass : Value
    {
        public override Value Invoke(Node val, Dictionary<string, Node> environment)
        {
            return new ConstantFunction(val);
        }

        public override string ToString()
        {
            return "t";
        }
    }

    public class FalseClass : Value
    {
        public override Value Invoke(Node _, Dictionary<string, Node> environment)
        {
            return Library.Identity;
        }

        public override string ToString()
        {
            return "f";
        }
    }

    public class NilClass : Value
    {
        public override Value Invoke(Node val, Dictionary<string, Node> environment)
        {
            return Library.TrueVal;
        }

        public override string ToString()
        {
            return "nil";
        }
    }

    public class IncrementClass : Value
    {
        public override Value Invoke(Node val, Dictionary<string, Node> environment)
        {
            return new Number(val.Evaluate(environment).AsNumber() + 1);
        }

        public override string ToString()
        {
            return "inc";
        }
    }

    public class DecrementClass : Value
    {
        public override Value Invoke(Node val, Dictionary<string, Node> environment)
        {
            return new Number(val.Evaluate(environment).AsNumber() - 1);
        }

        public override string ToString()
        {
            return "dec";
        }
    }

    public class NegateClass : Value
    {
        public override Value Invoke(Node val, Dictionary<string, Node> environment)
        {
            return new Number(-val.Evaluate(environment).AsNumber());
        }

        public override string ToString()
        {
            return "neg";
        }
    }

    public class Power2Class : Value
    {
        /// <summary>
        /// There is an assumption that val is an int
        /// </summary>
        public override Value Invoke(Node val, Dictionary<string, Node> environment)
        {
            return new Number(1L << (int)val.Evaluate(environment).AsNumber());
        }

        public override string ToString()
        {
            return "pwr2";
        }
    }

    public class IdentityClass : Value
    {
        public override Value Invoke(Node val, Dictionary<string, Node> environment)
        {
            return val.Evaluate(environment);
        }

        public override string ToString()
        {
            return "i";
        }
    }

    public class IsNilClass : Value
    {
        private static readonly Value EvaluationFunTime = new ConstantFunction(new ConstantFunction(Library.FalseVal));

        /// <summary>
        /// Here there be dragons. (if not nil or cons)
        /// </summary>
        public override Value Invoke(Node val, Dictionary<string, Node> environment)
        {
            return val.Evaluate(environment).Invoke(EvaluationFunTime, environment);
        }

        public override string ToString()
        {
            return "isnil";
        }
    }

    public class AddClass : Value
    {
        public override Value Invoke(Node val, Dictionary<string, Node> environment)
        {
            return new AddConstant(val);
        }

        public override string ToString()
        {
            return "add";
        }
    }

    public class AddConstant : Value
    {
        private readonly Node node;
        private bool evaluated;
        private long num;

        public AddConstant(Node node)
        {
            this.node = node;
            evaluated = false;
        }

        public override Value Invoke(Node val, Dictionary<string, Node> environment)
        {
            if (!evaluated)
            {
                num = node.Evaluate(environment).AsNumber();
                evaluated = true;
            }
            return new Number(num + val.Evaluate(environment).AsNumber());
        }


        public override string ToString()
        {
            return $"(ap add {node})";
        }
    }

    public class MultClass : Value
    {
        public override Value Invoke(Node val, Dictionary<string, Node> environment)
        {
            return new MultConstant(val);
        }

        public override string ToString()
        {
            return "mul";
        }
    }

    public class MultConstant : Value
    {
        private readonly Node node;
        private bool evaluated;
        private long num;

        public MultConstant(Node node)
        {
            this.node = node;
            evaluated = false;
        }

        public override Value Invoke(Node val, Dictionary<string, Node> environment)
        {
            if (!evaluated)
            {
                num = node.Evaluate(environment).AsNumber();
                evaluated = true;
            }
            return new Number(num * val.Evaluate(environment).AsNumber());
        }


        public override string ToString()
        {
            return $"(ap mul {node})";
        }
    }

    public class DivideClass : Value
    {
        public override Value Invoke(Node val, Dictionary<string, Node> environment)
        {
            return new DivideConstant(val);
        }

        public override string ToString()
        {
            return "div";
        }
    }

    public class DivideConstant : Value
    {
        private readonly Node node;
        private bool evaluated;
        private long num;

        public DivideConstant(Node node)
        {
            this.node = node;
            evaluated = false;
        }

        public override Value Invoke(Node val, Dictionary<string, Node> environment)
        {
            if (!evaluated)
            {
                num = node.Evaluate(environment).AsNumber();
                evaluated = true;
            }
            return new Number(num / val.Evaluate(environment).AsNumber());
        }


        public override string ToString()
        {
            return $"(ap div {node})";
        }
    }

    public class EqualsClass : Value
    {
        public override Value Invoke(Node val, Dictionary<string, Node> environment)
        {
            return new EqualsConstant(val.Evaluate(environment).AsNumber());
        }

        public override string ToString()
        {
            return "eq";
        }
    }

    public class EqualsConstant : Value
    {
        private readonly long n;

        public EqualsConstant(long n)
        {
            this.n = n;
        }

        public override Value Invoke(Node val, Dictionary<string, Node> environment)
        {
            return n == val.Evaluate(environment).AsNumber() ? (Value)Library.TrueVal : Library.FalseVal;
        }
    }

    public class LessThanClass : Value
    {
        public override Value Invoke(Node val, Dictionary<string, Node> environment)
        {
            return new LessThanConstant(val.Evaluate(environment).AsNumber());
        }

        public override string ToString()
        {
            return "lt";
        }
    }

    public class LessThanConstant : Value
    {
        private readonly long n;

        public LessThanConstant(long n)
        {
            this.n = n;
        }

        public override Value Invoke(Node val, Dictionary<string, Node> environment)
        {
            return n < val.Evaluate(environment).AsNumber() ? (Value)Library.TrueVal : Library.FalseVal;
        }
    }

    public class SClass : Value
    {
        public override Value Invoke(Node val1, Dictionary<string, Node> environment)
        {
            return new SIntermediate1(val1);
        }

        public override string ToString()
        {
            return "s";
        }
    }

    public class SIntermediate1 : Value
    {
        private readonly Node val1;

        public SIntermediate1(Node val1)
        {
            this.val1 = val1;
        }

        public override Value Invoke(Node val2, Dictionary<string, Node> environment)
        {
            return new SIntermediate2(val1, val2);
        }
    }

    public class SIntermediate2 : Value
    {
        private readonly Node val1;
        private readonly Node val2;

        public SIntermediate2(Node val1, Node val2)
        {
            this.val1 = val1;
            this.val2 = val2;
        }

        // ap ap ap s x0 x1 x2   =   ap ap x0 x2 ap x1 x2
        public override Value Invoke(Node val3, Dictionary<string, Node> environment)
        {
            Value application1 = val1.Evaluate(environment).Invoke(val3, environment);
            return application1.Invoke(new Apply(val2, val3), environment);
        }
    }

    public class CClass : Value
    {
        public override Value Invoke(Node val1, Dictionary<string, Node> environment)
        {
            return new CIntermediate1(val1);
        }

        public override string ToString()
        {
            return "c";
        }
    }

    public class CIntermediate1 : Value
    {
        private readonly Node val1;

        public CIntermediate1(Node val1)
        {
            this.val1 = val1;
        }

        public override Value Invoke(Node val2, Dictionary<string, Node> environment)
        {
            return new CIntermediate2(val1, val2);
        }
    }

    public class CIntermediate2 : Value
    {
        private readonly Node val1;
        private readonly Node val2;

        public CIntermediate2(Node val1, Node val2)
        {
            this.val1 = val1;
            this.val2 = val2;
        }

        public override Value Invoke(Node val3, Dictionary<string, Node> environment)
        {
            Value application1 = val1.Evaluate(environment).Invoke(val3, environment);
            return application1.Invoke(val2, environment);
        }
    }

    public class BClass : Value
    {
        public override Value Invoke(Node val1, Dictionary<string, Node> environment)
        {
            return new BIntermediate1(val1);
        }

        public override string ToString()
        {
            return "b";
        }
    }

    public class BIntermediate1 : Value
    {
        private readonly Node val1;

        public BIntermediate1(Node val1)
        {
            this.val1 = val1;
        }

        public override Value Invoke(Node val2, Dictionary<string, Node> environment)
        {
            return new BIntermediate2(val1, val2);
        }
    }

    public class BIntermediate2 : Value
    {
        private readonly Node val1;
        private readonly Node val2;

        public BIntermediate2(Node val1, Node val2)
        {
            this.val1 = val1;
            this.val2 = val2;
        }

        public override Value Invoke(Node val3, Dictionary<string, Node> environment)
        {
            return val1.Evaluate(environment).Invoke(new Apply(val2, val3), environment);
        }
    }

    public class ConsClass : Value
    {
        public override Value Invoke(Node val1, Dictionary<string, Node> environment)
        {
            return new ConsIntermediate1(val1);
        }

        public override string ToString()
        {
            return "cons";
        }
    }

    public class ConsIntermediate1 : Value
    {
        private readonly Node val1;

        public ConsIntermediate1(Node val1)
        {
            this.val1 = val1;
        }

        public override Value Invoke(Node val2, Dictionary<string, Node> environment)
        {
            return new ConsIntermediate2(val1, val2);
        }
    }

    public class ConsIntermediate2 : Value
    {
        private readonly Node val1;
        private readonly Node val2;

        public ConsIntermediate2(Node val1, Node val2)
        {
            this.val1 = val1;
            this.val2 = val2;
        }

        public override Value Invoke(Node val3, Dictionary<string, Node> environment)
        {
            Value application1 = val3.Evaluate(environment).Invoke(val1, environment);
            return application1.Invoke(val2, environment);
        }

        public override string ToString()
        {
            return " (" + val1 + " , " + val2 + ") ";
        }
    }

    public class CarClass : Value
    {
        public override Value Invoke(Node val, Dictionary<string, Node> environment)
        {
            return val.Evaluate(environment).Invoke(Library.TrueVal, environment);
        }

        public override string ToString()
        {
            return "car";
        }
    }

    public class CdrClass : Value
    {
        public override Value Invoke(Node val, Dictionary<string, Node> environment)
        {
            return val.Evaluate(environment).Invoke(Library.FalseVal, environment);
        }

        public override string ToString()
        {
            return "cdr";
        }
    }
}
