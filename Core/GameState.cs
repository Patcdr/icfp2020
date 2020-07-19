using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace Core
{

    public class Ship
    {

        public readonly long ID;
        public readonly Point Position;
        public readonly Point Velocity;

        private static Point ToPoint(Value point)
        {
            return new Point((int)point.Car().AsNumber(), (int)point.Cdr().AsNumber());
        }

        public Ship(Value ship)
        {
            this.Position = ToPoint(UtilityFunctions.Addr("cddar", ship));
            this.Velocity = ToPoint(UtilityFunctions.Addr("cdddar", ship));

        }
    }


    public class GameState
    {
        public readonly long TotalTurns;
        public readonly long CurrentTurn;
        public readonly long GameStateVal;
        public readonly List<List<Ship>> Ships;


        public GameState(Value server_state)
        {
            this.GameStateVal = UtilityFunctions.Addr("car", server_state).AsNumber();
            this.TotalTurns = UtilityFunctions.Addr("cddaar", server_state).AsNumber();
            this.CurrentTurn = UtilityFunctions.Addr("cdddaar", server_state).AsNumber();
            this.Ships = new List<List<Ship>>();

            Value players = UtilityFunctions.Addr("cdddaddar", server_state);
            foreach (Value player in UtilityFunctions.ListAsEnumerable(players, null))
            {
                if (player == Library.Nil) break;
                List<Ship> playerShips = new List<Ship>();
                foreach (Value ship in UtilityFunctions.ListAsEnumerable(player, null))
                {
                    if (ship == Library.Nil) break;
                    playerShips.Add(new Ship(ship));
                }
                this.Ships.Add(playerShips);
            }
        }

    }
}
