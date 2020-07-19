using Core;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using static Core.Library;

namespace app
{
    public class ClickInteractor
    {
        private readonly IProtocol protocol;
        private readonly Interactor interactor;
        private Value state;

        public ClickInteractor(Interactor interactor, IProtocol protocol, Value startingState)
        {
            this.interactor = interactor;
            this.protocol = protocol;
            state = startingState;
        }

        public IList<DrawFrame> Click(Point p)
        {
            var result = interactor.Interact(
                protocol, state, new ConsIntermediate2(new Number(p.X), new Number(p.Y)));
            state = result.NewState;
            return result.MultiDrawResult;
        }
    }
}
