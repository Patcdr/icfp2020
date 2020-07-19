﻿using Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static Core.Library;

namespace app
{
    class GameInteractStrategy : BaseInteractStrategy
    {
        public GameInteractStrategy(Interactor interactor, Value player) : base(interactor, player)
        {
        }

        public override void Start()
        {
            base.Start(1, 1, 1, 1);
        }

        public override void Next()
        {
            Game = Interactor.sender.Send(new Value[] {
                CMD, Player, NilList
            }, Player);
        }
    }
}
