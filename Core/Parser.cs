using System;
using System.Collections.Generic;
using System.Text;
using static Core.Library;
using System.Linq;

namespace Core
{
    public class Parser
    {
        public static Dictionary<string, Value> Parse(List<string> input)
        {
            Dictionary<string, Node> symbols = new Dictionary<string, Node>();
            foreach (string line in input)
            {
                string[] tokens = line.Split(" ");
                string symbol = tokens[0];

                if (tokens[1] != "=")
                {
                    throw new Exception("Line doesn't have an equals where we'd want.");
                }

                // Recursive helper function

                for (int i = 2; i < tokens.Length; i++)
                {
                    switch(tokens[i])
                    {
                        case "ap":
                            break;
                        default:
                            throw new Exception("Got an unexpected token.");
                    }
                }
            }

            // Need to evaluate dictionary before returning.
            return null;
        }
    }
}
