using Core;
using System;
using System.Collections.Generic;
using System.Text;

namespace app
{
    public abstract class BaseInteractStrategy
    {
        protected Interactor Interactor { get; }
        protected readonly int PlayerKey;

        public BaseInteractStrategy(Interactor interactor, int playerKey)
        {
            Interactor = interactor;
            PlayerKey = playerKey;

        }

        public abstract void Execute();
    }
}
