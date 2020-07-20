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

namespace Squigglr
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private static readonly SolidColorBrush MouseHoverBrush = new SolidColorBrush(Colors.Green);

        private readonly Rectangle MouseHover;
        private readonly TextBlock CurrentPosition;
        private readonly TextBlock Turn;
        private readonly TextBlock Ship;
        private int selectedShipId;

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

            MouseHover = new Rectangle();
            Scaler.ResizeRectangle(MouseHover);
            MouseHover.Fill = MouseHoverBrush;

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

            canvas.Children.Add(MouseHover);

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
                FillRectangle(x, y, 1, color);
                FillRectangle(x - 1, y + 1, 1, color);
                FillRectangle(x - 1, y - 1, 1, color);
                FillRectangle(x + 1, y + 1, 1, color);
                FillRectangle(x + 1, y - 1, 1, color);

                DrawText(x, y + 2, color, $"{ship.ID}");

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

        private void DrawText(long x, long y, Color color, String text, bool center = true)
        {
            TextBlock textBlock = new TextBlock();
            textBlock.Foreground = new SolidColorBrush(color);
            textBlock.Text = text;

            if (center)
            {
                textBlock.HorizontalAlignment = HorizontalAlignment.Center;
            }

            Point p = Scaler.Convert(new IntPoint((int)x, (int)y));
            Canvas.SetLeft(textBlock, p.X);
            Canvas.SetTop(textBlock, p.Y);

            canvas.Children.Add(textBlock);
        }

        private void DrawRectangle(long x, long y, long size, Color color)
        {
            var r = new Rectangle();

            Scaler.ResizeRectangle(r, size);
            r.Stroke = new SolidColorBrush(color);
            r.Fill = null;

            Point drawingPoint = Scaler.Convert(new IntPoint((int)(x - size / 2), (int)(y - size / 2)));
            Canvas.SetLeft(r, drawingPoint.X);
            Canvas.SetTop(r, drawingPoint.Y);

            canvas.Children.Add(r);
        }

        private void FillRectangle(long x, long y, long size, Color color)
        {
            var r = new Rectangle();

            Scaler.ResizeRectangle(r, size);
            r.Fill = new SolidColorBrush(color);

            Point drawingPoint = Scaler.Convert(new IntPoint((int)(x - size / 2), (int)(y - size / 2)));
            Canvas.SetLeft(r, drawingPoint.X);
            Canvas.SetTop(r, drawingPoint.Y);

            canvas.Children.Add(r);
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
            Render();
        }

        private void HelpButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Spacebar = Step\nR = Run/Pause");
        }

    }
}
