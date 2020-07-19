using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Linq;

namespace Core
{

    public class Ship
    {

        public readonly long PlayerID;
        public readonly long ID;
        public readonly Point Position;
        public readonly Point Velocity;

        public readonly long Health;
        public readonly long Lazers;
        public readonly long Cooling;
        public readonly long Babies;


        private static Point ToPoint(Value point)
        {
            return new Point((int)point.Car().AsNumber(), (int)point.Cdr().AsNumber());
        }

        public Ship(Value ship)
        {
            this.PlayerID = UtilityFunctions.Addr("caar", ship).AsNumber();
            this.ID = UtilityFunctions.Addr("cadar", ship).AsNumber();
            this.Position = ToPoint(UtilityFunctions.Addr("caddar", ship));
            this.Velocity = ToPoint(UtilityFunctions.Addr("cadddar", ship));
            var props = UtilityFunctions.Addr("caddddar", ship);

            this.Health = UtilityFunctions.Addr("car", props).AsNumber();
            this.Lazers = UtilityFunctions.Addr("cdar", props).AsNumber();
            this.Cooling = UtilityFunctions.Addr("cddar", props).AsNumber();
            this.Babies = UtilityFunctions.Addr("cdddar", props).AsNumber();
        }
    }


    public class GameState
    {
        public readonly long PlayerId;
        public readonly long TotalTurns;
        public readonly long CurrentTurn;
        public readonly long GameStateVal;
        public readonly List<Ship> Ships;
        public readonly long ArenaSize;
        public readonly long StarSize;


        public GameState(Value server_state)
        {
            this.GameStateVal = UtilityFunctions.Addr("car", server_state).AsNumber();
            this.TotalTurns = UtilityFunctions.Addr("cddaar", server_state).AsNumber();
            this.PlayerId = UtilityFunctions.Addr("cddadar", server_state).AsNumber();
            this.CurrentTurn = UtilityFunctions.Addr("cdddaar", server_state).AsNumber();
            var cons = UtilityFunctions.Addr("cddadddar", server_state);
            if (cons != Library.Nil)
            {
                this.ArenaSize = UtilityFunctions.Addr("car", cons).AsNumber();
                this.StarSize = UtilityFunctions.Addr("cdar", cons).AsNumber();
            }

            this.Ships = new List<Ship>();

            Value ships = UtilityFunctions.Addr("cdddaddar", server_state);
            foreach (Value ship in UtilityFunctions.ListAsEnumerable(ships, null))
            {
                this.Ships.Add(new Ship(ship));
            }
        }

        public Ship GetShipById(long shipId)
        {
            return Ships.Where(x => x.ID == shipId).First();
        }

        public Ship GetShipByPlayerId(long playerId)
        {
            return Ships.Where(x => x.PlayerID == playerId).First();
        }
    }
}
