using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Security.Cryptography;
using System.IO;
using color_sort_for_Tecan;

namespace color_sorting
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        private WriteableBitmap wb = null;
        private ColorGridData cgd = null;

        public MainWindow()
        {
            InitializeComponent();
            this.cgd = new ColorGridData((Int32)this.image.Width, (Int32)this.image.Height);
            this.wb = new WriteableBitmap((Int32)this.image.Width, (Int32)this.image.Height, 92, 92, PixelFormats.Bgr32, null);
            this.image.Source = this.wb;
        }

        private void button_Random_color_Click(object sender, RoutedEventArgs e)
        {
            this.cgd.generate_random_color_grid();
            this.wb.Lock();
            this.wb.WritePixels(new Int32Rect(0, 0, (Int32)this.image.Width, (Int32)this.image.Height), this.cgd.random_data, this.wb.BackBufferStride, 0);
            this.wb.Unlock();
            this.image.Visibility = Visibility.Visible;
        }

        private void button_Color_Sorting_Click(object sender, RoutedEventArgs e)
        {
            this.cgd.color_sort_by_hue(ColorGridData.color_align.buttom);
            this.wb.Lock();
            this.wb.WritePixels(new Int32Rect(0, 0, (Int32)this.image.Width, (Int32)this.image.Height), this.cgd.sorted_data, this.wb.BackBufferStride, 0);
            this.wb.Unlock();

            this.image.Source = this.wb;
        }
    }
}
