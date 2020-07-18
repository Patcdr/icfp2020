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
        private Result result;
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
            result = interactor.Interact(
                protocol, state, new ConsIntermediate2(new Number(p.X), new Number(p.Y)));
            state = result.NewState;

            return CreateFrame(result.MultiDrawResult);
        }

        public Dictionary<Point, byte> StartGame()
        {
            return SetPage(5, 9, 36, 0);
        }
        public Dictionary<Point, byte> SetPage(int page, int subpage, int x, int y)
        {
            state = new ConsIntermediate2(
                new Number(page),
                new ConsIntermediate2(
                    state.Cdr().Car(),
                    new ConsIntermediate2(
                        new Number(subpage),
                        state.Cdr().Cdr().Cdr()
                    )
                )
            );
            return AdvanceState(new Point(x, y));
        }

        public Dictionary<Point, byte> UndoState(int n = 1)
        {
            while (n > 0 && history.Count > 0)
            {
                history.Pop();
                n--;
            }

            var last = history.Pop();
            state = last.state;
            return AdvanceState(last.p);
        }

        public void SaveClicks(string filename = null)
        {
            string str = string.Join(Environment.NewLine, history.Select(x => $"{x.p.X},{x.p.Y}").Reverse().Skip(1));
            File.WriteAllText(filename ?? "SavedClicks.saves", str);
        }

        public Dictionary<Point, byte> LoadClicks(string filename = null)
        {
            string[] lines = File.ReadAllText(filename ?? "SavedClicks.saves").Split(Environment.NewLine);
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
