using app;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;

namespace Core
{
    public class ActionHandler
    {
        #region Directions
        public static readonly Point UpLeftDirection = new Point(-1, -1);
        public static readonly Point UpDirection = new Point(0, -1);
        public static readonly Point UpRightDirection = new Point(1, -1);
        public static readonly Point RightDirection = new Point(1, 0);
        public static readonly Point DownRightDirection = new Point(1, 1);
        public static readonly Point DownDirection = new Point(0, 1);
        public static readonly Point DownLeftDirection = new Point(-1, 1);
        public static readonly Point LeftDirection = new Point(-1, 0);

        public static readonly List<Point> AllDirections = new List<Point>
        {
            UpLeftDirection,
            UpDirection,
            UpRightDirection,
            RightDirection,
            DownRightDirection,
            DownDirection,
            DownLeftDirection,
            LeftDirection
        };
        #endregion

        private ClickInteractor interactor;
        public ActionHandler(ClickInteractor interactor)
        {
            // TODO: Must be constructed with the world interface.
            this.interactor = interactor;
        }

        public void Explode(GameState currentState, long shipId)
        {
            Point shipLocation = currentState.GetShipById(shipId).Position;
            interactor.Click(shipLocation);
            interactor.Click(new Point(shipLocation.X - 7, shipLocation.Y));
        }

        public void Thrust(GameState currentState, long shipId, Point direction)
        {
            if (direction.X > 1 || direction.X < -1 || direction.Y > 1 || direction.Y < -1 ||
                (direction.X == 0 && direction.Y == 0))
            {
                throw new Exception("That's not a unit direction.");
            }

            Point shipLocation = currentState.GetShipById(shipId).Position;
            Point thrustIcon = new Point(shipLocation.X, shipLocation.Y - 7);
            interactor.Click(shipLocation);
            interactor.Click(thrustIcon);
            interactor.Click(new Point(thrustIcon.X + direction.X * 5, thrustIcon.Y + direction.Y * 5));
        }

        public void Laser(GameState currentState, long shipId, Point target)
        {
            Point shipLocation = currentState.GetShipById(shipId).Position;
            interactor.Click(shipLocation);
            interactor.Click(new Point(shipLocation.X, shipLocation.Y + 7));
            interactor.Click(target);
        }

        public void Split(long shipId, int fuel, int hamburger, int cooling, int babies)
        {
            throw new NotImplementedException();
        }

        public Value GetCurrentState()
        {
            return interactor.state;
        }

        public void SetCurrentState(Value state)
        {
            interactor.state = state;
        }
    }
}
