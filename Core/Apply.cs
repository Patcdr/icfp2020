﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Core
{
    public class Apply : Node
    {
        private readonly Node node1;
        private readonly Node node2;

        public Apply(Node node1, Node node2)
        {
            this.node1 = node1;
            this.node2 = node2;
        }

        public override Value Evaluate()
        {
            return node1.Evaluate().Invoke(node2.Evaluate());
        }
    }
}