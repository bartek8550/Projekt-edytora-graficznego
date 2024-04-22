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
using Emgu.CV.Reg;

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

        #region lab1
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

        private void Histogram_Click(object sender, RoutedEventArgs e)
        {
            if (LastImage.MatImage != null)
            {
                Mat image = LastImage.MatImage;

                if (image.NumberOfChannels != 1)
                {
                    MessageBox.Show("Obraz nie jest szarocieniowy");
                    return;
                }
                LastImage.ShowHis();
            }
            else {
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
            Mat image = LastImage.MatImage;
            
            if (image.NumberOfChannels != 1)
            {
                MessageBox.Show("Operacja wymaga obrazu szarocieniowego.");
                return;
            }

            Image<Gray, byte> grayImage = image.ToImage<Gray, byte>();
            byte Lmin = 0, Lmax = 255, min = 255, max = 0;

            // Znajdowanie minimalnej i maksymalnej wartości intensywności
            for (int y = 0; y < grayImage.Height; ++y)
            {
                for (int x = 0; x < grayImage.Width; ++x)
                {
                    byte pixel = grayImage.Data[y, x, 0];
                    if(pixel < min) min = pixel;
                    if(pixel > max) max = pixel;
                }
            }

            // Wykonywanie rozciągania histogramu
            for (int y = 0; y < grayImage.Height; ++y) {
                for (int x = 0; x < grayImage.Width; ++x){
                    byte pixel = grayImage.Data[y, x, 0];
                    if (pixel < min)
                    {
                        grayImage.Data[y, x, 0] = Lmin;
                    }
                    else if (pixel > max)
                    {
                        grayImage.Data[y, x, 0] = Lmax;
                    }
                    else
                    {
                        grayImage.Data[y, x, 0] = (byte)(((pixel - min) * Lmax) / (max - min));
                    }
                }
            }
                
            Mat stretchedImage = grayImage.Mat;          
            LastImage.UpdateImageAndHistogram(stretchedImage);
        }

        private void HistogramEqualizacja_Click(object sender, RoutedEventArgs e)
        {
            Mat image = LastImage.MatImage;

            var (histogram, normalizedHistogram) = LastImage.CalculateHistogram();
            int pixels = image.Width * image.Height;
            int sumask = 0;

            float skala = 255.0f / pixels;

            byte[] tab = new byte[256];
            for (int i = 0; i < 256; ++i) {
                sumask += histogram[i];
                tab[i] = (byte) (sumask * skala);
            }

            Image<Gray, byte> image2 = image.ToImage<Gray, byte>();
            for (int y = 0; y < image2.Height; ++y)
            {
                for (int x = 0; x < image2.Width; ++x)
                {
                    byte pix = image2.Data[y, x, 0];
                    image2.Data[y, x, 0] = tab[pix];
                }
            }
            Mat equaliImage = image2.Mat;
            LastImage.UpdateImageAndHistogram(equaliImage);

        }

        private void Negacja_Click(object sender, RoutedEventArgs e) 
        {
            Mat image = LastImage.MatImage;
            if (image.NumberOfChannels != 1)
            {
                MessageBox.Show("Operacja wymaga obrazu szarocieniowego.");
                return;
            }

            Image<Gray, byte> image2 = image.ToImage<Gray, byte>();

            for (int y = 0; y < image2.Height; ++y)
            {
                for (int x = 0; x < image2.Width; ++x)
                {
                    byte pix = image2.Data[y, x, 0];
                    image2.Data[y, x, 0] = (byte)(255 - pix);
                }
            }

            Mat negatedImage = image2.Mat;
            LastImage.UpdateImageAndHistogram(negatedImage);
        }

        private void HistogramRozciaganieZakresu_Click(object sender, RoutedEventArgs e)
        {
            Mat image = LastImage.MatImage;

            if (image.NumberOfChannels != 1)
            {
                MessageBox.Show("Operacja wymaga obrazu szarocieniowego.");
                return;
            }

            SelekcjaRoz selekcjaRoz = new SelekcjaRoz();
            if(selekcjaRoz.ShowDialog() == true) {
                Mat rozciagnieteSel = RozciaganieSel(selekcjaRoz.p1Value, selekcjaRoz.p2Value, selekcjaRoz.q3Value, selekcjaRoz.q4Value);
                LastImage.UpdateImageAndHistogram(rozciagnieteSel);
            }
        }
        #endregion lab1

        #region lab2
        private void Posteryzuj_Click(object sender, RoutedEventArgs e)
        {
            Mat image = LastImage.MatImage;

            if (image.NumberOfChannels != 1)
            {
                MessageBox.Show("Operacja wymaga obrazu szarocieniowego.");
                return;
            }

            Posteryzacja pos = new Posteryzacja();
            if (pos.ShowDialog() == true)
            {
                Mat post = Posteryzuj(image, pos.lvl);
                LastImage.UpdateImageAndHistogram(post);
            }
        }



        #endregion lab2


        #endregion Clicki

        #region Metody

        #region lab1
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

        public Mat BitmapToMat(Bitmap bitmap)
        {
            Mat imagemat = BitmapExtension.ToMat(bitmap);

            return imagemat;
        }

        public Mat RozciaganieSel(int p1, int p2, int q3, int q4) 
        {
            Mat image = LastImage.MatImage;
            Image<Gray, byte> image2 = image.ToImage<Gray, byte>();
            byte minV = 255;
            byte maxV = 0;
            
            for (int y = 0; y < image2.Height; ++y) {
                for (int x = 0; x < image2.Width; ++x) {
                    byte pix = image2.Data[y, x, 0];
                    if (pix >= p1 && pix <= p2) {
                        if(pix < minV) minV = pix;
                        if(pix > maxV) maxV = pix;
                    }
                }
            }

            for (int y = 0; y < image2.Height; ++y)
            {
                for (int x = 0; x < image2.Width; ++x)
                {
                    double pix = image2.Data[y, x, 0];
                    if (pix >= p1 && pix <= p2)
                    {
                        byte newPix = (byte)Math.Round(((pix - minV) / (maxV - minV)) * (q4 - q3) + q3);
                        image2.Data[y, x, 0] = newPix;
                    }
                }
            }

            Mat rozciagnietyMat = image2.Mat;
            return rozciagnietyMat;            
        }
        #endregion lab1

        #region lab2
        public Mat Posteryzuj(Mat mat, int poziomySzarosci) 
        {
            Mat image = mat;

            Image<Gray, byte> image2 = image.ToImage<Gray, byte>();
            float step = 255f / (poziomySzarosci - 1);
            
            for (int y = 0; y < image2.Height; ++y)
            {
                for (int x = 0; x < image2.Width; ++x)
                {
                    byte pix = image2.Data[y, x, 0];
                    byte posteryzePix = (byte)((Math.Round(pix/step)) * step);
                    image2.Data[y, x, 0] = posteryzePix;
                }
            }
            return image2.Mat;
        }

        #endregion lab2

        #endregion

    }

}
