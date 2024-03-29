using Emgu.CV;
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
using System.Windows.Shapes;
using Emgu.CV;
using Emgu.CV.Structure;
using Emgu.CV.CvEnum;

namespace Projekt_edytora_graficznego
{
    /// <summary>
    /// Logika interakcji dla klasy ImageWindow.xaml
    /// </summary>
    public partial class ImageWindow : Window
    {
        public Mat MatImage { get; set; }
        
        public ImageWindow(Mat mat)
        {
            InitializeComponent();
            this.Activated += ImageWindow_Activated;
            MatImage = mat;
        }
        public void DisplayImage(BitmapSource image) 
        {
            Image.Source = image;
        }

        public void DisplayImage(Mat image)
        {
            BitmapSource aha = BitmapSourceExtension.ToBitmapSource(image);
            Image.Source = aha;
        }

        private void ImageWindow_Activated(object? sender, EventArgs e)
        {
            MainWindow.LastImage = this;
            MainWindow.ActualImage = (BitmapSource)Image.Source;
            
        }


    }
}
