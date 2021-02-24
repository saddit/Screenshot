using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace WpfApp.View
{
    /// <summary>
    /// ImgBox.xaml 的交互逻辑
    /// </summary>
    public partial class ImgBox : Window
    {
        private double scale;

        public ImgBox(ImageSource image, double width, double height)
        {
            InitializeComponent();
            this.Width = width;
            this.Height = height;
            img.Width = width;
            img.Height = height;
            scale = width / height;
            img.Source = image;
        }

        private void WindowDrag(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
                this.DragMove();
        }

        private void ExitItem_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void img_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            img.Width = e.NewSize.Width;
            img.Height = e.NewSize.Height;
        }

    }
}
