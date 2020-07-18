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
        GraphicsInterface gInterface;
        private Dictionary<IntPoint, byte> currentFrame;
        private readonly Rectangle MouseHover;

        public MainWindow()
        {
            InitializeComponent();
            canvas.Background = new SolidColorBrush(Colors.Black);
            Scaler.ResizeWindow(window.DesiredSize);

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

            var r = new Rectangle();

            Scaler.ResizeRectangle(r);
            r.Fill = new SolidColorBrush(c);
            
            Point drawingPoint = Scaler.Convert(p);
            Canvas.SetLeft(r, drawingPoint.X);
            Canvas.SetTop(r, drawingPoint.Y);

            return r;
        }

        private void canvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            IntPoint p = Scaler.Convert(e.GetPosition(canvas));
            RenderFrame(GetFrame(p));
        }

        private void window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            Scaler.ResizeWindow(e.NewSize);
            RenderFrame(currentFrame);
        }

        private void window_KeyDown(object sender, KeyEventArgs e)
        {
            switch(e.Key)
            {
                case Key.Down: Scaler.ShiftView(vertical: false); RenderFrame(currentFrame); break;
                case Key.Up: Scaler.ShiftView(vertical: true); RenderFrame(currentFrame); break;
                case Key.Left: Scaler.ShiftView(horizontal: true); RenderFrame(currentFrame); break;
                case Key.Right: Scaler.ShiftView(horizontal: false); RenderFrame(currentFrame); break;
            }
        }

        private void canvas_MouseMove(object sender, MouseEventArgs e)
        {
            IntPoint p = Scaler.Convert(e.GetPosition(canvas));
            Point p2 = Scaler.Convert(p);
            Canvas.SetLeft(MouseHover, p2.X);
            Canvas.SetTop(MouseHover, p2.Y);
        }
       
        private void canvas_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            Scaler.Zoom(e.Delta > 0);
            Scaler.ResizeRectangle(MouseHover);
            RenderFrame(currentFrame);
        }
    }
}
