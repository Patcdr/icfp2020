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

        public Frame(GraphicsInterface gInterface)
        {
            this.gInterface = gInterface;
            Rects = new List<Rectangle>();
            Advance(new IntPoint(0, 0));
        }

        public void Advance(IntPoint p)
        {
            points = gInterface.AdvanceState(p);
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

        private void Render()
        {
            Rects.Clear();
            foreach (var pair in points)
            {
                Rects.Add(CreateRectangle(pair.Key, pair.Value));
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
    }
}
