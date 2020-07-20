using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using Core;
using static Core.Library;

namespace app
{
    public class DontDieRunner : BaseRunner
    {
        Random r = new Random();
        public static readonly int MaxShips = 64;
        public static readonly int LookaheadTurns = 32;

        public int BabiesMade = 1;
        public long MaxBabyTurn = 0;

        public DontDieRunner(Sender sender, long player = 0)
            : base(sender, player)
        {
        }

        protected override (int lazers, int cooling, int ships) GetInitialValues(bool isAttacker)
        {
            if (isAttacker)
            {
                return (0, 8, MaxShips);
            }

            return (0, 8, MaxShips);
        }

        public override void Step() {
            foreach (Ship ship in State.GetMyShips())
            {
                ShipStep(ship);
            }
            LatentSend();
        }

        public void ShipStep(Ship ship)
        {
            if (IsDone) return;

            if (ship.Health == 0 && ship.Lazers == 0)
            {
                return;
            }

            if (State.IsAttacker)
            {
                AttackStrategy(ship);
            } else {
                DefenseStrategy(ship);
            }
        }

        private void AttackStrategy(Ship ship)
        {

            if (SevenTenSplit(ship, 3)) return;
            if (StarStrategy(ship)) return;
            if (SeekOrRun(ship, State.IsAttacker)) return;
        }

        private void DefenseStrategy(Ship ship)
        {
            if (SevenTenSplit(ship, 5)) return;
            if (StarStrategy(ship)) return;
            if (SeekOrRun(ship, State.IsAttacker)) return;
        }

        private bool StartOrbitStrategy(Ship ship)
        {
            if (State.CurrentTurn >= 6) return false;

            var opposite = new Point(Math.Sign(ship.Position.X) * -1, Math.Sign(ship.Position.Y) * -1);
            var ninetyDegrees = new Point(opposite.Y, -opposite.X);
            LatentCommand(Thrust(ship.ID, ninetyDegrees));

            return true;
        }

        // TODO: Look into bum rush bug.
        private bool SeekOrRun(Ship ship, bool towards, bool bumRush = false)
        {
            // Simulate the enemy's position into the future, save all positions
            // TODO: Deal with all enemy ships.
            Ship enemy = State.Ships.Where(x => x.PlayerID != State.PlayerId).First();
            List<Point> quantumPositions = ShipPositionSimulator.FuturePositionList(
                enemy,
                LookaheadTurns,
                enemy.Thrust);
            var thrustCount = 1;

            void rec(List<Point> moves, int max=1)
            {
                if (moves.Count == max) return;

                foreach (var thrust in ShipPositionSimulator.Thrusts)
                {
                    moves.Add(thrust);
                    List<Point> enemyPositions =
                        ShipPositionSimulator.FuturePositionList(
                            State.Ships.Where(x => x.PlayerID != State.PlayerId).First(),
                            LookaheadTurns,
                            moves);

                    if (quantumPositions == null)
                    {
                        quantumPositions = new List<Point>(enemyPositions);
                    }
                    else {
                        for (var i = 0; i < enemyPositions.Count; i++)
                        {
                            quantumPositions[i] = new Point(
                                (quantumPositions[i].X * thrustCount + enemyPositions[i].X) / (thrustCount + 1),
                                (quantumPositions[i].Y * thrustCount + enemyPositions[i].Y) / (thrustCount + 1)
                            );
                        }
                    }
                    thrustCount += 1;

                    rec(moves, max);
                    moves.Remove(thrust);
                }
            }
            rec(new List<Point>());

            return SeekPositionList(ship, quantumPositions, towards, bumRush);
        }

