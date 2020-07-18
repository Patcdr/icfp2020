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
        public const int SCALE = 10;
        
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
        private Dictionary<IntPoint, byte> currentFrame;

        public MainWindow()
        {
            InitializeComponent();
            canvas.Background = new SolidColorBrush(Colors.Black);
            RealWidth = window.Width;
            RealHeight = window.Height;

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

            IntPoint drawingPoint = ScaleIt(p);
            Canvas.SetLeft(r, drawingPoint.X);
            Canvas.SetTop(r, drawingPoint.Y);

            return r;
        }

        private void canvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            IntPoint p = UnScaleIt(e.GetPosition(canvas));
            RenderFrame(GetFrame(p));
        }

        public IntPoint ScaleIt(IntPoint p)
        {
            return new IntPoint((int)(p.X * SCALE + RealWidth / 2 - SCALE/2),
                                (int)(p.Y * SCALE + RealHeight / 2 - SCALE/2));
        }

        public IntPoint UnScaleIt(Point p)
        {
            return new IntPoint((int)((p.X - RealWidth / 2 + SCALE/2) / SCALE),
                                (int)((p.Y - RealHeight / 2 + SCALE/2) / SCALE));
        }

        private void window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            RealWidth = e.NewSize.Width;
            RealHeight = e.NewSize.Height;
            RenderFrame(currentFrame);
        }
    }
}
