using Core;
using System;
using System.Collections.Generic;
using System.Text;

namespace app
{
    public abstract class BaseInteractStrategy
    {
        protected Interactor Interactor { get; }

        public BaseInteractStrategy(Interactor interactor)
        {
            Interactor = interactor;
        }

        public abstract void Execute();

        public abstract void Next();
    }
}
