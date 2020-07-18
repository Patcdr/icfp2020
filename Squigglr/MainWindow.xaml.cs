using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
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
        private int OffScreenCount = 0;

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
            OffScreenCount = 0;

            foreach (var pair in frame)
            {
                canvas.Children.Add(CreateRectangle(pair.Key, pair.Value));
            }

            canvas.Children.Add(MouseHover);

            if (OffScreenCount > 0)
            {
                var textBlock = new TextBlock();
                textBlock.Text = "Offscreen Count Estimate: " + OffScreenCount;
                textBlock.Foreground = new SolidColorBrush(Colors.Yellow);
                Canvas.SetLeft(textBlock, 10);
                Canvas.SetTop(textBlock, 10);
                canvas.Children.Add(textBlock);
            }
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

            if (drawingPoint.X < 0 || drawingPoint.X >= window.ActualWidth ||
                drawingPoint.Y < 0 || drawingPoint.Y >= window.ActualHeight)
            {
                OffScreenCount++;
            }

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
                case Key.Z: currentFrame = gInterface.UndoState(); RenderFrame(currentFrame); break;
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

        private void HelpButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Click = Send Point\n" + 
                            "Scroll = Zoom\n" +
                            "Arrow Keys = Pan\n" +
                            "Z = Undo\n");
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            gInterface.SaveClicks();
            MessageBox.Show("Saved");
        }

        private void LoadButton_Click(object sender, RoutedEventArgs e)
        {
            currentFrame = gInterface.LoadClicks();
            RenderFrame(currentFrame);
        }

        private void CalibrateButton_Click(object sender, RoutedEventArgs e)
        {
            GetFrame(new IntPoint(0, 0));
            GetFrame(new IntPoint(0, 0));
            GetFrame(new IntPoint(0, 0));
            GetFrame(new IntPoint(0, 0));
            GetFrame(new IntPoint(0, 0));
            GetFrame(new IntPoint(0, 0));
            GetFrame(new IntPoint(0, 0));

            GetFrame(new IntPoint(8, 4));
            GetFrame(new IntPoint(2, -8));
            GetFrame(new IntPoint(3, 6));
            GetFrame(new IntPoint(0, -14));
            GetFrame(new IntPoint(-4, 10));
            GetFrame(new IntPoint(9, -3));
            GetFrame(new IntPoint(-4, 10));
            currentFrame = GetFrame(new IntPoint(1, 4));

            RenderFrame(currentFrame);
            CalibrateButton.Visibility = Visibility.Hidden;
        }
    }
}
