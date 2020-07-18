using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace Core
{
    public interface GraphicsInterface
    {
        public Dictionary<Point, byte> AdvanceState(Point p);
    }
}
