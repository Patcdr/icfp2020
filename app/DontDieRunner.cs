﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using Core;
using static Core.Library;

namespace app
{
    public class DontDieRunner : BaseRunner
    {
        Random r = new Random();
        public DontDieRunner(Sender sender, long player = 0)
            : base(sender, player, 0, 8, 1)
        {
        }

        public override void Step()
        {
            if (IsDone) return;

            if (State.CurrentTurn < 6)
            {
                StartOrbitStrategy();
            }
            else if (!State.IsAttacker || 
                State.Ships.Where(x => x.PlayerID != State.PlayerId).Count() > 1)
            {
                AvoidDeathStrategy();
            }
            else
            {
                GetCloseAndMurderThem();
            }
        }

        private void StartOrbitStrategy()
        {
            var ship = State.GetMyFirstShip();
            var opposite = new Point(Math.Sign(ship.Position.X) * -1, Math.Sign(ship.Position.Y) * -1);
            var ninetyDegrees = new Point(opposite.Y, -opposite.X);
            Command(Thrust(State.GetMyFirstShip().ID, ninetyDegrees));
        }

        private void GetCloseAndMurderThem()
        {
            int lookaheadTurns = 32;
            Ship ship = State.GetMyFirstShip();

            // Simulate the enemy's position into the future, save all positions
            // TODO: Deal with all enemy ships.
            List<Point> enemyPositions =
                ShipPositionSimulator.FuturePositionList(
                    State.Ships.Where(x => x.PlayerID != State.PlayerId).First(),
                    lookaheadTurns,
                    new Point(0, 0));

            // Simulate our position given no thrust, and all possible thrusts.
            List<Tuple<Point, List<Point>>> allPaths = new List<Tuple<Point, List<Point>>>();
            allPaths.Add(
                new Tuple<Point, List<Point>>(
                    new Point(0, 0),
                    ShipPositionSimulator.FuturePositionList(
                        State.GetMyFirstShip(),
                        lookaheadTurns,
                        new Point(0, 0))));
            foreach (Point dir in ActionHandler.AllDirections)
            {
                allPaths.Add(
                    new Tuple<Point, List<Point>>(
                        dir,
                        ShipPositionSimulator.FuturePositionList(
                            State.GetMyFirstShip(),
                            lookaheadTurns,
                            dir)));
            }

            // Figure out when we'd die along each of the paths.
            List<Tuple<int, Point, List<Point>>> deathTurnAndPaths = new List<Tuple<int, Point, List<Point>>>();
            foreach (var path in allPaths)
            {
                int deathTurn = int.MaxValue;
                for (int i = 0; i < path.Item2.Count; i++)
                {
                    if (IsDeadLocation(path.Item2[i]))
                    {
                        deathTurn = i;
                        break;
                    }
                }

                deathTurnAndPaths.Add(new Tuple<int, Point, List<Point>>(deathTurn, path.Item1, path.Item2));
            }

            // Filter out the paths that would lead to death sooner than 15 turns.  If none exist, then try the avoid
            // death strategy.
            int deathHorizon = 15;
            List<Tuple<int, Point, List<Point>>> nonDyingPaths =
                deathTurnAndPaths.Where(x => x.Item1 >= deathHorizon).ToList();
            if (nonDyingPaths.Count == 0)
            {
                AvoidDeathStrategy();
                return;
            }

            // Foreach of our paths
            //   Score them based on which one gets closest to the enemy soonest.
            int bestScore = int.MaxValue;
            Tuple<int, Point, List<Point>> bestPlan = null;
            foreach (var deathAndPath in nonDyingPaths)
            {
                var distanceAndTurn = ClosestDistanceAndTurn(deathAndPath.Item3, enemyPositions);
                int score = distanceAndTurn.Item1 + distanceAndTurn.Item2;
                if (score < bestScore)
                {
                    bestScore = score;
                    bestPlan = deathAndPath;
                }
            }

            // Pick the thrust that's highest scored!
            List<Value> commands = new List<Value>();
            if (bestPlan.Item2 != new Point(0, 0)) { 
                commands.Add(Thrust(ship.ID, bestPlan.Item2));
            }

            if (bestPlan.Item3[0] == enemyPositions[0] &&
                State.Ships.Where(x => x.PlayerID != State.PlayerId).Count() == 1)
            {
                commands.Add(Detonate(ship.ID));
            }

            if (commands.Count > 0)
            {
                Command(commands.ToArray());
            }
            else
            {
                Command();
            }
        }

