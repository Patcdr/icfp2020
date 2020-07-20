using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Linq;
using static Core.Library;
using Core;
using static app.Interactor;
using System.IO;
using Microsoft.VisualBasic;

namespace app
{
    public class GameStateGraphics : GraphicsInterface
    {
        private readonly DoubleRunner runner;
        public GameState GameState { get; private set; }

        public GameStateGraphics(DoubleRunner runner)
        {
            this.runner = runner;
        }

        public Dictionary<Point, byte> AdvanceState(Point p)
        {
            GameState = runner.Step();
            return new Dictionary<Point, byte>();
        }

        public Dictionary<Point, byte> StartGame()
        {
            GameState = runner.Join();
            return new Dictionary<Point, byte>();
        }

        public Dictionary<Point, byte> SetState(Value s)
        {
            throw new NotImplementedException();
        }

        public Dictionary<Point, byte> UndoState(int n = 1)
        {
            throw new NotImplementedException();
        }

        public void SaveClicks(string filename = null)
        {
            throw new NotImplementedException();
        }

        public Dictionary<Point, byte> LoadClicks(string filename = null)
        {
            throw new NotImplementedException();
        }

        public Dictionary<Point, byte> CreateFrame(IList<DrawFrame> frames)
        {
            byte maxBrightness = 128;
            byte minBrightness = 16;
            double scale = (maxBrightness - minBrightness) / (double) frames.Count;

            Dictionary<Point, byte> frame = new Dictionary<Point, byte>();

            for (int i = 0; i < frames.Count; i++)
            {
                byte currentBrightness = (byte)(maxBrightness - scale * i);
                if (i == 0)
                {
                    currentBrightness = 255;
                }

                foreach (var point in frames[i].Points)
                {
                    if (!frame.ContainsKey(point))
                    {
                        frame.Add(point, currentBrightness);
                    }
                }
            }

            return frame;
        }
    }
}
