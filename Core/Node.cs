﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Core
{
    public abstract class Node
    {
        public virtual Value Evaluate()
        {
            throw new NotImplementedException();
        }
    }
}