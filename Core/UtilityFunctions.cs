using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
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

        public static Value MakeList(IEnumerable<Value> items)
        {
            return MakeList(items.ToArray());
        }

        public static Value MakeList(params Value[] items)
        {
            Value list = NilList;
            for (var i = items.Length - 1; i >= 0; i--)
            {
                list = new ConsIntermediate2(items[i], list);
            }
            return list;
        }

        public static Value MakeList(int[] items)
        {
            var list = new Value[items.Length];
            for(var i = 0; i < list.Length; i++)
            {
                list[i] = new Number(items[i]);
            }
            return MakeList(list);
        }

        // Can be used to caddr
        public static Value Addr(string address, Value consPile, Dictionary<string, Node> env = null)
        {
            Value curr = consPile;
            foreach (char c in address)
            {
                switch (c)
                {
                    case 'c':
                    case 'r':
                        break;
                    case 'a':
                        curr = curr.Invoke(TrueVal, env);
                        break;
                    case 'd':
                        curr = curr.Invoke(FalseVal, env);
                        break;
                    default:
                        throw new Exception("Too lazy to be witty");
                }
            }
            return curr;
        }

        public static Value Replace(string address, Value consPile, Value value, Dictionary<string, Node> env = null)
        {
            if (address.Length == 0)
            {
                return value;
            }

            char c = address[0];
            switch (c)
            {
                case 'c':
                case 'r':
                    return Replace(address.Substring(1), consPile, value, env);
                case 'a':
                    return new ConsIntermediate2(
                        Replace(address.Substring(1), consPile.Invoke(TrueVal, env), value, env),
                        consPile.Invoke(FalseVal, env)
                    );
                case 'd':
                    return new ConsIntermediate2(
                        consPile.Invoke(TrueVal, env),
                        Replace(address.Substring(1), consPile.Invoke(FalseVal, env), value, env)
                    );
                default:
                    throw new Exception("Too lazy to be witty");
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
        public static string PrettyPrint(Value list, bool compact=false)
        {
            Tuple<string, object> rec(Value thing, string prefix)
            {
                if (thing is ConsIntermediate2)
                {
                    var car = thing.Invoke(TrueVal, null);
                    var cdr = thing.Invoke(FalseVal, null);
                    if (car is Number && cdr is Number)
                    {
                        return new Tuple<string, object>(prefix + "r", new Tuple<long, long>(car.AsNumber(), cdr.AsNumber()));
                    }

                    var iter_prefix = prefix;
                    List<Tuple<string, object>> ret = new List<Tuple<string, object>>();
                    foreach (Value val in UtilityFunctions.ListAsEnumerable(thing, null))
                    {
                        ret.Add(rec(val, iter_prefix + "a"));
                        iter_prefix += "d";
                    }
                    return new Tuple<string, object>(prefix + "r", ret);
                }
                else if (thing is Number)
                {
                    return new Tuple<string, object>(prefix + "r", thing.AsNumber());
                }
                else if (thing == Nil)
                {
                    return new Tuple<string, object>(prefix + "r", null);
                }
                throw new Exception("The hell is this?");
            }

            string rec2(Tuple<string, object> tup, int level)
            {
                var x = tup.Item2;
                var prefix = tup.Item1;
                string indent = "".PadLeft(level * 4, ' ');
                if (x is long)
                {
                    return (compact ? "" : indent) + x.ToString() + (compact ? "," : " #" + prefix) + "\n";
                }
                else if (x is Tuple<long, long>)
                {
                    var y = x as Tuple<long, long>;
                    return (compact ? "" : indent) + $"({y.Item1}, {y.Item2})" +(compact ? "," : " #" + prefix) + "\n";
                }
                else if (x is List<Tuple<string, object>>)
                {
                    string acccc = (compact ? "" : indent) + "[" + (compact ? "" : " #" + prefix) + "\n"; ;
                    foreach (Tuple<string, object> i in (List<Tuple<string, object>>)x)
                    {
                        acccc += rec2(i, level + 1);
                    }
                    return acccc + (compact ? "" : indent) + "],\n";
                }
                else if (x == null)
                {
                    return (compact ? "" : indent + "Nil" + " #" + prefix) + "\n";
                }
                throw new Exception("The hell is this?");
            }

            var x = rec(list, "c");
            var s = rec2(x, 0);
            if (!compact) Console.WriteLine(s);
            return s;
        }
    }
}