        private Tuple<int, int> ClosestDistanceAndTurn(List<Point> path1, List<Point> path2)
        {
            if (path1.Count != path2.Count)
            {
                throw new Exception("I didn't write this to allow for different length paths.");
            }

            int closestDistance = int.MaxValue;
            int closestTurn = -1;
            for (int i = 0; i < path1.Count; i++)
            {
                int distance = ManhattanDistance(path1[i], path2[i]);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestTurn = i;
                }
            }

            return new Tuple<int, int>(closestDistance, closestTurn);
        }

        private int ManhattanDistance(Point first, Point second)
        {
            return Math.Abs(first.X - second.X) + Math.Abs(first.Y - second.Y);
        }

        private void AvoidDeathStrategy()
        {
            int lookaheadTurns = 15;
            Ship ship = State.GetMyFirstShip();
            bool thrusted = false;

            // If we would die by not thrusting
            if (TurnsTilDeath(lookaheadTurns, new Point(0, 0)) != int.MaxValue)
            {
                int longestLife = 0;
                Point thrust = new Point(0, 0);
                foreach (Point p in ActionHandler.AllDirections)
                {
                    int currentLife = TurnsTilDeath(lookaheadTurns, p);
                    if (currentLife > longestLife)
                    {
                        longestLife = currentLife;
                        thrust = p;
                    }
                }

                if (thrust != new Point(0, 0))
                {
                    Command(Thrust(ship.ID, thrust));
                    thrusted = true;
                }
            }

            if (!thrusted)
            {
                // Note: this means I think I've got no way to live longer.
                Command();
            }
        }

        private int TurnsTilDeath(int lookAheadTurns, Point thrust)
        {
            for (int i = 1; i <= lookAheadTurns; i++)
            {
                Point futureLocation = ShipPositionSimulator.FuturePosition(State.GetMyFirstShip(), i, thrust);
                if (IsDeadLocation(futureLocation))
                {
                    return i;
                }
            }

            return int.MaxValue;
        }

        private bool IsDeadLocation(Point location)
        {
            return (Math.Abs(location.X) <= State.StarSize && Math.Abs(location.Y) <= State.StarSize) ||
                Math.Abs(location.X) >= State.ArenaSize ||
                Math.Abs(location.Y) >= State.ArenaSize;

        }

        private void HoverOnStartStrategy()
        {
            Ship ship = State.GetMyFirstShip();
            Point thrust = new Point(0, 0);

            // Am I on a diagonal?
            if (ship.Position.X == ship.Position.Y)
            {
                int xDir = ship.Position.X > 0 ? -1 : 1;
                int yDir = ship.Position.Y > 0 ? -1 : 1;
                thrust = new Point(xDir, yDir);
            }
            else
            {
                // Not on a diagonal - fire in the direction where I'm furthest from the planet.
                if (Math.Abs(ship.Position.X) < Math.Abs(ship.Position.Y))
                {
                    int yDir = ship.Position.Y > 0 ? -1 : 1;
                    thrust = new Point(0, yDir);
                }
                else
                {
                    int xDir = ship.Position.X > 0 ? -1 : 1;
                    thrust = new Point(xDir, 0);
                }
            }

            Command(Thrust(ship.ID, thrust));
        }
    }
}
