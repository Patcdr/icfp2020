using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Linq;

namespace Core
{

    public class Ship
    {

        public readonly Value Raw;
        public readonly long PlayerID;
        public readonly long ID;
        public readonly long Role;

        public readonly Point Position;
        public readonly Point Velocity;
        public readonly Point Thrust;

        public readonly Value X4; // Appears to be the initial point allocation
        public readonly Value X5;
        public readonly Value X6;
        public readonly Value X7;

        public readonly long Health;
        public readonly long Lazers;
        public readonly long Cooling;
        public readonly long Babies;
        public readonly long Heat;

        private static Point ToPoint(Value point)
        {
            return new Point((int)point.Car().AsNumber(), (int)point.Cdr().AsNumber());
        }

        public Ship(Value ship)
        {
            this.Raw = ship;
            this.PlayerID = UtilityFunctions.Addr("caar", ship).AsNumber();
            this.ID = UtilityFunctions.Addr("cadar", ship).AsNumber();
            this.Role = UtilityFunctions.Addr("cadar", ship).AsNumber();
            this.Position = ToPoint(UtilityFunctions.Addr("caddar", ship));
            this.Velocity = ToPoint(UtilityFunctions.Addr("cadddar", ship));
            this.X4 = UtilityFunctions.Addr("caddddar", ship);
            this.X5 = UtilityFunctions.Addr("cadddddar", ship);
            this.X6 = UtilityFunctions.Addr("caddddddar", ship);
            this.X7 = UtilityFunctions.Addr("cadddddddar", ship);
            var props = UtilityFunctions.Addr("caddddar", ship);

            this.Health = UtilityFunctions.Addr("car", props).AsNumber();
            this.Lazers = UtilityFunctions.Addr("cdar", props).AsNumber();
            this.Cooling = UtilityFunctions.Addr("cddar", props).AsNumber();
            this.Babies = UtilityFunctions.Addr("cdddar", props).AsNumber();

            this.Heat = UtilityFunctions.Addr("cadddddar", ship).AsNumber();
            var commands = UtilityFunctions.Addr("cdar", ship);
            foreach (var command in UtilityFunctions.ListAsEnumerable(commands, null))
            {
                if (command.Car().AsNumber() == 0 /* THRUST */)
                {
                    Thrust = new Point(
                        (int)command.Cdr().Car().Car().AsNumber(),
                        (int)command.Cdr().Car().Cdr().AsNumber()
                    );
                }
            }
        }

        public override string ToString()
        {
            return $"Ship[ PlayerId={PlayerID}, ID={ID}, Position={Position}, Velocity={Velocity}, Health={Health}, Lazers={Lazers}, Cooling={Cooling}, Babies={Babies} ]";
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
        public readonly long TotalPoints;

        public Value server_state;
        public readonly GameState Prev;

        public GameState(Value server_state, GameState prev) : this(server_state)
        {
            Prev = prev;
        }

        public GameState(Value server_state)
        {
            this.server_state = server_state;
            this.GameStateVal = UtilityFunctions.Addr("cdar", server_state).AsNumber();
            this.TotalTurns = UtilityFunctions.Addr("cddaar", server_state).AsNumber();
            this.PlayerId = UtilityFunctions.Addr("cddadar", server_state).AsNumber();
            this.TotalPoints = UtilityFunctions.Addr("cddaddaar", server_state).AsNumber();

            if (GameStateVal >= 1)
            {
                this.CurrentTurn = UtilityFunctions.Addr("cdddaar", server_state).AsNumber();
                var cons = UtilityFunctions.Addr("cddadddar", server_state);
                if (cons != Library.Nil)
                {
                    this.StarSize = UtilityFunctions.Addr("car", cons).AsNumber();
                    this.ArenaSize = UtilityFunctions.Addr("cdar", cons).AsNumber();
                }

                this.Ships = new List<Ship>();

                Value ships = UtilityFunctions.Addr("cdddaddar", server_state);
                foreach (Value ship in UtilityFunctions.ListAsEnumerable(ships, null))
                {
                    this.Ships.Add(new Ship(ship));
                }
            }
        }

        public bool IsAttacker => PlayerId == 0;

        public Ship GetShipById(long shipId)
        {
            return Ships.Where(x => x.ID == shipId).First();
        }

        public Ship GetMyFirstShip()
        {
            return Ships.Where(x => x.PlayerID == PlayerId).First();
        }

        public Ship GetOpponentFirstShip()
        {
            return Ships.Where(x => x.PlayerID != PlayerId).First();
        }

        public override string ToString()
        {
            string str = $"GameState [ PlayerId={PlayerId}, TotalTurns={TotalTurns}, CurrentTurn={CurrentTurn}, GameStateVal={GameStateVal}, ArenaSize={ArenaSize}, StarSize={StarSize}, TotalPoints={TotalPoints} ]";

            if (Ships != null)
            {
                foreach (Ship ship in Ships)
                {
                    str += $"\n{ship}";
                }
            }

            return str;
        }
    }
}
