using Core;
using System;
using System.Collections.Generic;
using System.Text;

namespace app
{
    interface ShipBrain
    {
        // ship is this ship (Use this instead of state.GetMyFirstShip())
        // state is the GameState this turn
        //
        // Returns the commands you want this ship to use this turn
        //
        // If you would normally do this:
        // Command(Thrust(shipId, vec));
        //
        // You can do this instead:
        // yield return Thrust(shipId, vec)
        public IEnumerable<Value> Step(Ship ship, GameState state);
    }
}
