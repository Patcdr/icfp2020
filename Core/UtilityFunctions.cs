using System;
using System.Collections.Generic;
using System.Drawing;
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

            foreach (Value point in ListAsEnumerable(initial, env))
            {
                long car = point.Invoke(TrueVal, env).AsNumber();
                long cdr = point.Invoke(FalseVal, env).AsNumber();
                points.Add(new Tuple<long, long>(car, cdr));
            }

            return points;
        }

        public static Value EvaluateFully(Value curr, Dictionary<string, Node> env)
        {
            if (curr.IsNumber() || curr is NilClass)
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

        public static IEnumerable<Value> ListAsEnumerable(Value list, Dictionary<string, Node> env)
        {
            Value curr = list;
            while (!ToBool(IsNil.Invoke(curr, env)))
            {
                yield return curr.Invoke(TrueVal, env);
                curr = curr.Invoke(FalseVal, env);
            }
        }

        // Utility to convert from a cons list to multiple DrawFrames
        public static IList<DrawFrame> MultipleDraw(Value consList)
        {
            var result = new List<DrawFrame>();

            foreach (Value points in UtilityFunctions.ListAsEnumerable(consList, null))
            {
                result.Add(Draw(points));
            }

            return result;
        }

        // Utility to convert from a cons list to a DrawFrame
        public static DrawFrame Draw(Value consList)
        {
            var points = new List<Point>();

            foreach (Value point in UtilityFunctions.ListAsEnumerable(consList, null))
            {
                int x = (int)point.Invoke(Library.TrueVal, null).AsNumber();
                int y = (int)point.Invoke(Library.FalseVal, null).AsNumber();
                points.Add(new Point(x, y));
            }

            return new DrawFrame(points);
        }

    }
}
