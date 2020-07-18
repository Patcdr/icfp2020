using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using app;
using Core;
using IntPoint = System.Drawing.Point;

namespace Squigglr
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public double SCALE = 10;
        
        public class TempGraphics : GraphicsInterface
        {
            private Dictionary<IntPoint, byte> frame { get; set; } = new Dictionary<IntPoint, byte>();

            public Dictionary<IntPoint, byte> AdvanceState(IntPoint p)
            {
                if (frame.ContainsKey(p)) frame.Remove(p);
                else frame.Add(p, 255);

                return frame;
            }
        }

        GraphicsInterface gInterface;
        private double RealWidth;
        private double RealHeight;
        private int PanShiftWidth = 0;
        private int PanShiftHeight = 0;
        private Dictionary<IntPoint, byte> currentFrame;

        private Rectangle MouseHover;

        public MainWindow()
        {
            InitializeComponent();
            canvas.Background = new SolidColorBrush(Colors.Black);
            RealWidth = window.Width;
            RealHeight = window.Height;

            MouseHover = CreateRectangle(new IntPoint(0, 0), 255);
            MouseHover.Fill = new SolidColorBrush(Colors.Green);

            // Default to the test server
            string serverUrl = "https://icfpc2020-api.testkontur.ru";
            string playerKey = "463bf8217ff3469189e1d9d15f8a29ce";

            Sender sender = new Sender(serverUrl, playerKey);
            Interactor interactor = new Interactor(sender);
            gInterface = new UIInteractor(interactor);
            RenderFrame(GetFrame(new IntPoint(0, 0)));
        }

        public Dictionary<IntPoint, byte> GetFrame(IntPoint p)
        {
            currentFrame = gInterface.AdvanceState(p);
            return currentFrame;
        }

        public void RenderFrame(Dictionary<IntPoint, byte> frame)
        {
            canvas.Children.Clear();
            
            foreach (var pair in frame)
            {
                canvas.Children.Add(CreateRectangle(pair.Key, pair.Value));
            }

            canvas.Children.Add(MouseHover);
        }

        public Rectangle CreateRectangle(IntPoint p, byte color)
        {
            var c = Color.FromRgb(color, color, color);

            var r = new Rectangle()
            {
                Width = SCALE,
                Height = SCALE,
                Fill = new SolidColorBrush(c)
            };

            Point drawingPoint = ScaleIt(p);
            Canvas.SetLeft(r, drawingPoint.X);
            Canvas.SetTop(r, drawingPoint.Y);

            return r;
        }

        private void canvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            IntPoint p = UnScaleIt(e.GetPosition(canvas));
            RenderFrame(GetFrame(p));
        }

        public Point ScaleIt(IntPoint p)
        {
            return new Point(p.X * SCALE + RealWidth / 2 - SCALE/2 + PanShiftWidth * SCALE,
                             p.Y * SCALE + RealHeight / 2 - SCALE/2 + PanShiftHeight * SCALE);
        }

        public IntPoint UnScaleIt(Point p)
        {
            return new IntPoint((int)Math.Round((p.X - RealWidth / 2 - PanShiftWidth * SCALE) / SCALE),
                                (int)Math.Round((p.Y - RealHeight / 2 - PanShiftHeight * SCALE) / SCALE));
        }

        private void window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            RealWidth = e.NewSize.Width;
            RealHeight = e.NewSize.Height;
            RenderFrame(currentFrame);
        }

        private void window_KeyDown(object sender, KeyEventArgs e)
        {
            switch(e.Key)
            {
                case Key.Down: PanShiftHeight--; RenderFrame(currentFrame); break;
                case Key.Up: PanShiftHeight++; RenderFrame(currentFrame); break;
                case Key.Left: PanShiftWidth++; RenderFrame(currentFrame); break;
                case Key.Right: PanShiftWidth--; RenderFrame(currentFrame); break;
            }
        }

        private void canvas_MouseMove(object sender, MouseEventArgs e)
        {
            IntPoint p = UnScaleIt(e.GetPosition(canvas));
            Point p2 = ScaleIt(p);
            Canvas.SetLeft(MouseHover, p2.X);
            Canvas.SetTop(MouseHover, p2.Y);
        }
       
        private void canvas_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            SCALE += e.Delta > 0 ? 0.5 : -0.5;

            if (SCALE < 0.2)
            {
                SCALE = 0.2;
            }

            MouseHover.Width = SCALE;
            MouseHover.Height = SCALE;

            RenderFrame(currentFrame);
        }
    }
}
