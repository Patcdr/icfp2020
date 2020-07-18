using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Linq;
using static Core.Library;
using Core;
using static app.Interactor;
using System.IO;

namespace app
{
    public class UIInteractor : GraphicsInterface
    {
        private readonly IProtocol protocol;
        private readonly Interactor interactor;
        private Value state;
        private readonly Stack<(Value state, Point p)> history = new Stack<(Value, Point)>();

        public UIInteractor(Interactor interactor)
        {
            this.interactor = interactor;
            protocol = new GalaxyProtocol();
            state = Nil;
        }

        public Dictionary<Point, byte> AdvanceState(Point p)
        {
            history.Push((state, p));
            Result result = interactor.Interact(
                protocol, state, new ConsIntermediate2(new Number(p.X), new Number(p.Y)));
            state = result.NewState;
            return CreateFrame(result.MultiDrawResult);
        }

        public Dictionary<Point, byte> UndoState(int n = 1)
        {
            while (n > 0 && history.Count > 0)
            {
                history.Pop();
                n--;
            }

            var result = history.Pop();
            state = result.state;
            return AdvanceState(result.p);
        }

        public void SaveClicks()
        {
            string str = string.Join(Environment.NewLine, history.Select(x => $"{x.p.X},{x.p.Y}").Reverse().Skip(1));
            File.WriteAllText("SavedClicks", str);
        }

        public Dictionary<Point, byte> LoadClicks()
        {
            string[] lines = File.ReadAllText("SavedClicks").Split(Environment.NewLine);
            Dictionary<Point, byte> result = null;

            foreach (var line in lines)
            {
                string[] pts = line.Split(',');
                result = AdvanceState(new Point(int.Parse(pts[0]), int.Parse(pts[1])));
            }

            return result;
        }

        public Dictionary<Point, byte> CreateFrame(IList<DrawFrame> frames)
        {
            byte maxBrightness = 128;
            byte minBrightness = 16;
            double scale = (maxBrightness - minBrightness) / (double) frames.Count;

            Dictionary<Point, byte> result = new Dictionary<Point, byte>();

            for (int i = 0; i < frames.Count; i++)
            {
                byte currentBrightness = (byte)(maxBrightness - scale * i);
                if (i == 0)
                {
                    currentBrightness = 255;
                }

                foreach (var point in frames[i].Points)
                {
                    if (!result.ContainsKey(point))
                    {
                        result.Add(point, currentBrightness);
                    }
                }
            }

            return result;
        }
    }
}
