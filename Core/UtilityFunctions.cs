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

        public static List<Tuple<long, long>> EvaluatePointList(Value initial, Dictionary<string, Node> environment)
        {
            List<Tuple<long, long>> points = new List<Tuple<long, long>>();
            Value curr = initial;
            while (!UtilityFunctions.ToBool(IsNil.Invoke(curr, environment)))
            {
                Value point = curr.Invoke(TrueVal, environment);
                long car = point.Invoke(TrueVal, environment).AsNumber();
                long cdr = point.Invoke(FalseVal, environment).AsNumber();
                points.Add(new Tuple<long, long>(car, cdr));
                curr = curr.Invoke(FalseVal, environment);
            }

            return points;
        }
    }
}
