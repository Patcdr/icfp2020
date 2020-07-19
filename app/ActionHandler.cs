using app;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace Core
{
    public class ActionHandler
    {
        private ClickInteractor interactor;
        public ActionHandler(ClickInteractor interactor)
        {
            // TODO: Must be constructed with the world interface.
            this.interactor = interactor;
        }

        public void Explode(int shipId)
        {
            Point shipLocation = GetShipLocation(shipId);
            interactor.Click(shipLocation);
            interactor.Click(new Point(shipLocation.X - 7, shipLocation.Y));
        }

        public void Thrust(int shipId, Point direction)
        {
            if (direction.X > 1 || direction.X < -1 || direction.Y > 1 || direction.Y < -1 ||
                (direction.X == 0 && direction.Y == 0))
            {
                throw new Exception("That's not a unit direction.");
            }

            Point shipLocation = GetShipLocation(shipId);
            Point thrustIcon = new Point(shipLocation.X, shipLocation.Y - 7);
            interactor.Click(shipLocation);
            interactor.Click(thrustIcon);
            interactor.Click(new Point(thrustIcon.X + direction.X * 5, thrustIcon.Y + direction.Y * 5));
        }

        public void Laser(int shipId, Point target)
        {
            Point shipLocation = GetShipLocation(shipId);
            interactor.Click(shipLocation);
            interactor.Click(new Point(shipLocation.X, shipLocation.Y + 7));
            interactor.Click(target);
        }

        public void Split(int shipId, int fuel, int hamburger, int cooling, int babies)
        {
            throw new NotImplementedException();
        }
        
        private Point GetShipLocation(int shipId)
        {
            return new Point(16, 0);
        }
    }
}
