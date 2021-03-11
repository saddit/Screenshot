using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using WpfApp.View;

namespace WpfApp
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        private NotifyIcon notifyIcon;
        private KeyboardHook keyHook;

        public MainWindow()
        {
            InitializeComponent();
            InitNotifyIcon();

            keyHook = new KeyboardHook();
            keyHook.KeyDownEvent += new System.Windows.Forms.KeyEventHandler(Awake);
            keyHook.Start();

            this.ResizeMode = ResizeMode.NoResize;

            //全屏
            this.Left = 0f;
            this.Top = 0f;
            this.Width = SystemParameters.PrimaryScreenWidth;
            this.Height = SystemParameters.PrimaryScreenHeight;

            canvas.Width = Width;
            canvas.Height = Height;

            hasOpen = new List<View.ImgBox>();
        }

        private void InitNotifyIcon()
        {
            notifyIcon = new NotifyIcon();
            notifyIcon.Text = "点击截屏";

            notifyIcon.Icon = Properties.Resources.logo;
            notifyIcon.Visible = true;
            notifyIcon.MouseClick += new System.Windows.Forms.MouseEventHandler(Awake);
                
            System.Windows.Forms.MenuItem quitMenuItem = new System.Windows.Forms.MenuItem("退出");
            System.Windows.Forms.MenuItem hideAllItem = new System.Windows.Forms.MenuItem("隐藏全部");
            System.Windows.Forms.MenuItem showAllItem = new System.Windows.Forms.MenuItem("显示全部");
            System.Windows.Forms.MenuItem startItem = new System.Windows.Forms.MenuItem("开始截图");
            startItem.Shortcut = Shortcut.CtrlQ;

            quitMenuItem.Click += new EventHandler(quitMenuItem_Click);
            hideAllItem.Click += new EventHandler((o, e) => EnumAllBox("hide"));
            showAllItem.Click += new EventHandler((o, e) => EnumAllBox("show"));
            startItem.Click += new EventHandler((o, e) => this.Show());

            //将上面的自选项加入到parentMenuitem中。
            System.Windows.Forms.MenuItem[] parentMenuitem = new System.Windows.Forms.MenuItem[] { startItem, hideAllItem, showAllItem, quitMenuItem };
            //为notifyIconContextMenu。
            notifyIcon.ContextMenu = new System.Windows.Forms.ContextMenu(parentMenuitem);
            this.ShowInTaskbar = false;
        }

        private void quitMenuItem_Click(object sender, EventArgs e)
        {
            EnumAllBox("close");
            this.Close();
        }

        private Bitmap Screenshot()
        {
            int w = Convert.ToInt32(SystemParameters.PrimaryScreenWidth);
            int h = Convert.ToInt32(SystemParameters.PrimaryScreenHeight);
            Bitmap bitImg = new Bitmap(w, h);
            using (Graphics g = Graphics.FromImage(bitImg))
            {
                g.CopyFromScreen(0, 0, 0, 0, new System.Drawing.Size(w, h));
            }
            return bitImg;
        }

        private void WindowActivated(object sender, EventArgs e)
        {
            EnumAllBox("hide");

            Bitmap bm = Screenshot();
            img.Source = Imaging.CreateBitmapSourceFromHBitmap(
                bm.GetHbitmap(),
                IntPtr.Zero,
                Int32Rect.Empty,
                BitmapSizeOptions.FromEmptyOptions());
        }

        private System.Windows.Point center;
        private System.Windows.Shapes.Rectangle capture;

        private void StartDrawRect(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                center = e.GetPosition(this);
                System.Windows.Shapes.Rectangle rect = new System.Windows.Shapes.Rectangle();
                rect.Fill = new SolidColorBrush(System.Windows.Media.Color.FromArgb(0, 255, 255, 255));
                rect.Width = 0;
                rect.Height = 0;
                rect.Stroke = new SolidColorBrush(System.Windows.Media.Color.FromArgb(255, 255, 0, 0));
                rect.StrokeDashArray.Add(3.0);
                _ = canvas.Children.Add(rect);
                Canvas.SetLeft(rect, center.X);
                Canvas.SetTop(rect, center.Y);
                capture = rect;
            }
            else if (e.ChangedButton == MouseButton.Right)
            {
                EnumAllBox("show");
                hidden();
            }
        }

        private void DrawingRect(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                System.Windows.Point mouse = e.GetPosition(this);
                System.Windows.Point[] be = GetBeginEnd(mouse, center);
                capture.Width = Math.Abs(be[1].X - be[0].X);
                capture.Height = Math.Abs(be[1].Y - be[0].Y);
                Canvas.SetLeft(capture, be[0].X);
                Canvas.SetTop(capture, be[0].Y);
            }
        }

        private System.Windows.Point[] GetBeginEnd(System.Windows.Point p1, System.Windows.Point p2)
        {
            List<double> dx = new List<double> { p1.X, p2.X };
            List<double> dy = new List<double> { p1.Y, p2.Y };
            dx.Sort();
            dy.Sort();
            System.Windows.Point[] be = new System.Windows.Point[2];
            be[0] = new System.Windows.Point(dx[0], dy[0]);
            be[1] = new System.Windows.Point(dx[1], dy[1]);
            return be;
        }

        private void StopDrawRect(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                EnumAllBox("show");
                NewImgBox(GetBeginEnd(e.GetPosition(this), center)[0]);
                hidden();
            }
        }

        private void hidden()
        {
            canvas.Children.Clear();
            this.Hide();
        }

        List<View.ImgBox> hasOpen;

        private void EnumAllBox(string op)
        {
            List<View.ImgBox> unvalidItem = new List<View.ImgBox>();
            foreach (View.ImgBox ib in hasOpen)
            {
                try
                {
                    switch (op)
                    {
                        case "show": ib.Show(); break;
                        case "hide": ib.Hide(); break;
                        case "close": ib.Close(); break;
                    }
                }
                catch (InvalidOperationException)
                {
                    unvalidItem.Add(ib);
                }
            }
            foreach (View.ImgBox i in unvalidItem)
            {
                hasOpen.Remove(i);
            }
        }

        private void NewImgBox(System.Windows.Point begin)
        {

            Int32Rect region = new Int32Rect((int)begin.X, (int)begin.Y, (int)capture.Width, (int)capture.Height);
            View.ImgBox win = new View.ImgBox(
                new CroppedBitmap(img.Source as BitmapSource, region),
                capture.Width, capture.Height
            );
            win.Show();
            win.Top = begin.Y;
            win.Left = begin.X;
            hasOpen.Add(win);
        }

        private void Awake(object sender, System.Windows.Forms.KeyEventArgs e)
        {
            if(e.KeyCode == Keys.Q && System.Windows.Forms.Control.ModifierKeys == Keys.Control)
            {
                this.Show();
            }
           
        }

        private void Awake(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                this.Show();
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            notifyIcon.Dispose();
        }

        private void Window_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            Console.WriteLine(e.Key);
        }
    }
}
