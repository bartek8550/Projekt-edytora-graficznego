using Microsoft.Win32;
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
using Emgu.CV;
using Emgu.CV.Structure;
using Emgu.CV.CvEnum;
using System.Drawing.Imaging;
using System.Drawing;
using PixelFormat = System.Drawing.Imaging.PixelFormat;
using Rectangle = System.Drawing.Rectangle;
using Point = System.Drawing.Point;
using System.IO;
using static System.Net.Mime.MediaTypeNames;

namespace Projekt_edytora_graficznego
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        public static ImageWindow? LastImage { get; set; }

        #region Clicki
        private void Szarocieniowe_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Multiselect = true;
            ofd.Filter = "Obraz (*.bmp, *.jpg, *.png)|*.bmp;*.jpg;*.png|Wszystkie pliki (*.*)|*.*";

            if (ofd.ShowDialog() == true)
            {
                string[] paths = ofd.FileNames;

                foreach (string path in paths)
                {
                    OpenGrayScale(path);
                }
            }
            else 
            {
                MessageBox.Show("Nie wybrano żadnego obrazu");
            }

        }

        private void Kolorowe_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Multiselect = true;
            ofd.Filter = "Obraz (*.bmp, *.jpg, *.png)|*.bmp;*.jpg;*.png|Wszystkie pliki (*.*)|*.*";

            if (ofd.ShowDialog() == true)
            {
                string[] paths = ofd.FileNames;

                foreach (string path in paths)
                {
                    OpenColorScale(path);
                }
            }
            else
            {
                MessageBox.Show("Nie wybrano żadnego obrazu");
            }
        }

        private void HistogramGraficzny_Click(object sender, RoutedEventArgs e)
        {
            if (LastImage.MatImage != null)
            {
                Mat image = LastImage.MatImage;

                if (image.NumberOfChannels != 1)
                {
                    MessageBox.Show("Obraz nie jest szarocieniowy");
                    return;
                }

                var (histogram, normalizedHistogram) = CalculateHistogram(image);
                HistogramWindow histogramWindow = new HistogramWindow();
                histogramWindow.PrepareChartData(histogram);
                histogramWindow.Show();

            }
            else {
                MessageBox.Show("Nie wybrano obrazka");
            }
        }

        private void HistogramTabela_Click(object sender, RoutedEventArgs e)
        {
            if (LastImage.MatImage != null)
            {
                var (histogram, normalizedHistogram) = CalculateHistogram(LastImage.MatImage);
                HistogramWindow histogramWindow = new HistogramWindow();
                histogramWindow.PrepareHistogramTable(histogram, normalizedHistogram);
                histogramWindow.Show();

            }
            else 
            {
                MessageBox.Show("Nie wybrano obrazka");
            }
        }

        private void RGBToGray_Click(object sender, RoutedEventArgs e)
        {
            if (LastImage.MatImage.NumberOfChannels == 1)
            {
                MessageBox.Show("Obraz jest w odcieniach szarości, możesz wykonać operacje jedynie dla obrazu kolorowego. Po co zmieniać obraz czarnobiały na czarnobiały ;)");
                return;
            }
            else 
            {
                Mat grayMat = LastImage.MatImage.ToImage<Gray, byte>().Mat;
                LastImage.MatImage = grayMat;
                LastImage.DisplayImage();
            }
        }

        private void RGBTo3xGray_Click(object sender, RoutedEventArgs e)
        {
            if (LastImage.MatImage.NumberOfChannels == 1)
            {
                MessageBox.Show("Obraz jest w odcieniach szarości, możesz wykonać operacje jedynie dla obrazu kolorowego. Po co zmieniać obraz czarnobiały na czarnobiały i to jeszcze 3 razy?!");
                return;
            }
            else
            {
                Mat image = LastImage.MatImage;

                //Dzielenie na kanały 
                Mat[] channels = image.Split();

                //Obiekty mat są w formacie Bgr (Blue, green, red)
                Mat channelBlue = channels[0];
                Mat channelGreen = channels[1];
                Mat channelRed = channels[2];

                // Tworzenie okien obrazów
                ImageWindow imageWindowBlue = new ImageWindow(channelBlue);
                ImageWindow imageWindowGreen = new ImageWindow(channelGreen);
                ImageWindow imageWindowRed = new ImageWindow(channelRed);

                imageWindowBlue.Title = "Kanał niebieski";
                imageWindowGreen.Title = "Kanał zielony";
                imageWindowRed.Title = "Kanał czerwony";

                // Wyświetlanie okien obrazów
                imageWindowBlue.Show();
                imageWindowGreen.Show();
                imageWindowRed.Show();
            }
        }

        private void RGBToHSV_Click(object sender, RoutedEventArgs e)
        {
            if (LastImage.MatImage != null)
            {
                Mat image = LastImage.MatImage;
                Mat imageHsv = new Mat();

                CvInvoke.CvtColor(image, imageHsv, ColorConversion.Bgr2Hsv);

                LastImage.MatImage = imageHsv;
                LastImage.DisplayImage();
            }
            else 
            {
                MessageBox.Show("Wybierz obraz");
            }

        }

        private void RGBToLab_Click(object sender, RoutedEventArgs e)
        {
            if (LastImage.MatImage != null)
            {
                Mat image = LastImage.MatImage;
                Mat imageLab = new Mat();

                CvInvoke.CvtColor(image, imageLab, ColorConversion.Bgr2Lab);

                LastImage.MatImage = imageLab;
                LastImage.DisplayImage();
            }
            else
            {
                MessageBox.Show("Wybierz obraz");
            }
        }

        private void HistogramRozciaganie_Click(object sender, RoutedEventArgs e)
        {
            Mat image = LastImage.MatImage; // Załóżmy, że to jest obraz szarocieniowy
            if (image.NumberOfChannels != 1)
            {
                MessageBox.Show("Operacja wymaga obrazu szarocieniowego.");
                return;
            }

            Image<Gray, byte> grayImage = image.ToImage<Gray, byte>();
            byte min = 255, max = 0;

            // Znajdowanie minimalnej i maksymalnej wartości intensywności
            for (int y = 0; y < grayImage.Height; y++)
            {
                for (int x = 0; x < grayImage.Width; x++)
                {
                    byte pixelValue = grayImage.Data[y, x, 0];
                    if (pixelValue < min) min = pixelValue;
                    if (pixelValue > max) max = pixelValue;
                }
            }

            // Sprawdzenie czy obraz już nie jest rozciągnięty
            if (min == max)
            {
                MessageBox.Show("Wszystkie piksele mają tę samą wartość.");
                return;
            }

            // Rozciąganie histogramu
            for (int y = 0; y < grayImage.Height; y++)
            {
                for (int x = 0; x < grayImage.Width; x++)
                {
                    grayImage.Data[y, x, 0] = (byte)((grayImage.Data[y, x, 0] - min) * (255 / (max - min)));
                }
            }

            // Konwersja z powrotem na Mat do wyświetlenia
            Mat stretchedImage = grayImage.Mat;
            LastImage.MatImage = stretchedImage;
            LastImage.DisplayImage();

        }

        private void HistogramEqualizacja_Click(object sender, RoutedEventArgs e)
        {

        }

        

        #endregion

        #region Metody
        private void OpenGrayScale(string path) 
        {
            Mat MatGray = ToGray(path);

            ImageWindow imageWindow = new ImageWindow(MatGray);
            imageWindow.Title = path;
            imageWindow.Show();
        }

        private void OpenColorScale(string path)
        {
            Mat MatColor = ToColor(path);

            ImageWindow imageWindow = new ImageWindow(MatColor);
            imageWindow.Title = path;
            imageWindow.Show();
        }

        public Mat ToGray(string path) 
        {
            Mat oryginal = CvInvoke.Imread(path,ImreadModes.Grayscale);
            return oryginal;
        }

        public Mat ToColor(string path)
        {
            Mat oryginal = CvInvoke.Imread(path, ImreadModes.Color);
            return oryginal;
        }

        public (int[], double[]) CalculateHistogram(Mat image)
        {
            int[] histogram = new int[256];
            double[] normalizedHistogram = new double[256];
            byte[] bytes = new byte[image.Rows * image.Cols];
            System.Runtime.InteropServices.Marshal.Copy(image.DataPointer, bytes, 0, bytes.Length);

            // Obliczanie histogramu
            foreach (byte value in bytes)
            {
                histogram[value]++;
            }

            int totalPixels = image.Rows * image.Cols;

            // Normalizacja histogramu względem maksymalnej liczby zliczeń
            for (int i = 0; i < histogram.Length; i++)
            {
                normalizedHistogram[i] = Math.Round((double)histogram[i] / totalPixels, 6); ;
            }

            return (histogram, normalizedHistogram);
        }

        public Mat BitmapToMat(Bitmap bitmap)
        {
            Mat imagemat = BitmapExtension.ToMat(bitmap);

            return imagemat;
        }


        #endregion

        
    }

}
