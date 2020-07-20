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
using System.Globalization;

namespace Squigglr
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly TextBlock CurrentPosition;
        private readonly TextBlock Turn;

        private IntPoint mousePosition;
        private readonly Rectangle MeasureStartingLocationBlock;
        private IntPoint? measureStartingLocationPoint;

        private DoubleRunner runner;
        private GameState gameState;
        private Sender player1Sender;
        private Sender player2Sender;

        private bool running;
        private Task continuousRunner;

        public MainWindow()
        {
            InitializeComponent();
            canvas.Background = new SolidColorBrush(Colors.Black);
            Scaler.ResizeWindow(window.DesiredSize);
            WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen;

            CurrentPosition = new TextBlock();
            CurrentPosition.Foreground = new SolidColorBrush(Colors.Yellow);
            Canvas.SetLeft(CurrentPosition, 0);
            Canvas.SetTop(CurrentPosition, 0);

            Turn = new TextBlock();
            Turn.Foreground = new SolidColorBrush(Colors.Yellow);
            Canvas.SetLeft(Turn, 0);
            Canvas.SetTop(Turn, 15);

            MeasureStartingLocationBlock = new Rectangle();
            Scaler.ResizeRectangle(MeasureStartingLocationBlock, 1);
            MeasureStartingLocationBlock.Fill = new SolidColorBrush(Colors.Red);
            MeasureStartingLocationBlock.Visibility = Visibility.Hidden;

            // Default to the test server
            string serverUrl = "https://icfpc2020-api.testkontur.ru";
            string playerKey = "463bf8217ff3469189e1d9d15f8a29ce";

            player1Sender = new Sender(serverUrl, playerKey);
            player2Sender = new Sender(serverUrl, playerKey);
            runner = new DoubleRunner(
                    new Sender(serverUrl, playerKey),
                    new DontDieRunner(player1Sender),
                    new DontDieRunner(player2Sender));

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

            // Render commands
            if (gameState.GameStateVal == 1)
            {
                RenderCommands(player1Sender.LastSentValue);
                RenderCommands(player2Sender.LastSentValue);
            }

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

                // Ship stats
                Color statColor = Color.FromRgb(90, 90, 90);
                DrawText(x, y + 3, statColor, $"{ship.ID}");
                DrawText(x + 2, y, statColor, $"F:{ship.Health}\nL:{ship.Lazers}\nC:{ship.Cooling}", false);

                //DrawLine(x, y, 0, 0, Colors.Crimson);
            }

            canvas.Children.Remove(Turn);
            Turn.Text = $"Turn: {gameState.CurrentTurn} of {gameState.TotalTurns}";
            canvas.Children.Add(Turn);

            // Draw mouse hover
            FillRectangle(mousePosition.X, mousePosition.Y, 0.5, Colors.Yellow);
            CurrentPosition.Text = $"({mousePosition.X}, {mousePosition.Y})";
        }

        private void RenderCommands(Value value)
        {
            long sendType = UtilityFunctions.Addr("car", value).AsNumber();

            // Only type 4 is commands
            if (sendType != 4)
            {
                return;
            }

            var ships = new Dictionary<long, Ship>();
            foreach(var ship in gameState.Ships)
            {
                ships[ship.ID] = ship;
            }

            var commands = UtilityFunctions.Addr("cddar", value);

            foreach (var command in UtilityFunctions.ListAsEnumerable(commands, null))
            {
                long code = UtilityFunctions.Addr("car", command).AsNumber();
                long shipId = UtilityFunctions.Addr("cdar", command).AsNumber();
                Ship ship = ships[shipId];

                // Thrust
                if (code == 0)
                {
                    var vector = UtilityFunctions.Addr("cddar", command);
                    int vx = ship.Position.X + (int)vector.Car().AsNumber() * 5;
                    int vy = ship.Position.Y + (int)vector.Cdr().AsNumber() * 5;

                    DrawLine(ship.Position.X, ship.Position.Y, vx, vy, Colors.DarkSlateGray);
                }
                // Explode
                else if (code == 1)
                {
                    DrawCircle(ship.Position.X, ship.Position.Y, 4, Colors.OrangeRed);
                    DrawCircle(ship.Position.X, ship.Position.Y, 5, Colors.OrangeRed);
                    DrawCircle(ship.Position.X, ship.Position.Y, 6, Colors.OrangeRed);
                }
                // Lazer
                else if (code == 2)
                {
                    var target = UtilityFunctions.Addr("cddar", command);
                    int tx = (int)target.Car().AsNumber();
                    int ty = (int)target.Cdr().AsNumber();

                    DrawLine(ship.Position.X, ship.Position.Y, tx, ty, Colors.Crimson);
                }
            }

        }

        private void DrawText(int x, int y, Color color, string text, bool center = true)
        {
            FormattedText formattedText = new FormattedText(
                text,
                CultureInfo.GetCultureInfo("en-us"),
                FlowDirection.LeftToRight,
                new Typeface("Courier New"),
                10,
                new SolidColorBrush(color));

            TextBlock textBlock = new TextBlock();
            textBlock.IsHitTestVisible = false;
            textBlock.Foreground = new SolidColorBrush(color);
            textBlock.Text = text;
            textBlock.FontSize = 10;
            textBlock.LineHeight = 10;
            textBlock.LineStackingStrategy = LineStackingStrategy.BlockLineHeight;
            textBlock.FontFamily = new FontFamily("Courier New");

            if (center)
            {
                textBlock.TextAlignment = TextAlignment.Center;
                Point p = Scaler.ConvertGridToScreen(x, y);
                Canvas.SetLeft(textBlock, p.X - formattedText.Width / 2);
                Canvas.SetTop(textBlock, p.Y - formattedText.Height / 2);
            } else
            {
                Point p = Scaler.ConvertGridToScreen(x, y);
                Canvas.SetLeft(textBlock, p.X);
                Canvas.SetTop(textBlock, p.Y - formattedText.Height / 2);
            }

            canvas.Children.Add(textBlock);
        }

        private void DrawRectangle(long x, long y, double radius, Color color)
        {
            var r = new Rectangle();
            r.IsHitTestVisible = false;

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
            r.IsHitTestVisible = false;

            Scaler.ResizeRectangle(r, radius * 2);
            r.Fill = new SolidColorBrush(color);

            Point drawingPoint = Scaler.ConvertGridToScreen(x - radius, y - radius);
            Canvas.SetLeft(r, drawingPoint.X);
            Canvas.SetTop(r, drawingPoint.Y);

            canvas.Children.Add(r);
        }

        private void DrawCircle(long x, long y, double radius, Color color)
        {
            var r = new Ellipse();
            r.IsHitTestVisible = false;
            r.Width = Scaler.Scale * radius * 2;
            r.Height = Scaler.Scale * radius * 2;

            r.Stroke = new SolidColorBrush(color);
            r.StrokeThickness = 1;

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
            line.IsHitTestVisible = false;
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
