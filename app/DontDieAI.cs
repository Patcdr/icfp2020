using Core;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace app
{
    public class DontDieAI : BaseInteractStrategy
    {
        private ActionHandler handler;
        private List<Tuple<double, double>> unitVectors;

        public DontDieAI(Interactor interactor) : base(interactor)
        {
            handler = new ActionHandler(new ClickInteractor(interactor, Protocol, Local));
            unitVectors = ActionHandler.AllDirections.Select(x => ScaleToUnitLength(x)).ToList();
        }

        public override Value Next(GameState state)
        {
            Ship myShip = state.GetShipByPlayerId(1);
            handler.Thrust(state, myShip.ID, GetThrustDirection(myShip.Position));

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
                    + unitVectors[i].Item2 + currentPosition.Y;
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