        private bool SeekPositionList(Ship ship, IEnumerable<Point> quantumPositions, bool towards, bool bumRush)
        {
            // Simulate our position given no thrust, and all possible thrusts.
            List<Tuple<Point, List<Point>>> allPaths = new List<Tuple<Point, List<Point>>>();
            allPaths.Add(
                new Tuple<Point, List<Point>>(
                    new Point(0, 0),
                    ShipPositionSimulator.FuturePositionList(
                        ship,
                        LookaheadTurns,
                        new Point(0, 0))));
            foreach (Point dir in ShipPositionSimulator.Thrusts)
            {
                allPaths.Add(
                    new Tuple<Point, List<Point>>(
                        dir,
                        ShipPositionSimulator.FuturePositionList(
                            ship,
                            LookaheadTurns,
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
            int DeathHorizon = bumRush ? 2 : 15;
            List<Tuple<int, Point, List<Point>>> nonDyingPaths =
                deathTurnAndPaths.Where(x => x.Item1 >= DeathHorizon).ToList();
            if (nonDyingPaths.Count == 0)
            {
                AvoidDeathStrategy(ship);
                return true;
            }

            // Foreach of our paths
            //   Score them based on which one gets closest to the enemy soonest.
            int bestScore = towards ? int.MaxValue : int.MinValue;
            Tuple<int, Point, List<Point>> bestPlan = null;
            foreach (var deathAndPath in nonDyingPaths)
            {
                var distanceAndTurn = ClosestDistanceAndTurn(deathAndPath.Item3, quantumPositions);
                int score = distanceAndTurn.Item1 + distanceAndTurn.Item2;
                if (towards && score < bestScore)
                {
                    bestScore = score;
                    bestPlan = deathAndPath;
                }
                else if (!towards && score > bestScore)
                {
                    bestScore = score;
                    bestPlan = deathAndPath;
                }
            }

            // Pick the thrust that's highest scored!
            Point expectedPosition = ship.Position;
            if (bestPlan.Item2 != new Point(0, 0) && ship.Health > 0) {
                expectedPosition = new Point(expectedPosition.X - bestPlan.Item2.X, expectedPosition.Y - bestPlan.Item2.Y);
                LatentCommand(Thrust(ship.ID, bestPlan.Item2));
            }

            if (bestPlan.Item3[0] == quantumPositions.First() &&
                State.Ships.Where(x => x.PlayerID != State.PlayerId).Count() == 1 &&
                towards)
            {
                LatentCommand(Detonate(ship.ID));
            }

            if (towards)
            {
                //ConsiderShootingLaser(ship, expectedPosition);
            }

            return true;
        }

        private void ConsiderShootingLaser(Ship ship, Point expectedPosition)
        {
            // If we're not too hot
            if (ship.Heat > 80)
            {
                return;
            }

            // And there's an enemy ship close enough
            int maxDistance = 40;
            int closestDistance = int.MaxValue;
            Point closestShip = new Point(0, 0);
            foreach (Ship s in State.Ships.Where(x => x.PlayerID != State.PlayerId))
            {
                Point enemyPosition = ShipPositionSimulator.FuturePosition(s, 1, s.Thrust);
                int distance = ManhattanDistance(enemyPosition, expectedPosition);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestShip = enemyPosition;
                }
            }

            // FIRE TEH LZER
            if (closestDistance < maxDistance)
            {
                LatentCommand(Shoot(ship.ID, closestShip, ship.Lazers));
            }
        }

        private Tuple<int, int> ClosestDistanceAndTurn(IEnumerable<Point> us, IEnumerable<Point> them)
        {
            var usList = new List<Point>(us);
            var themList = new List<Point>(them);

            int closestDistance = int.MaxValue;
            int closestTurn = -1;
            for (int i = 0; i < usList.Count(); i++)
            {
                if (IsDeadLocation(usList[i]))
                {
                    break;
                }

                int distance = ManhattanDistance(usList[i], themList[Math.Min(i, themList.Count - 1)]);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestTurn = i;
                }
            }

            return new Tuple<int, int>(closestDistance, closestTurn);
        }

        private void AvoidDeathStrategy(Ship ship)
        {
            // If we would die by not thrusting
            if (TurnsTilDeath(ship, new Point(0, 0)) != int.MaxValue)
            {
                int longestLife = 0;
                Point thrust = new Point(0, 0);
                foreach (Point p in ShipPositionSimulator.Thrusts)
                {
                    int currentLife = TurnsTilDeath(ship, p);
                    if (currentLife > longestLife)
                    {
                        longestLife = currentLife;
                        thrust = p;
                    }
                }

                if (thrust != new Point(0, 0))
                {
                    LatentCommand(Thrust(ship.ID, thrust));
                }
            }
        }

        private int TurnsTilDeath(Ship ship, Point thrust)
        {
            int a = LookaheadTurns;
            List<Point> futureLocations = ShipPositionSimulator.FuturePositionList(ship, a, thrust);
            for (int i = 0; i < futureLocations.Count; i++)
            {
                if (IsDeadLocation(futureLocations[i]))
                {
                    return i;
                }
            }

            return int.MaxValue;
        }

        private void HoverOnStartStrategy(Ship ship)
        {
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

            LatentCommand(Thrust(ship.ID, thrust));
        }


        private bool SevenTenSplit(Ship ship, int boostTurns)
        {
            // Split quick
            if (State.CurrentTurn > boostTurns) return false;

            // Split
            if (MyShips.Count() == 1)
            {
                LatentCommand(Split(ship.ID, (int)ship.Health/2, (int)ship.Lazers/2, (int)ship.Cooling/2, (int)ship.Babies/2));
                return true;
            }

            int index = MyAliveShips.ToList().IndexOf(ship);
            var sign = (ship == MyFirstShip ? 1 : - 1);
            var opposite = new Point(Math.Sign(ship.Position.X) * -1, Math.Sign(ship.Position.Y) * -1);
            var ninetyDegrees = new Point(sign * opposite.Y * 2, -sign * opposite.X * 2);
            LatentCommand(Thrust(ship.ID, ninetyDegrees));
            return true;
        }


        private bool StarStrategy(Ship ship)
        {
            // If no eggs left, return false
            if (ship.Babies == 1)
            {
                return false;
            }

            if (State.IsAttacker)
            {
                ;
            }

            // If we're not on a baby (pos and velocity) and our orbit is stable, give birth!
            bool onBaby =
                State.Ships.Any(
                    x => x.Position == ship.Position &&
                    x.Velocity == ship.Velocity &&
                    x.ID < ship.ID);
            if (!onBaby && IsStableOrbit(ship))
            {
                if (!State.IsAttacker)
                {
                    LatentCommand(Split(ship.ID, 0, 0, 0, 1));
                }
                else
                {
                    LatentCommand(Split(ship.ID, (int)ship.Health / 2, 0, 0, (int)ship.Babies / 2));
                }
                return true;
            }

            // Otherwise, get to a (different) stable orbit.
            LatentCommand(Thrust(ship.ID, GetStableOrbitThrust(ship)));
            return true;
        }

        private Point GetStableOrbitThrust(Ship ship)
        {
            Point[] thrustVectors = ShipPositionSimulator.Thrusts;
            int maxTurns = 0;
            Point bestThrust = new Point(0, 0);
            foreach (Point thrust in thrustVectors)
            {
                int ttl = TimeToLive(ship, thrust);
                if (ttl == int.MaxValue)
                {
                    return thrust;
                }
                else if (ttl > maxTurns)
                {
                    maxTurns = ttl;
                    bestThrust = thrust;
                }
            }

            return bestThrust;
        }

        private int TimeToLive(Ship ship, Point thrust)
        {
            List<Point> positions =
                ShipPositionSimulator.FuturePositionList(ship, (int)State.TotalTurns - (int)State.CurrentTurn, thrust);
            for (int i = 0; i < positions.Count; i++)
            {
                if (IsDeadLocation(positions[i]))
                {
                    return i + 1;
                }
            }

            return int.MaxValue;
        }

        private bool IsStableOrbit(Ship ship)
        {
            return TimeToLive(ship, new Point(0, 0)) == int.MaxValue;
        }

        private void BabyBumRush(Ship ship)
        {
            // Wait a turn (in some fashion) to see which way they go.
            if (State.CurrentTurn == 0)
            {
                return;
            }

            if (State.CurrentTurn < 3)
            {
                Ship enemyShip = State.GetOpponentFirstShip();
                Point predictedThrust = enemyShip.Thrust;
                List<Point> predictedThrusts = new List<Point>();
                for (int i = 0; i < 3 - State.CurrentTurn; ++i)
                {
                    predictedThrusts.Add(predictedThrust);
                }

                List<Point> predictedPath = ShipPositionSimulator.FuturePositionList(enemyShip, (int)State.TotalTurns, predictedThrusts);

                SeekPositionList(ship, predictedPath, true, true);
            }
            else
            {
                SeekOrRun(ship, true, true);
            }

            // Rush to meet them. ???
            return;
        }

        public Point GetThrustToMeetEnemy(List<Point> expectedPositions)
        {
            for (int i = 0; i < expectedPositions.Count; i++)
            {
                // Can we reach expectedPositions[i] in time?
                // BFS or A*
            }

            return Point.Empty;
        }

        public int SpaceTimeDistance(Point ourLocation, Point theirLocation, int turns)
        {
            return ManhattanDistance(ourLocation, theirLocation) + turns;
        }

        private Point BabyMama(Point origin, int index)
        {
            var r_o = Math.Sqrt(origin.X * origin.X + origin.Y * origin.Y);
            var a_o = Math.Atan((double)origin.Y / (double)origin.X);

            var r = State.ArenaSize - index / 8 * (State.ArenaSize - State.StarSize) - 5;
            var a = (index % 8 * (2 * Math.PI / 8) + a_o);

            var cart = new Point((int)(r * Math.Cos(a)), (int)(r * Math.Sin(a)));
            return cart;
        }

    }
}
