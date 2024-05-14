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
using Emgu.CV.Reg;
using System.Runtime.InteropServices;

namespace Projekt_edytora_graficznego
{
    /// <summary>
    /// Logika interakcji dla klasy ImageWindow.xaml
    /// </summary>
    public partial class ImageWindow : Window
    {
        public Mat MatImage { get; set; }
        public Mat OriginalMatImage { get; set; }
        public HistogramWindow HistogramWindow { get; set; }
        private Point? start { get; set; }
        private Point? end { get; set; }
        public List<int> pointsValues { get; set; }
        public ImageWindow(Mat mat)
        {
            InitializeComponent();
            this.Activated += ImageWindow_Activated;
            this.Closing += ImageWindow_Closing;
            MatImage = mat;
            OriginalMatImage = mat.Clone();
            DisplayImage();
            start = null;
            end = null;
        }

        private void Image_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (this.start == null)
            {
                this.start = ScalePointToImage(e.GetPosition(Image));

            }
            else if ((this.start != null) && (this.end != null))
            {
                this.start = ScalePointToImage(e.GetPosition(Image));
                this.end = null;
            }
            else if (this.start != null)
            {
                this.end = ScalePointToImage(e.GetPosition(Image));
                DrawLineAndShowHistogram();

            }
            
        }

        private void DrawLineAndShowHistogram()
        {
            if (start.HasValue && end.HasValue)
            {
                MatImage = OriginalMatImage.Clone(); 
                Image<Bgr, byte> img = MatImage.ToImage<Bgr, byte>();

                LineSegment2D line = new LineSegment2D(new System.Drawing.Point((int)start.Value.X, (int)start.Value.Y), new System.Drawing.Point((int)end.Value.X, (int)end.Value.Y));
                img.Draw(line, new Bgr(System.Drawing.Color.Red), 2);
                MatImage = img.Mat;
                DisplayImage();
                List<Point> points = BresenhamLine((int)this.start.Value.X, (int)this.start.Value.Y, (int)this.end.Value.X, (int)this.end.Value.Y);
                pointsValues = new List<int>();
                foreach (Point point in points ) {
                    int x = (int)point.X;
                    int y = (int)point.Y;
                    if (x >= 0 && y >= 0 && x < MatImage.Width && y < MatImage.Height) {
                        IntPtr lStart = MatImage.DataPointer + y * MatImage.Step;
                        byte pixValue = Marshal.ReadByte(lStart, x);
                        pointsValues.Add(pixValue);
                    }
                }
                ShowProfileLine();
            }
        }

        public List<Point> BresenhamLine(int x0, int y0, int x1, int y1) {
            List<Point> points = new List<Point>();

            int dx = Math.Abs(x1 - x0);
            int dy = Math.Abs(y1 - y0);

            int x = x0;
            int y = y0;

            int sx = x0 > x1 ? -1 : 1;
            int sy = y0 > y1 ? -1 : 1;
            double err = 0;
            if (dx > dy)
            {
                err = dx / 2.0;
                while (x != x1)
                {
                    points.Add(new Point(x, y));
                    err -= dy;
                    if (err < 0)
                    {
                        y += sy;
                        err += dx;
                    }
                    x += sx;
                }
            }
            else {
                err = dy / 2.0;
                while (y != y1)
                {
                    points.Add(new Point(x, y));
                    err -= dx;
                    if (err < 0)
                    {
                        x += sx;
                        err += dy;
                    }
                    y += sy;
                }
            }
            points.Add(new Point(x, y));
            return points;
        }

        private Point ScalePointToImage(Point point)
        {
            if (Image.Source is BitmapSource bitmapSource)
            {
                var actualWidth = bitmapSource.PixelWidth;
                var actualHeight = bitmapSource.PixelHeight;
                var displayedWidth = Image.ActualWidth;
                var displayedHeight = Image.ActualHeight;

                var scaleX = actualWidth / displayedWidth;
                var scaleY = actualHeight / displayedHeight;

                return new Point(point.X * scaleX, point.Y * scaleY);
            }

            return point;
        }

        public void UpdateImageAndHistogram(Mat image)
        {
            MatImage = image;
            DisplayImage();
            UpdateHis();
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

        public void ShowProfileLine()
        {
            HistogramWindow histogramWindow = new HistogramWindow();
            HistogramWindow = histogramWindow;
            histogramWindow.Closed += (s, e) => HistogramWindow = null;
            HistogramWindow.ProfileLine(pointsValues);
            HistogramWindow.Show();
        }

        public void UpdateHis() 
        {
            if (HistogramWindow == null) return;
            
            var (histogram, normalizedHistogram) = CalculateHistogram();
            HistogramWindow.PrepareChartData(histogram);
            HistogramWindow.PrepareHistogramTable(histogram, normalizedHistogram);
            
        }

        private void ImageWindow_Activated(object? sender, EventArgs e)
        {
            MainWindow.LastImage = this;
        }

        private void ImageWindow_Closing(object? sender, EventArgs e)
        {
            MainWindow.LastImage = null;
            if (HistogramWindow != null) {
                HistogramWindow.Close();
            }
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            Image.MouseDown += Image_MouseDown;
        }
    }
}
