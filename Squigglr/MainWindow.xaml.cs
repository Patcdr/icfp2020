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
using IntPoint = System.Drawing.Point;
using Core;

namespace Squigglr
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private static readonly SolidColorBrush MouseHoverBrush = new SolidColorBrush(Colors.Green);

        private readonly Rectangle MouseHover;
        private readonly TextBlock OffScreen;
        private readonly TextBlock CurrentPosition;
        private readonly TextBlock Turn;
        private readonly TextBlock Ship;
        private int selectedShipId;

        private int OffScreenCount = 0;
        private readonly Frame frame;

        private readonly Rectangle MeasureStartingLocationBlock;
        private IntPoint? measureStartingLocationPoint;

        private GameStateGraphics gInterface;

        public MainWindow()
        {
            InitializeComponent();
            canvas.Background = new SolidColorBrush(Colors.Black);
            Scaler.ResizeWindow(window.DesiredSize);

            MouseHover = new Rectangle();
            Scaler.ResizeRectangle(MouseHover);
            MouseHover.Fill = MouseHoverBrush;

            OffScreen = new TextBlock();
            OffScreen.Text = "Offscreen Count Estimate: N/A";
            OffScreen.Foreground = new SolidColorBrush(Colors.Yellow);
            Canvas.SetLeft(OffScreen, 80);
            Canvas.SetTop(OffScreen, 0);
            OffScreen.Visibility = Visibility.Hidden;

            CurrentPosition = new TextBlock();
            CurrentPosition.Foreground = new SolidColorBrush(Colors.Yellow);
            Canvas.SetLeft(CurrentPosition, 0);
            Canvas.SetTop(CurrentPosition, 0);

            Turn = new TextBlock();
            Turn.Foreground = new SolidColorBrush(Colors.Yellow);
            Canvas.SetLeft(Turn, 0);
            Canvas.SetTop(Turn, 15);

            Ship = new TextBlock();
            Ship.Foreground = new SolidColorBrush(Colors.Yellow);
            Canvas.SetLeft(Ship, 0);
            Canvas.SetTop(Ship, 30);

            MeasureStartingLocationBlock = new Rectangle();
            Scaler.ResizeRectangle(MeasureStartingLocationBlock);
            MeasureStartingLocationBlock.Fill = new SolidColorBrush(Colors.Red);
            MeasureStartingLocationBlock.Visibility = Visibility.Hidden;

            // Default to the test server
            string serverUrl = "https://icfpc2020-api.testkontur.ru";
            string playerKey = "463bf8217ff3469189e1d9d15f8a29ce";

            Sender sender = new Sender(serverUrl, playerKey);
            HeadToHeadStrategy strategy = new HeadToHeadStrategy(sender);

            gInterface = new GameStateGraphics(strategy);
            gInterface.StartGame();

            frame = new Frame(gInterface);

            Render();
        }

        public void Render()
        {
            canvas.Children.Clear();

            OffScreenCount = 0;

            foreach (var rect in frame.Rects)
            {
                double x = Canvas.GetLeft(rect);
                double y = Canvas.GetTop(rect);

                if (x < 0 || x >= window.ActualWidth ||
                    y < 0 || y >= window.ActualHeight)
                {
                    OffScreenCount++;
                }

                canvas.Children.Add(rect);
            }

            foreach (var label in frame.NumberOverlays.Values)
            {
                canvas.Children.Add(label);
            }

            canvas.Children.Add(MouseHover);

            if (OffScreenCount > 0)
            {
                OffScreen.Text = "Offscreen Count Estimate: " + OffScreenCount;
                OffScreen.Visibility = Visibility.Visible;
                canvas.Children.Add(OffScreen);
            }
            else
            {
                OffScreen.Visibility = Visibility.Hidden;
                canvas.Children.Add(OffScreen);
            }

            canvas.Children.Add(CurrentPosition);
            canvas.Children.Add(MeasureStartingLocationBlock);

            RenderGameState();
        }

        public void Update()
        {
            Render();
        }

        private void RenderGameState()
        {
            GameState gameState = gInterface.GameState;

            //DrawRectangle(0, 0, gameState.StarSize, 128);

            foreach (var ship in gameState.Ships)
            {
                bool isAttacker = (ship.PlayerID == gameState.PlayerId && gameState.IsAttacker);

                TextBlock shipIndicator = new TextBlock();
                shipIndicator.Foreground = new SolidColorBrush(isAttacker ? Colors.Red : Colors.Green);
                shipIndicator.Text = $"{ship.ID}";

                Point p = Scaler.Convert(ship.Position);
                Canvas.SetLeft(shipIndicator, p.X - 2);
                Canvas.SetTop(shipIndicator, p.Y + 3);

                canvas.Children.Add(shipIndicator);

                /*
                Line line = new Line();
                line.Stroke = new SolidColorBrush(Colors.Crimson);
                line.StrokeThickness = 3;
                line.X1 = p.X;
                line.Y1 = p.Y;
                line.X2 = 0;
                line.Y2 = 0;
                canvas.Children.Add(line);
                */
            }

            canvas.Children.Remove(Turn);
            Turn.Text = $"Turn: {gameState.CurrentTurn} of {gameState.TotalTurns}";
            canvas.Children.Add(Turn);

            canvas.Children.Remove(Ship);
            Ship.Text = $"Ship: {gameState.Ships[0].Role}\n{gameState.Ships[0].Position}";
            canvas.Children.Add(Ship);
        }

        private void DrawRectangle(long x, long y, long radius, byte color)
        {
            var c = Color.FromRgb(color, color, color);

            var r = new Rectangle();

            Scaler.ResizeRectangle(r, radius);
            r.Fill = new SolidColorBrush(c);

            Point drawingPoint = Scaler.Convert(new IntPoint((int)x, (int)y));
            Canvas.SetLeft(r, drawingPoint.X - radius);
            Canvas.SetTop(r, drawingPoint.Y - radius);

            canvas.Children.Add(r);
        }

        private void canvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            IntPoint p = Scaler.Convert(e.GetPosition(canvas));
            frame.Advance(p);
            Render();
        }

        /// <summary>
        /// Measure distance from a point
        /// </summary>
        private void Canvas_OnMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (!measureStartingLocationPoint.HasValue)
            {
                measureStartingLocationPoint = Scaler.Convert(e.GetPosition(canvas));
                Point p2 = Scaler.Convert(measureStartingLocationPoint.Value);
                Canvas.SetLeft(MeasureStartingLocationBlock, p2.X);
                Canvas.SetTop(MeasureStartingLocationBlock, p2.Y);
                MeasureStartingLocationBlock.Visibility = Visibility.Visible;
            }
            else
            {
                IntPoint end = Scaler.Convert(e.GetPosition(canvas));
                var sb = new StringBuilder();
                IntPoint start = measureStartingLocationPoint.Value;
                sb.AppendLine($"Original point: ({start.X}, {start.Y})");
                sb.AppendLine($"Current point: ({end.X}, {end.Y})");
                sb.AppendLine($"DeltaX: ({end.X - start.X}) DeltaY: ({end.Y - start.Y})");
                MessageBox.Show(sb.ToString());

                measureStartingLocationPoint = null;
                MeasureStartingLocationBlock.Visibility = Visibility.Hidden;
            }
        }

        private void window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            Scaler.ResizeWindow(e.NewSize);
            Update();
        }

        private void window_KeyDown(object sender, KeyEventArgs e)
        {
            switch(e.Key)
            {
                case Key.Down: Scaler.ShiftView(vertical: false); Update(); break;
                case Key.Up: Scaler.ShiftView(vertical: true); Update(); break;
                case Key.Left: Scaler.ShiftView(horizontal: true); Update(); break;
                case Key.Right: Scaler.ShiftView(horizontal: false); Update(); break;
                case Key.Z: frame.Undo(); Render(); break;
            }
        }

        private void canvas_MouseMove(object sender, MouseEventArgs e)
        {
            IntPoint p = Scaler.Convert(e.GetPosition(canvas));
            Point p2 = Scaler.Convert(p);
            Canvas.SetLeft(MouseHover, p2.X);
            Canvas.SetTop(MouseHover, p2.Y);
            CurrentPosition.Text = $"({p.X}, {p.Y})";
        }

        private void canvas_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            Scaler.Zoom(e.Delta > 0);
            Scaler.ResizeRectangle(MouseHover);
            Update();
        }

        private void HelpButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Click = Step\n");
        }

    }
}
