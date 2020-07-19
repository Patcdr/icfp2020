using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace Core
{

    public class Ship
    {

        public readonly long PlayerID;
        public readonly long ID;
        public readonly Point Position;
        public readonly Point Velocity;

        private static Point ToPoint(Value point)
        {
            return new Point((int)point.Car().AsNumber(), (int)point.Cdr().AsNumber());
        }

        public Ship(Value ship)
        {
            this.PlayerID = UtilityFunctions.Addr("caar", ship).AsNumber();
            this.ID = UtilityFunctions.Addr("cdar", ship).AsNumber();
            this.Position = ToPoint(UtilityFunctions.Addr("caddar", ship));
            this.Velocity = ToPoint(UtilityFunctions.Addr("cadddar", ship));

        }
    }


    public class GameState
    {
        public readonly long TotalTurns;
        public readonly long CurrentTurn;
        public readonly long GameStateVal;
        public readonly List<Ship> Ships;


        public GameState(Value server_state)
        {
            this.GameStateVal = UtilityFunctions.Addr("car", server_state).AsNumber();
            this.TotalTurns = UtilityFunctions.Addr("cddaar", server_state).AsNumber();
            this.CurrentTurn = UtilityFunctions.Addr("cdddaar", server_state).AsNumber();
            this.Ships = new List<Ship>();

            Value ships = UtilityFunctions.Addr("cdddaddar", server_state);
            foreach (Value ship in UtilityFunctions.ListAsEnumerable(ships, null))
            {
                this.Ships.Add(new Ship(ship));
            }
        }

    }
}
