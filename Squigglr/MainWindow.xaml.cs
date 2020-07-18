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

        GraphicsInterface graphicsSingleton = new TempGraphics();

        public MainWindow()
        {
            InitializeComponent();
            canvas.Background = new SolidColorBrush(Colors.Black);
            canvas.Width = window.Width;
            canvas.Height = window.Height;
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
            RenderFrame(graphicsSingleton.AdvanceState(p));
        }

        public IntPoint ScaleIt(IntPoint p)
        {
            return new IntPoint((int)(p.X * SCALE + canvas.Width / 2 - SCALE/2),
                                (int)(p.Y * SCALE + canvas.Height / 2 - SCALE/2));
        }

        public IntPoint UnScaleIt(Point p)
        {
            return new IntPoint((int)((p.X - canvas.Width / 2 + SCALE/2) / SCALE),
                                (int)((p.Y - canvas.Height / 2 + SCALE/2) / SCALE));
        }
    }
}
