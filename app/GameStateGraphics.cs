using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Linq;
using static Core.Library;
using Core;
using static app.Interactor;
using System.IO;

namespace app
{
    public class GameStateGraphics : GraphicsInterface
    {
        private readonly DoubleRunner runner;
        public GameState GameState { get; private set; }

        public GameStateGraphics(DoubleRunner runner)
        {
            this.runner = runner;
        }

        public Dictionary<Point, byte> AdvanceState(Point p)
        {
            GameState = runner.Step();
            return GameStateToFrame(GameState);
        }

        public Dictionary<Point, byte> StartGame()
        {
            GameState = runner.Join();
            return GameStateToFrame(GameState);
        }

        public Dictionary<Point, byte> SetState(Value s)
        {
            throw new NotImplementedException();
        }

        public Dictionary<Point, byte> UndoState(int n = 1)
        {
            throw new NotImplementedException();
        }

        public void SaveClicks(string filename = null)
        {
            throw new NotImplementedException();
        }

        public Dictionary<Point, byte> LoadClicks(string filename = null)
        {
            throw new NotImplementedException();
        }

        public Dictionary<Point, byte> CreateFrame(IList<DrawFrame> frames)
        {
            byte maxBrightness = 128;
            byte minBrightness = 16;
            double scale = (maxBrightness - minBrightness) / (double) frames.Count;

            Dictionary<Point, byte> frame = new Dictionary<Point, byte>();

            for (int i = 0; i < frames.Count; i++)
            {
                byte currentBrightness = (byte)(maxBrightness - scale * i);
                if (i == 0)
                {
                    currentBrightness = 255;
                }

                foreach (var point in frames[i].Points)
                {
                    if (!frame.ContainsKey(point))
                    {
                        frame.Add(point, currentBrightness);
                    }
                }
            }

            return frame;
        }

        private Dictionary<Point, byte> GameStateToFrame(GameState gameState)
        {
            return CreateFrame(GameStateToDrawFrames(gameState));
        }

        private static IList<DrawFrame> GameStateToDrawFrames(GameState gameState)
        {
            var foregroundPoints = new List<Point>();
            var backgroundPoints = new List<Point>();

            // Draw ships
            if (gameState.Ships != null)
            {
                foreach (var ship in gameState.Ships)
                {
                    Point p = ship.Position;

                    // Draw as a small X
                    foregroundPoints.Add(p);
                    foregroundPoints.Add(new Point(p.X + 1, p.Y + 1));
                    foregroundPoints.Add(new Point(p.X - 1, p.Y + 1));
                    foregroundPoints.Add(new Point(p.X + 1, p.Y - 1));
                    foregroundPoints.Add(new Point(p.X - 1, p.Y - 1));
                }
            }

            // Draw star
            int halfStarSize = (int)gameState.StarSize / 2;
            for (int x = -halfStarSize; x < halfStarSize; x++)
            {
                for (int y = -halfStarSize; y < halfStarSize; y++)
                {
                    backgroundPoints.Add(new Point(x, y));
                }
            }

            // Draw border around arena
            int halfArenaSize = (int)gameState.ArenaSize / 2;
            for (int i = -halfArenaSize; i < halfArenaSize; i++)
            {
                backgroundPoints.Add(new Point(i, -halfArenaSize));
                backgroundPoints.Add(new Point(i, halfArenaSize));
                backgroundPoints.Add(new Point(-halfArenaSize, i));
                backgroundPoints.Add(new Point(halfArenaSize, i));
            }

            return new List<DrawFrame> { new DrawFrame(foregroundPoints), new DrawFrame(backgroundPoints) };
        }
    }
}
