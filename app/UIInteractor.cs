using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using static Core.Library;
using Core;
using static app.Interactor;

namespace app
{
    public class UIInteractor : GraphicsInterface
    {
        private readonly IProtocol protocol;
        private readonly Interactor interactor;
        private Value state;

        public UIInteractor(Interactor interactor)
        {
            this.interactor = interactor;
            protocol = new GalaxyProtocol();
            state = Nil;
        }

        public Dictionary<Point, byte> AdvanceState(Point p)
        {
            Result result = interactor.Interact(
                protocol, state, new ConsIntermediate2(new Number(p.X), new Number(p.Y)));
            state = result.NewState;
            return CreateFrame(result.MultiDrawResult);
        }

        public Dictionary<Point, byte> CreateFrame(IList<DrawFrame> frames)
        {
            byte maxBrightness = 255;
            byte minBrightness = 64;
            double scale = (maxBrightness - minBrightness) / (double) frames.Count;

            Dictionary<Point, byte> result = new Dictionary<Point, byte>();

            for (int i = 0; i < frames.Count; i++)
            {
                byte currentBrightness = (byte)(maxBrightness - scale * i);
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
