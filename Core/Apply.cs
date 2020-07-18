using System;
using System.Collections.Generic;
using System.Text;

namespace Core
{
    public class Apply : Node
    {
        private readonly Node node1;
        private readonly Node node2;
        private Value evaluateCache = null;

        public Apply(Node node1, Node node2)
        {
            this.node1 = node1;
            this.node2 = node2;
        }

        public override Value Evaluate(Dictionary<string, Node> environment)
        {
            if (evaluateCache == null)
            {
                evaluateCache = node1.Evaluate(environment).Invoke(node2, environment);
            }

            return evaluateCache;
        }

        public override string ToString()
        {
            return " (ap " + node1 + " " + node2 + ") ";
        }
    }
}
