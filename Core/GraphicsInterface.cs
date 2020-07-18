using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace Core
{
    public interface GraphicsInterface
    {
        public Dictionary<Point, byte> AdvanceState(Point p);
        public Dictionary<Point, byte> UndoState(int n = 1);
        //public void SaveStates();
        //public void LoadStates();
    }
}
