using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace Core
{
    interface GraphicsInterface
    {
        public Dictionary<Point, byte> AdvanceState(Point p);
    }
}
