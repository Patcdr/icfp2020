using Core;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace app
{
    public class DontDieAI : GameInteractStrategy
    {
        private ActionHandler handler;
        private List<Tuple<double, double>> unitVectors;

        public DontDieAI(Sender sender, Value player, int playerId) : base(sender, player, playerId)
        {
            handler = new ActionHandler(new ClickInteractor(Interactor, Protocol, Local));
            unitVectors = ActionHandler.AllDirections.Select(x => ScaleToUnitLength(x)).ToList();
        }

        public override Value Start()
        {
            // TODO: Be smarter about choosing values
            return Start(446, 0, 0, 1);
        }

        public override Value Next(GameState state)
        {
            handler.SetCurrentState(Local);
            Ship myShip = state.GetShipByPlayerId(PlayerId);
            if (myShip.Health > 1)
            {
                handler.Thrust(state, myShip.ID, GetThrustDirection(myShip.Position));
            }

            Local = handler.GetCurrentState();
            var result = Interact(0, 0);
            return result;
        }

        private Point GetThrustDirection(Point currentPosition)
        {
            Point bestDirection = new Point(0, 0);
            double bestProduct = 0;
            for (int i = 0; i < ActionHandler.AllDirections.Count; i++)
            {
                double product = unitVectors[i].Item1 * currentPosition.X
                    + unitVectors[i].Item2 * currentPosition.Y;
                if (product > bestProduct)
                {
                    bestDirection = ActionHandler.AllDirections[i];
                    bestProduct = product;
                }
            }

            return new Point(-1 * bestDirection.X, -1 * bestDirection.Y);
        }

        private Tuple<double, double> ScaleToUnitLength(Point p)
        {
            double length = GetLength(p);
            return new Tuple<double, double>(p.X / length, p.Y / length);
        }

        private double GetLength(Point p)
        {
            return Math.Sqrt(p.X * p.X + p.Y * p.Y);
        }
    }
}
