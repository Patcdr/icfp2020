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
using System.Threading;
using System.Printing;

namespace Squigglr
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly TextBlock CurrentPosition;
        private readonly TextBlock Turn;
        private readonly TextBlock Ship;
        private int selectedShipId;

        private IntPoint mousePosition;
        private readonly Rectangle MeasureStartingLocationBlock;
        private IntPoint? measureStartingLocationPoint;

        private DoubleRunner runner;
        private GameState gameState;

        private bool running;
        private Task continuousRunner;

        public MainWindow()
        {
            InitializeComponent();
            canvas.Background = new SolidColorBrush(Colors.Black);
            Scaler.ResizeWindow(window.DesiredSize);

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
            Scaler.ResizeRectangle(MeasureStartingLocationBlock, 1);
            MeasureStartingLocationBlock.Fill = new SolidColorBrush(Colors.Red);
            MeasureStartingLocationBlock.Visibility = Visibility.Hidden;

            // Default to the test server
            string serverUrl = "https://icfpc2020-api.testkontur.ru";
            string playerKey = "463bf8217ff3469189e1d9d15f8a29ce";

            runner = new DoubleRunner(
                    new Sender(serverUrl, playerKey),
                    new DontDieRunner(new Sender(serverUrl, playerKey)),
                    new DontDieRunner(new Sender(serverUrl, playerKey)));

            gameState = runner.Join();

            Render();
        }

        public void Render()
        {
            canvas.Children.Clear();

            canvas.Children.Add(CurrentPosition);
            canvas.Children.Add(MeasureStartingLocationBlock);

            // Render the GameState
            FillRectangle(0, 0, gameState.StarSize, Colors.DarkGray);
            DrawRectangle(0, 0, gameState.ArenaSize, Colors.DarkGray);

            foreach (var ship in gameState.Ships)
            {
                int x = ship.Position.X;
                int y = ship.Position.Y;
                bool isAttacker = (ship.PlayerID == gameState.PlayerId && gameState.IsAttacker);
                Color color = (isAttacker ? Colors.Red : Colors.Green);

                // Draw pixelized X
                FillRectangle(x, y, 0.5, color);
                FillRectangle(x - 1, y + 1, 0.5, color);
                FillRectangle(x - 1, y - 1, 0.5, color);
                FillRectangle(x + 1, y + 1, 0.5, color);
                FillRectangle(x + 1, y - 1, 0.5, color);

                DrawText(x, y + 2, color, $"{ship.ID}");

                //DrawLine(x, y, 0, 0, Colors.Crimson);
            }

            canvas.Children.Remove(Turn);
            Turn.Text = $"Turn: {gameState.CurrentTurn} of {gameState.TotalTurns}";
            canvas.Children.Add(Turn);

            canvas.Children.Remove(Ship);
            Ship.Text = $"Ship: {gameState.Ships[0].Role}\n{gameState.Ships[0].Position}";
            canvas.Children.Add(Ship);

            // Draw mouse hover
            FillRectangle(mousePosition.X, mousePosition.Y, 0.5, Colors.Yellow);
            CurrentPosition.Text = $"({mousePosition.X}, {mousePosition.Y})";
        }

        private void DrawText(int x, int y, Color color, String text, bool center = true)
        {
            TextBlock textBlock = new TextBlock();
            textBlock.Foreground = new SolidColorBrush(color);
            textBlock.Text = text;

            if (center)
            {
                textBlock.HorizontalAlignment = HorizontalAlignment.Center;
            }

            Point p = Scaler.ConvertGridToScreen(x, y);
            Canvas.SetLeft(textBlock, p.X);
            Canvas.SetTop(textBlock, p.Y);

            canvas.Children.Add(textBlock);
        }

        private void DrawRectangle(long x, long y, double radius, Color color)
        {
            var r = new Rectangle();

            Scaler.ResizeRectangle(r, radius * 2);
            r.Stroke = new SolidColorBrush(color);
            r.Fill = null;

            Point drawingPoint = Scaler.ConvertGridToScreen(x - radius, y - radius);
            Canvas.SetLeft(r, drawingPoint.X);
            Canvas.SetTop(r, drawingPoint.Y);

            canvas.Children.Add(r);
        }

        private void FillRectangle(long x, long y, double radius, Color color)
        {
            var r = new Rectangle();

            Scaler.ResizeRectangle(r, radius * 2);
            r.Fill = new SolidColorBrush(color);

            Point drawingPoint = Scaler.ConvertGridToScreen(x - radius, y - radius);
            Canvas.SetLeft(r, drawingPoint.X);
            Canvas.SetTop(r, drawingPoint.Y);

            canvas.Children.Add(r);
        }

        private void DrawLine(int x1, int y1, int x2, int y2, Color color)
        {
            Point p1 = Scaler.ConvertGridToScreen(x1, y1);
            Point p2 = Scaler.ConvertGridToScreen(x2, y2);

            Line line = new Line();
            line.Stroke = new SolidColorBrush(color);
            line.StrokeThickness = 3;
            line.X1 = p1.X;
            line.Y1 = p1.Y;
            line.X2 = p2.X;
            line.Y2 = p2.Y;

            canvas.Children.Add(line);
        }

        private void StepOne()
        {
            gameState = runner.Step();
            Render();
        }

        private void ToggleRun()
        {
            if (running)
            {
                running = false;

                if (continuousRunner != null)
                {
                    continuousRunner.Wait();
                }
            }
            else
            {
                running = true;

                continuousRunner = Task.Factory.StartNew(() =>
                {
                    while (running)
                    {
                        Dispatcher.Invoke(StepOne);
                        Thread.Sleep(100);
                    }
                });
            }
        }

        private void canvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            //IntPoint p = Scaler.Convert(e.GetPosition(canvas));
        }

        /// <summary>
        /// Measure distance from a point
        /// </summary>
        private void Canvas_OnMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (!measureStartingLocationPoint.HasValue)
            {
                double sx = e.GetPosition(canvas).X;
                double sy = e.GetPosition(canvas).Y;
                measureStartingLocationPoint = Scaler.ConvertScreenToGrid(sx, sy);

                int x = measureStartingLocationPoint.Value.X;
                int y = measureStartingLocationPoint.Value.Y;
                Point p2 = Scaler.ConvertGridToScreen(x, y);
                Canvas.SetLeft(MeasureStartingLocationBlock, p2.X);
                Canvas.SetTop(MeasureStartingLocationBlock, p2.Y);
                MeasureStartingLocationBlock.Visibility = Visibility.Visible;
            }
            else
            {
                double x = e.GetPosition(canvas).X;
                double y = e.GetPosition(canvas).Y;
                IntPoint end = Scaler.ConvertScreenToGrid(x, y);

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
            Render();
        }

        private void window_KeyDown(object sender, KeyEventArgs e)
        {
            switch(e.Key)
            {
                case Key.Down: Scaler.ShiftView(vertical: false); Render(); break;
                case Key.Up: Scaler.ShiftView(vertical: true); Render(); break;
                case Key.Left: Scaler.ShiftView(horizontal: true); Render(); break;
                case Key.Right: Scaler.ShiftView(horizontal: false); Render(); break;
                case Key.Space: StepOne(); break;
                case Key.R: ToggleRun(); break;
            }
        }

        private void canvas_MouseMove(object sender, MouseEventArgs e)
        {
            double x = e.GetPosition(canvas).X;
            double y = e.GetPosition(canvas).Y;
            mousePosition = Scaler.ConvertScreenToGrid(x, y);
            Render();
        }

        private void canvas_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            Scaler.Zoom(e.Delta > 0);
            Render();
        }

        private void HelpButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Spacebar = Step\nR = Run/Pause");
        }

    }
}
