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

                int[] histogram = new int[256];
                byte[] bytes = new byte[image.Rows * image.Cols * image.ElementSize];
                System.Runtime.InteropServices.Marshal.Copy(image.DataPointer, bytes, 0, bytes.Length);

                foreach (byte value in bytes)
                {
                    histogram[value]++;
                }

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
                int[] histogram = CalculateHistogram(LastImage.MatImage);
                HistogramWindow histogramWindow = new HistogramWindow();
                histogramWindow.PrepareHistogramTable(histogram);
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

        public int[] CalculateHistogram(Mat image) {
            int[] histogram = new int[256];
            byte[] bytes = new byte[image.Rows * image.Cols * image.ElementSize];
            System.Runtime.InteropServices.Marshal.Copy(image.DataPointer, bytes, 0, bytes.Length);

            foreach (byte value in bytes)
            {
                histogram[value]++;
            }
            return histogram;
        }

        #endregion



        public Mat BitmapToMat(Bitmap bitmap)
        {
            Mat imagemat = BitmapExtension.ToMat(bitmap);

            return imagemat;
        }

        private Bitmap BitmapSourceToBitmap(BitmapSource bitmapsource)
        {
            Bitmap bitmap;

            // Użycie MemoryStream do przechowania tymczasowych danych obrazu
            using (MemoryStream outStream = new MemoryStream())
            {
                // Stworzenie enkodera do konwersji BitmapSource na format BMP
                BitmapEncoder enc = new BmpBitmapEncoder();

                // Dodanie klatki obrazu do enkodera
                enc.Frames.Add(BitmapFrame.Create(bitmapsource));

                // Zapisanie skonwertowanego obrazu do MemoryStream
                enc.Save(outStream);

                // Stworzenie obiektu System.Drawing.Bitmap z zawartości MemoryStream
                // Pamiętaj, że strumień musi pozostać otwarty podczas tworzenia obiektu Bitmap
                bitmap = new Bitmap(outStream);
            }

            // Zwrócenie nowo utworzonego obiektu Bitmap
            return bitmap;
        }

        public static int GetNumberOfChannels(Bitmap bitmap)
        {
            // Sprawdzanie formatu pikseli bitmapy
            switch (bitmap.PixelFormat)
            {
                case PixelFormat.Format24bppRgb:
                    return 3; // RGB
                case PixelFormat.Format32bppRgb:
                case PixelFormat.Format32bppPArgb:
                case PixelFormat.Format32bppArgb:
                    return 4; // RGBA
                case PixelFormat.Format8bppIndexed:
                    return 1; // Skala szarości
                              // Możesz dodać więcej przypadków dla innych formatów, jeśli jest to potrzebne
                default:
                    // Domyślnie zwracana jest wartość 0, jeśli format nie jest obsługiwany
                    // Możesz również rzucić wyjątek lub obsłużyć ten przypadek inaczej w zależności od wymagań
                    return 0;
            }
        }

        
    }

}
