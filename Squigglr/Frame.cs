using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using Core;
using IntPoint = System.Drawing.Point;

namespace Squigglr
{
    public class Frame
    {
        public Dictionary<IntPoint, byte> points;
        private GraphicsInterface gInterface;

        public List<Rectangle> Rects { get; private set; }
        public Dictionary<IntPoint, Label> NumberOverlays { get; }

        public Frame(GraphicsInterface gInterface)
        {
            this.gInterface = gInterface;
            Rects = new List<Rectangle>();
            NumberOverlays = new Dictionary<IntPoint, Label>();
            Advance(new IntPoint(0, 0));
        }

        public void Advance(IntPoint p)
        {
            points = gInterface.AdvanceState(p);
            Render();
        }

        public void Show(IList<DrawFrame> frames)
        {
            points = gInterface.CreateFrame(frames);
            Render();
        }

        public void AdvanceMany(List<IntPoint> pList)
        {
            foreach (var p in pList)
            {
                points = gInterface.AdvanceState(p);
            }
            Render();
        }

        public void Undo()
        {
            points = gInterface.UndoState();
            Render();
        }

        public void Save(string filename) => gInterface.SaveClicks(filename);

        public void Load(string filename)
        {
            points = gInterface.LoadClicks(filename);
            Render();
        }

        public void StartGame()
        {
            points = gInterface.StartGame();
            Render();
        }

        public void SetState(Value s)
        {
            points = gInterface.SetState(s);
            Render();
        }

        private void Render()
        {
            Rects.Clear();
            foreach (var pair in points)
            {
                Rects.Add(CreateRectangle(pair.Key, pair.Value));
            }

            NumberOverlays.Clear();
            // Number extraction/overlay. This should *probably* be in the frame instead of here.
            var whitePixels = points.Where(x => x.Value == 255).Select(x => x.Key);
            List<NumberLocation> numberLocations = GlyphParsing.ExtractNumbers(whitePixels);
            foreach (var location in numberLocations)
            {
                var numberLabel = new Label();
                numberLabel.Foreground = new SolidColorBrush(Colors.Red); // Yeah, this should be in a static. There aren't too many of these.
                numberLabel.Content = location.num;
                numberLabel.Visibility = Visibility.Visible;
                numberLabel.FontSize = 20;
                Point loc = Scaler.Convert(location.p);
                Canvas.SetLeft(numberLabel, loc.X);
                Canvas.SetTop(numberLabel, loc.Y);
                NumberOverlays.Add(location.p, numberLabel);
            }
        }

        public void Update()
        {
            var pointKeys = points.Keys.ToArray();

            for (int i = 0; i < Rects.Count; i++)
            {
                var rect = Rects[i];
                var p = pointKeys[i];

                UpdateRectangle(rect, p);
            }

            foreach (var kvp in NumberOverlays)
            {
                UpdateNumberOverlay(kvp.Value, kvp.Key);
            }
        }

        private Rectangle CreateRectangle(IntPoint p, byte color)
        {
            var c = Color.FromRgb(color, color, color);

            var r = new Rectangle();

            Scaler.ResizeRectangle(r);
            r.Fill = new SolidColorBrush(c);

            Point drawingPoint = Scaler.Convert(p);
            Canvas.SetLeft(r, drawingPoint.X);
            Canvas.SetTop(r, drawingPoint.Y);

            return r;
        }

        private void UpdateRectangle(Rectangle rect, IntPoint p)
        {
            Scaler.ResizeRectangle(rect);
            Point drawingPoint = Scaler.Convert(p);
            Canvas.SetLeft(rect, drawingPoint.X);
            Canvas.SetTop(rect, drawingPoint.Y);
        }

        private void UpdateNumberOverlay(Label numberLabel, IntPoint p)
        {
            Point drawingPoint = Scaler.Convert(p);
            Canvas.SetLeft(numberLabel, drawingPoint.X);
            Canvas.SetTop(numberLabel, drawingPoint.Y);
        }
    }
}
