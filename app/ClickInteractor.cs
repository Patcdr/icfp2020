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
        protected readonly IProtocol protocol;
        protected readonly Interactor interactor;
        protected Value state;

        public ClickInteractor(Interactor interactor, IProtocol protocol, Value currentState)
        {
            this.interactor = interactor;
            this.protocol = protocol;
            state = currentState;
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
