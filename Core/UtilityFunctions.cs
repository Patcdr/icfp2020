using System;
using System.Collections.Generic;
using System.Text;
using static Core.Library;

namespace Core
{
    public class UtilityFunctions
    {
        public static bool ToBool(Value val)
        {
            if (val is TrueClass)
            {
                return true;
            }
            else if (val is FalseClass)
            {
                return false;
            }
            else
            {
                throw new Exception("Neither true nor false, sucka!");
            }
        }

        public static List<Tuple<long, long>> EvaluatePointList(Value initial, Dictionary<string, Node> env)
        {
            List<Tuple<long, long>> points = new List<Tuple<long, long>>();
            Value curr = initial;
            while (!UtilityFunctions.ToBool(IsNil.Invoke(curr, env)))
            {
                Value point = curr.Invoke(TrueVal, env);
                long car = point.Invoke(TrueVal, env).AsNumber();
                long cdr = point.Invoke(FalseVal, env).AsNumber();
                points.Add(new Tuple<long, long>(car, cdr));
                curr = curr.Invoke(FalseVal, env);
            }

            return points;
        }

        public static Value EvaluateFully(Value curr, Dictionary<string, Node> env)
        {
            if (curr.IsNumber() || curr == Nil)
            {
                return curr;
            }

            if (curr is ConsIntermediate2)
            {
                Value car = EvaluateFully(curr.Invoke(TrueVal, env), env);
                Value cdr = EvaluateFully(curr.Invoke(FalseVal, env), env);
                return new ConsIntermediate2(car, cdr);
            }

            throw new Exception("Not a number, nil, or Cons cell");
        }
    }
}
