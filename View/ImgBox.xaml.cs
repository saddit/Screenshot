using Microsoft.Win32;
using System;
using System.ComponentModel;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace WpfApp.View
{

    enum Mode
    {
        Drag,
        Pen,
        Eraser,
    }

    /// <summary>
    /// ImgBox.xaml 的交互逻辑
    /// </summary>
    public partial class ImgBox : Window
    {
        private double scale;
        private Mode mode = Mode.Drag;
        private double rw;

        public ImgBox(ImageSource image, double width, double height)
        {
            InitializeComponent();
            width *= 1.1;
            height *= 1.1;
            this.Width = width;
            this.Height = height;
            this.MaxHeight = SystemParameters.PrimaryScreenHeight;

            img.Width = width;
            img.Height = height;

            scale = width / height;
            img.Source = image;

            rw = this.Bar.Width;
            this.Bar.Width = 15;
        }

        private void Mouse_Move(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                if (mode.Equals(Mode.Drag))
                {
                    this.DragMove();
                }
            }

        }

        private void ExitItem_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void CopyItem_Click(object sender, RoutedEventArgs e)
        {
            Clipboard.SetImage(img.Source as BitmapSource);
        }

        private void SaveItem_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "Image Files (*.png, *.bmp, *.jpg)|*.png;*.bmp;*.jpg | All Files | *.*";
            sfd.RestoreDirectory = true;//保存对话框是否记忆上次打开的目录
            if (sfd.ShowDialog().GetValueOrDefault())
            {
                var encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create((BitmapSource)img.Source));
                using (FileStream stream = new FileStream(sfd.FileName, FileMode.Create))
                {
                    encoder.Save(stream);
                }   
            }
        }

        private void img_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            img.Width = e.NewSize.Width;
            img.Height = e.NewSize.Height;
        }

        private void Grid_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            double w = this.Width;
            if (e.Delta > 0)
            {
                w *= 1.15;
            }
            else
            {
                w *= 0.85;
            }
            if (w / scale <= MaxHeight)
            {
                this.Width = w;
                this.Height = this.Width / scale;
            }
        }

        private void ClearItem_Click(object sender, RoutedEventArgs e)
        {
            this.Panel.Children.Clear();
        }
    }
}
