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
using static Projekt_edytora_graficznego.HistogramWindow;
using System.Runtime.CompilerServices;

namespace Projekt_edytora_graficznego
{
    /// <summary>
    /// Logika interakcji dla klasy ImageWindow.xaml
    /// </summary>
    public partial class ImageWindow : Window
    {
        public Mat MatImage { get; set; }
        public HistogramWindow HistogramWindow { get; set; }
        public ImageWindow(Mat mat)
        {
            InitializeComponent();
            this.Activated += ImageWindow_Activated;
            this.Closing += ImageWindow_Closing;
            MatImage = mat;
            DisplayImage();
        }

        public void UpdateImageAndHistogram(Mat image)
        {
            MatImage = image;
            DisplayImage();
            ShowHis();
        }

        public void DisplayImage()
        {
            BitmapSource image = BitmapSourceExtension.ToBitmapSource(MatImage);
            Image.Source = image;
        }

        public (int[], double[]) CalculateHistogram()
        {
            int[] histogram = new int[256];
            double[] normalizedHistogram = new double[256];
            byte[] bytes = new byte[MatImage.Rows * MatImage.Cols];
            System.Runtime.InteropServices.Marshal.Copy(MatImage.DataPointer, bytes, 0, bytes.Length);

            // Obliczanie histogramu
            foreach (byte value in bytes)
            {
                histogram[value]++;
            }

            int totalPixels = MatImage.Rows * MatImage.Cols;

            // Normalizacja histogramu względem maksymalnej liczby zliczeń
            for (int i = 0; i < histogram.Length; i++)
            {
                normalizedHistogram[i] = Math.Round((double)histogram[i] / totalPixels, 6); ;
            }

            return (histogram, normalizedHistogram);
        }

        public void ShowHis() 
        {
            HistogramWindow histogramWindow = new HistogramWindow();
            HistogramWindow = histogramWindow;
            histogramWindow.Closed += (s, e) => HistogramWindow = null;  
            var (histogram, normalizedHistogram) = CalculateHistogram();
            HistogramWindow.PrepareChartData(histogram);
            HistogramWindow.PrepareHistogramTable(histogram, normalizedHistogram);
            HistogramWindow.Show();
        }

        private void ImageWindow_Activated(object? sender, EventArgs e)
        {
            MainWindow.LastImage = this;
        }

        private void ImageWindow_Closing(object? sender, EventArgs e)
        {
            MainWindow.LastImage = null;
            HistogramWindow.Close();
        }



    }
}
