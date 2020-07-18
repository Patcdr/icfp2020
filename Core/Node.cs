using System;
using System.Collections.Generic;
using System.Text;

namespace Core
{
    public abstract class Node
    {
        public virtual Value Evaluate(Dictionary<string, Node> environment)
        {
            throw new NotImplementedException();
        }
    }

    public class SymbolNode : Node
    {
        private string symbol;
        private Value evaluateCache = null;

        public SymbolNode(string symbol)
        {
            this.symbol = symbol;
        }

        public override Value Evaluate(Dictionary<string, Node> environment)
        {
            if (evaluateCache == null)
            {
                evaluateCache = environment[symbol].Evaluate(environment);
            }

            return evaluateCache;
        }

        public override string ToString()
        {
            return symbol;
        }
    }
}
