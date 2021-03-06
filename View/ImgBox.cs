using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace WpfApp.View
{
    public partial class ImgBox : Window
    {
        private void Pen_Click(object sender, RoutedEventArgs e)
        {
            if (!mode.Equals(Mode.Pen))
            {
                ShutdownEraserMode();
                OpenPenMode();
            }
            else
            {
                ShutdownPenMode();
            }

        }

        private void Eraser_Click(object sender, RoutedEventArgs e)
        {
            if (!mode.Equals(Mode.Eraser))
            {
                ShutdownPenMode();
                OpenEraserMode();
            }
            else
            {
                ShutdownEraserMode();
            }
        }

        private void OpenEraserMode()
        {
            mode = Mode.Eraser;
            this.MouseLeftButtonDown += RemoveLine;
        }

        private void ShutdownEraserMode()
        {
            mode = Mode.Drag;
            this.MouseLeftButtonDown -= RemoveLine;
        }

        private void RemoveLine(object sender, MouseButtonEventArgs e)
        {
            Point p = e.GetPosition(this);
            IEnumerator ie = this.Panel.Children.GetEnumerator();
            Line cur = null;
            double min = 10;
            while (ie.MoveNext())
            {
                var elem = ie.Current as Line;
                double dis = GetDistance(p, elem);
                if (dis < min)
                {
                    cur = elem;
                    min = dis;
                }
            }
            if(cur != null)
            {
                this.Panel.Children.Remove(cur);
            }
        }

        private double GetDistance(Point p, Line l)
        {
            double k = (l.Y2 - l.Y1) / (l.X2 - l.X1);
            double a = k;
            double b = -1;
            double c = l.Y1 - k * l.X1;
            double fz = Math.Abs(a * p.X + b * p.Y + c);
            double fm = Math.Sqrt(a * a + b * b);
            return fz / fm;
        }

        private void OpenPenMode() 
        {
            mode = Mode.Pen;
            this.Cursor = Cursors.Pen;
            this.MouseLeftButtonUp += StopDrawLine;
            this.MouseLeftButtonDown += StartDrawLine;
            this.MouseMove += DrawLine;
        }

        private void ShutdownPenMode()
        {
            mode = Mode.Drag;
            this.Cursor = Cursors.Arrow;
            this.MouseLeftButtonUp -= StopDrawLine;
            this.MouseLeftButtonDown -= StartDrawLine;
            this.MouseMove -= DrawLine;
        }

        private Line line;

        private void StartDrawLine(object sender, MouseEventArgs e)
        {
            var p = e.GetPosition(this);
            line = new Line();
            line.Stroke = new SolidColorBrush(Color.FromArgb(255, 255, 0, 0));
            line.StrokeThickness = 3;
            line.X1 = p.X;
            line.Y1 = p.Y;
            this.Panel.Children.Add(line);
        }

        private void DrawLine(object sender, MouseEventArgs e)
        {
            if(e.LeftButton == MouseButtonState.Pressed)
            {
                Point p = e.GetPosition(this);
                if (line != null)
                {
                    line.X2 = p.X;
                    line.Y2 = p.Y;
                }
            }
        }

        private void StopDrawLine(object sender, MouseEventArgs e) 
        {
            line = null;
        }

        private void Bar_MouseEnter(object sender, MouseEventArgs e)
        {
            this.Bar.Width = rw;
        }

        private void Bar_MouseLeave(object sender, MouseEventArgs e)
        {
            this.Bar.Width = 15;
        }

        private void Bar_KeyDown(object sender, KeyEventArgs e)
        {
            if(e.Key.Equals(Key.Escape))
            {
                if (!mode.Equals(Mode.Drag))
                {
                    ShutdownEraserMode();
                    ShutdownPenMode();
                }
            }
        }
    }
}
