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
using System.Windows.Forms;
using System.Drawing;
using System.Windows.Interop;
using System.Threading;

namespace WpfApp
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        private NotifyIcon notifyIcon;

        public MainWindow()
        {
            InitializeComponent();
            InitNotifyIcon();

            // 去边框
            this.WindowStyle = WindowStyle.None;
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

            quitMenuItem.Click += new EventHandler(quitMenuItem_Click);
            hideAllItem.Click += new EventHandler((o, e) => EnumAllBox("hide"));
            showAllItem.Click += new EventHandler((o, e) => EnumAllBox("show"));

            //将上面的自选项加入到parentMenuitem中。
            System.Windows.Forms.MenuItem[] parentMenuitem = new System.Windows.Forms.MenuItem[] { quitMenuItem, hideAllItem, showAllItem };
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

        private System.Windows.Point begin;
        private System.Windows.Shapes.Rectangle capture;

        private void StartDrawRect(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                begin = e.GetPosition(this);
                System.Windows.Shapes.Rectangle rect = new System.Windows.Shapes.Rectangle();
                rect.Fill = new SolidColorBrush(System.Windows.Media.Color.FromArgb(0, 255, 255, 255));
                rect.Width = 0;
                rect.Height = 0;
                rect.Stroke = new SolidColorBrush(System.Windows.Media.Color.FromArgb(255, 255, 0, 0));
                rect.StrokeDashArray.Add(3.0);
                _ = canvas.Children.Add(rect);
                Canvas.SetLeft(rect, begin.X);
                Canvas.SetTop(rect, begin.Y);
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
                System.Windows.Point end = e.GetPosition(this);
                capture.Width = Math.Abs(end.X - begin.X);
                capture.Height = Math.Abs(end.Y - begin.Y);
            }
        }

        private void StopDrawRect(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                System.Windows.Point end = e.GetPosition(this);
                List<double> dx = new List<double> { begin.X, end.X };
                List<double> dy = new List<double> { begin.Y, end.Y };
                dx.Sort();
                dy.Sort();
                begin.X = dx[0];
                begin.Y = dy[0];
                Canvas.SetLeft(capture, begin.X);
                Canvas.SetTop(capture, begin.Y);
                EnumAllBox("show");
                NewImgBox();
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
            List<int> unvalidIndex = new List<int>();
            for (int i = 0; i<hasOpen.Count; i++)
            {
                View.ImgBox ib = hasOpen[i];
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
                    unvalidIndex.Add(i);
                }
            }
            foreach (int i in unvalidIndex)
            {
                hasOpen.RemoveAt(i);
            }
        }

        private void NewImgBox()
        {
            Int32Rect region = new Int32Rect((int)begin.X, (int)begin.Y, (int)capture.Width, (int)capture.Height);
            View.ImgBox win = new WpfApp.View.ImgBox(
                new CroppedBitmap(img.Source as BitmapSource, region),
                capture.Width, capture.Height
            );
            hasOpen.Add(win);
            win.Show();
            win.Top = begin.Y;
            win.Left = begin.X;
        }

        private void Awake(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                this.Show();
            }  
        }
    }
}
