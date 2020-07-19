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
        private readonly Rectangle MouseHover;
        private readonly Rectangle MeasureStartingLocation;
        private readonly TextBlock OffScreen;
        private readonly TextBlock CurrentPosition;
        private int OffScreenCount = 0;
        private readonly Frame frame;

        Sender sender;
        Interactor interactor;

        public MainWindow()
        {
            InitializeComponent();
            canvas.Background = new SolidColorBrush(Colors.Black);
            Scaler.ResizeWindow(window.DesiredSize);

            MouseHover = new Rectangle();
            Scaler.ResizeRectangle(MouseHover);
            MouseHover.Fill = new SolidColorBrush(Colors.Green);

            OffScreen = new TextBlock();
            OffScreen.Text = "Offscreen Count Estimate: N/A";
            OffScreen.Foreground = new SolidColorBrush(Colors.Yellow);
            Canvas.SetLeft(OffScreen, 10);
            Canvas.SetTop(OffScreen, 10);
            OffScreen.Visibility = Visibility.Hidden;

            CurrentPosition = new TextBlock();
            CurrentPosition.Foreground = new SolidColorBrush(Colors.Yellow);
            Canvas.SetLeft(OffScreen, 10);
            Canvas.SetTop(OffScreen, 25);

            // Default to the test server
            string serverUrl = "https://icfpc2020-api.testkontur.ru";
            string playerKey = "463bf8217ff3469189e1d9d15f8a29ce";

            sender = new Sender(serverUrl, playerKey);
            interactor = new Interactor(sender);

            var gInterface = new UIInteractor(interactor);

            frame = new Frame(gInterface);

            // Fast forward
            frame.AdvanceMany(new List<IntPoint>()
            {
                new IntPoint(0, 0),
                new IntPoint(0, 0),
                new IntPoint(0, 0),
                new IntPoint(0, 0),
                new IntPoint(0, 0),
                new IntPoint(0, 0),
                new IntPoint(0, 0),
                new IntPoint(0, 0),
                new IntPoint(0, 0),

                new IntPoint(8, 4),
                new IntPoint(2, -8),
                new IntPoint(3, 6),
                new IntPoint(0, -14),
                new IntPoint(-4, 10),
                new IntPoint(9, -3),
                new IntPoint(-4, 10),
                new IntPoint(1, 4),
                new IntPoint(-3, 20),

                new IntPoint(0, 0),
                new IntPoint(0, 0),
                new IntPoint(0, 0),
                new IntPoint(0, 0),
                new IntPoint(0, 0),
                new IntPoint(0, 0),
                new IntPoint(0, 0),
                new IntPoint(0, 0),
                new IntPoint(0, 0),
                new IntPoint(0, 0),
                new IntPoint(0, 0),
                new IntPoint(0, 0),
            });

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
        }

        public void Update()
        {
            frame.Update();

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
            }

            if (OffScreenCount > 0)
            {
                OffScreen.Text = "Offscreen Count Estimate: " + OffScreenCount;
                OffScreen.Visibility = Visibility.Visible;
            }
            else
            {
                OffScreen.Visibility = Visibility.Hidden;
            }
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
                case Key.N: frame.StartGame(); Render(); break;
                case Key.A: RunAttackAI(); Render(); break;
                case Key.D: RunDefendAI(); Render(); break;
                case Key.Z: frame.Undo(); Render(); break;
            }
        }

        private void RunDefendAI() {}
        private void RunAttackAI()
        {
            frame.StartGame();
            frame.Advance(new IntPoint(44, 00));

            var strategy = new HeadToHeadStrategy(interactor, (Value state) => {
                if (state != null) frame.SetState(state);
            });
            strategy.AttackStep = (Value state) => {
                if (state != null) frame.SetState(state);
            };
            strategy.Run();
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
            MessageBox.Show("Click = Send Point\n" +
                            "Scroll = Zoom\n" +
                            "Arrow Keys = Pan\n" +
                            "N = Open New Game\n" +
                            "Z = Undo\n");
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog();
            dlg.FileName = "SavedClicks"; // Default file name
            dlg.DefaultExt = ".saves"; // Default file extension
            dlg.Filter = "Saves (.saves)|*.saves"; // Filter files by extension

            // Show save file dialog box
            bool? result = dlg.ShowDialog();

            // Process save file dialog box results
            if (result == true)
            {
                // Save document
                string filename = dlg.FileName;

                frame.Save(filename);
                MessageBox.Show("Saved");
            }
        }

        private void LoadButton_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.FileName = "SavedClicks";
            dlg.DefaultExt = ".saves";
            dlg.Filter = "Saves (.saves)|*.saves";

            bool? result = dlg.ShowDialog();

            if (result == true)
            {
                string filename = dlg.FileName;

                frame.Load(filename);
                Render();
            }
        }

        private void CalibrateButton_Click(object sender, RoutedEventArgs e)
        {
            frame.AdvanceMany(new List<IntPoint>()
            {
                new IntPoint(0, 0),
                new IntPoint(0, 0),
                new IntPoint(0, 0),
                new IntPoint(0, 0),
                new IntPoint(0, 0),
                new IntPoint(0, 0),
                new IntPoint(0, 0),

                new IntPoint(8, 4),
                new IntPoint(2, -8),
                new IntPoint(3, 6),
                new IntPoint(0, -14),
                new IntPoint(-4, 10),
                new IntPoint(9, -3),
                new IntPoint(-4, 10),
                new IntPoint(1, 4)
            });

            Render();
            CalibrateButton.Visibility = Visibility.Hidden;
        }
    }
}
