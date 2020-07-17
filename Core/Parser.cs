using System;
using System.Collections.Generic;
using static Core.Library;
using System.Linq;

namespace Core
{
    public class Parser
    {
        public static Dictionary<string, Node> Parse(List<string> input)
        {
            Dictionary<string, Node> symbols = new Dictionary<string, Node>();
            foreach (string line in input)
            {
                // Is this how we're supposed to do this?  Cast it? :(
                IEnumerator<string> tokens = line.Split(" ").AsEnumerable().GetEnumerator();
                tokens.MoveNext();
                string symbol = tokens.Current;
                tokens.MoveNext();

                if (tokens.Current != "=")
                {
                    throw new Exception("Line doesn't have an equals where we'd want.");
                }
                tokens.MoveNext();

                symbols.Add(symbol, ParseTree(tokens));
            }

            return symbols;
        }

        private static Node ParseTree(IEnumerator<string> tokens)
        {
            string currentToken = tokens.Current;
            tokens.MoveNext();
            switch (currentToken)
            {
                case "ap":
                    return new Apply(ParseTree(tokens), ParseTree(tokens));
                case "add":
                    return Add;
                case "b":
                    return BCombo;
                case "c":
                    return CCombo;
                case "car":
                    return Car;
                case "cdr":
                    return Cdr;
                case "cons":
                    return Cons;
                case "div":
                    return Divide;
                case "eq":
                    return EqualVal;
                case "i":
                    return Identity;
                case "isnil":
                    return IsNil;
                case "lt":
                    return LessThan;
                case "mul":
                    return Mult;
                case "neg":
                    return Negate;
                case "nil":
                    return Nil;
                case "s":
                    return SCombo;
                case "t":
                    return TrueVal;
                case "f":
                    return FalseVal;
                case "inc":
                    return Increment;
                case "dec":
                    return Decrement;
                case "pwr2":
                    return Power2;
                case "if0":
                    return IfZero;
                case "vec":
                    return Vec;
                default:
                    if (currentToken.StartsWith(":") || currentToken == "galaxy")
                    {
                        return new SymbolNode(currentToken);
                    }

                    if (long.TryParse(currentToken, out long number))
                    {
                        return new Number(number);
                    }

                    throw new Exception("Got a token we don't recognize.");
            }
        }
    }
}
