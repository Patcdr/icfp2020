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
        public void SaveClicks(string fileName = null);
        public Dictionary<Point, byte> LoadClicks(string fileName = null);
        public Dictionary<Point, byte> StartGame();
    }
}
