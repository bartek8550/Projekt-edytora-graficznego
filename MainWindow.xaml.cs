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
using static System.Formats.Asn1.AsnWriter;
using Emgu.CV.Dnn;
using System.Runtime.CompilerServices;

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
        public static List<ImageWindow> imageWindows = new List<ImageWindow>();
        public int i = 1;
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

        private void Blur_Click(object sender, RoutedEventArgs e)
        {
            Mat image = LastImage.MatImage;

            if (image.NumberOfChannels != 1)
            {
                MessageBox.Show("Operacja wymaga obrazu szarocieniowego.");
                return;
            }

            MenuItem? menuItem = sender as MenuItem;
            if (menuItem != null) { 
                string? cp = menuItem.CommandParameter as string;
                BorderType bt = cp switch
                {
                    "isolated" => BorderType.Isolated,
                    "reflect" => BorderType.Reflect,
                    "replicate" => BorderType.Replicate,
                    _ => BorderType.Isolated
                };

                Mat mat = new Mat();
                CvInvoke.Blur(image, mat, new System.Drawing.Size(3, 3), new Point(-1, -1), bt);
                LastImage.UpdateImageAndHistogram(mat);
            }
        }

        private void gaussianBlur_Click(object sender, RoutedEventArgs e)
        {
            Mat image = LastImage.MatImage;

            if (image.NumberOfChannels != 1)
            {
                MessageBox.Show("Operacja wymaga obrazu szarocieniowego.");
                return;
            }

            MenuItem? menuItem = sender as MenuItem;
            if (menuItem != null)
            {
                string? cp = menuItem.CommandParameter as string;
                BorderType bt = cp switch
                {
                    "isolated" => BorderType.Isolated,
                    "reflect" => BorderType.Reflect,
                    "replicate" => BorderType.Replicate,
                    _ => BorderType.Isolated
                };

                Mat mat = new Mat();
                CvInvoke.GaussianBlur(image, mat, new System.Drawing.Size(3,3), 1.5, 0, bt);
                LastImage.UpdateImageAndHistogram(mat);
            }

        }

        private void Sobel_Click(object sender, RoutedEventArgs e) {
            Mat image = LastImage.MatImage;
            if (image.NumberOfChannels != 1)
            {
                MessageBox.Show("Operacja wymaga obrazu szarocieniowego.");
                return;
            }

            DetekcjaKrawedzi det = new DetekcjaKrawedzi("Sobel");

            if (det.ShowDialog() == true) {
                Mat mat1 = new Mat(new System.Drawing.Size(image.Width, image.Height), DepthType.Cv64F, 1);
                if (det.SobelDirection == "X")
                {
                    CvInvoke.Sobel(image, mat1, DepthType.Cv64F, 1, 0, 3, 1, 0, det.bt);
                }
                else {
                    CvInvoke.Sobel(image, mat1, DepthType.Cv64F, 0, 1, 3, 1, 0, det.bt);  
                }
                Mat mat2 = new Mat();
                CvInvoke.ConvertScaleAbs(mat1, mat2, 1.0, 0);
                LastImage.UpdateImageAndHistogram(mat2);
            }  
        }

        private void Laplacian_Click(object sender, RoutedEventArgs e)
        {
            Mat image = LastImage.MatImage;
            if (image.NumberOfChannels != 1)
            {
                MessageBox.Show("Operacja wymaga obrazu szarocieniowego.");
                return;
            }

            DetekcjaKrawedzi det = new DetekcjaKrawedzi("Laplacian");

            if (det.ShowDialog() == true)
            {
                Mat mat = new Mat();
                CvInvoke.Laplacian(image, mat, DepthType.Cv64F, 1, 1, 0, det.bt);
                Mat mat1 = new Mat();
                CvInvoke.ConvertScaleAbs(mat, mat1, 1.0, 0);
                LastImage.UpdateImageAndHistogram(mat1);
            }
            else {
                MessageBox.Show("Wystąpił błąd");
            }

        }

        private void Canny_Click(object sender, RoutedEventArgs e)
        {
            Mat image = LastImage.MatImage;
            if (image.NumberOfChannels != 1)
            {
                MessageBox.Show("Operacja wymaga obrazu szarocieniowego.");
                return;
            }

            DetekcjaKrawedzi det = new DetekcjaKrawedzi("Canny");

            if (det.ShowDialog() == true)
            {
                Mat mat = new Mat();
                CvInvoke.CopyMakeBorder(image, mat, 1,1,1,1, det.bt);
                CvInvoke.Canny(mat, mat, (double)det.TresholdValue1, (double)det.TresholdValue2);
                Mat mat1 = new Mat();
                CvInvoke.ConvertScaleAbs(mat, mat1, 1.0, 0);
                LastImage.UpdateImageAndHistogram(mat1);
            }
            else
            {
                MessageBox.Show("Wystąpił błąd");
            }
        }

        private void Prewitt_Click(object sender, RoutedEventArgs e)
        {
            Mat image = LastImage.MatImage;
            if (image.NumberOfChannels != 1)
            {
                MessageBox.Show("Operacja wymaga obrazu szarocieniowego.");
                return;
            }

            DetekcjaKrawedzi det = new DetekcjaKrawedzi("Prewitt");
            if (det.ShowDialog() == true)
            {
                switch (det.PrewittDirection)
                {
                    case "N":
                        {
                            Matrix<double> matrix = new Matrix<double>(3, 3)
                            {
                                Data = new double[3, 3] {
                                { 1, 1, 1 },
                                { 0, 0, 0 },
                                { -1, -1, -1 } }
                            };
                            Image<Gray, byte> mat = image.ToImage<Gray, byte>();
                            CvInvoke.Filter2D(image, mat, matrix, new System.Drawing.Point(-1, -1), 0, det.bt);
                            LastImage.UpdateImageAndHistogram(mat.Mat);
                            break;
                        }
                    case "NE": 
                        {
                            Matrix<double> matrix = new Matrix<double>(3, 3)
                            {
                                Data = new double[3, 3] {
                                { 0, 1, 1 },
                                { -1, 0, 1 },
                                { -1, -1, 0 } }
                            };
                            Image<Gray, byte> mat = image.ToImage<Gray, byte>();
                            CvInvoke.Filter2D(image, mat, matrix, new System.Drawing.Point(-1, -1), 0, det.bt);
                            LastImage.UpdateImageAndHistogram(mat.Mat);
                            break;
                        }
                    case "E": 
                        {
                            Matrix<double> matrix = new Matrix<double>(3, 3)
                            {
                                Data = new double[3, 3] {
                                { -1, 0, 1 },
                                { -1, 0, 1 },
                                { -1, 0, 1 } }
                            };
                            Image<Gray, byte> mat = image.ToImage<Gray, byte>();
                            CvInvoke.Filter2D(image, mat, matrix, new System.Drawing.Point(-1, -1), 0, det.bt);
                            LastImage.UpdateImageAndHistogram(mat.Mat);
                            break;

                        }
                    case "SE":
                        {
                            Matrix<double> matrix = new Matrix<double>(3, 3)
                            {
                                Data = new double[3, 3] {
                                { -1, -1, 0 },
                                { -1, 0, 1 },
                                { 0, 1, 1 } }
                            };
                            Image<Gray, byte> mat = image.ToImage<Gray, byte>();
                            CvInvoke.Filter2D(image, mat, matrix, new System.Drawing.Point(-1, -1), 0, det.bt);
                            LastImage.UpdateImageAndHistogram(mat.Mat);
                            break;
                        }
                    case "S":
                        {
                            Matrix<double> matrix = new Matrix<double>(3, 3)
                            {
                                Data = new double[3, 3] {
                                { -1, -1, -1 },
                                { 0, 0, 0 },
                                { 1, 1, 1 } }
                            };
                            Image<Gray, byte> mat = image.ToImage<Gray, byte>();
                            CvInvoke.Filter2D(image, mat, matrix, new System.Drawing.Point(-1, -1), 0, det.bt);
                            LastImage.UpdateImageAndHistogram(mat.Mat);
                            break;

                        }
                    case "SW":
                        {
                            Matrix<double> matrix = new Matrix<double>(3, 3)
                            {
                                Data = new double[3, 3] {
                                { 0, -1, -1 },
                                { 1, 0, -1 },
                                { 1, 1, 0 } }
                            };
                            Image<Gray, byte> mat = image.ToImage<Gray, byte>();
                            CvInvoke.Filter2D(image, mat, matrix, new System.Drawing.Point(-1, -1), 0, det.bt);
                            LastImage.UpdateImageAndHistogram(mat.Mat);
                            break;
                        }
                    case "W":
                        {
                            Matrix<double> matrix = new Matrix<double>(3, 3)
                            {
                                Data = new double[3, 3] {
                                { 1, 0, -1 },
                                { 1, 0, -1 },
                                { 1, 0, -1 } }
                            };
                            Image<Gray, byte> mat = image.ToImage<Gray, byte>();
                            CvInvoke.Filter2D(image, mat, matrix, new System.Drawing.Point(-1, -1), 0, det.bt);
                            LastImage.UpdateImageAndHistogram(mat.Mat);
                            break;
                        }
                    case "NW":
                        {
                            Matrix<double> matrix = new Matrix<double>(3, 3)
                            {
                                Data = new double[3, 3] {
                                { 1, 1, 0 },
                                { 1, 0, -1 },
                                { 0, -1, -1 } }
                            };
                            Image<Gray, byte> mat = image.ToImage<Gray, byte>();
                            CvInvoke.Filter2D(image, mat, matrix, new System.Drawing.Point(-1, -1), 0, det.bt);
                            LastImage.UpdateImageAndHistogram(mat.Mat);
                            break;
                        }
                }
            }

        }
        //Laplacian z 3 maskami
        private void WyostrzanieLiniowe_Click(object sender, RoutedEventArgs e)
        {
            Mat image = LastImage.MatImage;
            if (image.NumberOfChannels != 1)
            {
                MessageBox.Show("Operacja wymaga obrazu szarocieniowego.");
                return;
            }

            DetekcjaKrawedzi det = new DetekcjaKrawedzi("Laplacian3Mask");
            if (det.ShowDialog() == true)
            {
                if (det.LaplacianMasks == "Maska 1")
                {
                    Matrix<double> matrix = new Matrix<double>(3, 3)
                    {
                        Data = new double[3, 3] {
                        { 0, -1, 0 },
                        { -1, 5, -1 },
                        { 0, -1, 0 } }
                    };
                    Image<Gray, byte> mat = image.ToImage<Gray, byte>();
                    CvInvoke.Filter2D(image, mat, matrix, new System.Drawing.Point(-1, -1), 0, det.bt);
                    LastImage.UpdateImageAndHistogram(mat.Mat);
                }
                else if (det.LaplacianMasks == "Maska 2")
                {
                    Matrix<double> matrix = new Matrix<double>(3, 3)
                    {
                        Data = new double[3, 3] {
                        { -1, -1, -1 },
                        { -1, 9, -1 },
                        { -1, -1, -1 } }
                    };
                    Image<Gray, byte> mat = image.ToImage<Gray, byte>();
                    CvInvoke.Filter2D(image, mat, matrix, new System.Drawing.Point(-1, -1), 0, det.bt);
                    LastImage.UpdateImageAndHistogram(mat.Mat);
                }
                else if (det.LaplacianMasks == "Maska 3")
                {
                    Matrix<double> matrix = new Matrix<double>(3, 3)
                    {
                        Data = new double[3, 3] {
                        { 1, -2, 1 },
                        { -2, 5, -2 },
                        { 1, -2, 1 } }
                    };
                    Image<Gray, byte> mat = image.ToImage<Gray, byte>();
                    CvInvoke.Filter2D(image, mat, matrix, new System.Drawing.Point(-1, -1), 0, det.bt);
                    LastImage.UpdateImageAndHistogram(mat.Mat);
                }
            }
        }

        private void OperacjaLiniowaSasiedzctwa_Click(object sender, RoutedEventArgs e)
        {
            Mat image = LastImage.MatImage;
            if (image.NumberOfChannels != 1)
            {
                MessageBox.Show("Operacja wymaga obrazu szarocieniowego.");
                return;
            }
            OperacjaLiniowa ol = new OperacjaLiniowa();
            if (ol.ShowDialog() == true) {
                Matrix<double> matrix = new Matrix<double>(3, 3)
                {
                    Data = new double[3, 3] {
                        { ol.value1, ol.value2, ol.value3 },
                        { ol.value4, ol.value5, ol.value6 },
                        { ol.value7, ol.value8, ol.value9 } }
                };
                Image<Gray, byte> mat = image.ToImage<Gray, byte>();
                CvInvoke.Filter2D(image, mat, matrix, new System.Drawing.Point(-1, -1), 0, ol.bt);
                LastImage.UpdateImageAndHistogram(mat.Mat);
            }

        }
        private void FiltracjaMedianowa_Click(object sender, RoutedEventArgs e) 
        {
            Mat image = LastImage.MatImage;
            if (image.NumberOfChannels != 1)
            {
                MessageBox.Show("Operacja wymaga obrazu szarocieniowego.");
                return;
            }
            MenuItem? menuItem = sender as MenuItem;
            if (menuItem != null)
            {
                string? cp = menuItem.CommandParameter as string;
                string[] tab = cp.Split(' ');
                if (tab[0] == "3x3")
                {
                    BorderType bt = tab[1] switch
                    {
                        "isolated" => BorderType.Isolated,
                        "reflect" => BorderType.Reflect,
                        "replicate" => BorderType.Replicate,
                        _ => BorderType.Isolated
                    };
                    Mat mat = new Mat();
                    CvInvoke.CopyMakeBorder(image, mat, 1, 1, 1, 1, bt);
                    CvInvoke.MedianBlur(mat, mat, 3);
                    LastImage.UpdateImageAndHistogram(mat);
                }
                else if (tab[0] == "5x5")
                {
                    BorderType bt = tab[1] switch
                    {
                        "isolated" => BorderType.Isolated,
                        "reflect" => BorderType.Reflect,
                        "replicate" => BorderType.Replicate,
                        _ => BorderType.Isolated
                    };
                    Mat mat = new Mat();
                    CvInvoke.CopyMakeBorder(image, mat, 1, 1, 1, 1, bt);
                    CvInvoke.MedianBlur(mat, mat, 5);
                    LastImage.UpdateImageAndHistogram(mat);
                }
                else if (tab[0] == "7x7") {
                    BorderType bt = tab[1] switch
                    {
                        "isolated" => BorderType.Isolated,
                        "reflect" => BorderType.Reflect,
                        "replicate" => BorderType.Replicate,
                        _ => BorderType.Isolated
                    };
                    Mat mat = new Mat();
                    CvInvoke.CopyMakeBorder(image, mat, 1, 1, 1, 1, bt);
                    CvInvoke.MedianBlur(mat, mat, 7);
                    LastImage.UpdateImageAndHistogram(mat);
                }

                
            }

        }

        private void OperacjePunktoweDwuargumentowe_Click(object sender, RoutedEventArgs e)
        {
            Mat image = LastImage.MatImage;
            if (image.NumberOfChannels != 1)
            {
                MessageBox.Show("Operacja wymaga obrazu szarocieniowego.");
                return;
            }

            OperacjePunktoweDwuargumentowe opd = new OperacjePunktoweDwuargumentowe(imageWindows);
            if (opd.ShowDialog() == true)
            {
                Mat mat = new Mat();
                switch (opd.operacja) 
                {
                    case 0:                      
                        CvInvoke.Add(imageWindows[opd.ind1].MatImage, imageWindows[opd.ind2].MatImage, mat);
                        Otworz(mat);
                        break;
                    case 1:
                        CvInvoke.Subtract(imageWindows[opd.ind1].MatImage, imageWindows[opd.ind2].MatImage, mat);
                        Otworz(mat);
                        break;
                    case 2:
                        CvInvoke.AddWeighted(imageWindows[opd.ind1].MatImage, opd.ciezar1, imageWindows[opd.ind2].MatImage, opd.ciezar2, 0, mat, DepthType.Default);
                        Otworz(mat);
                        break;
                    case 3:
                        CvInvoke.BitwiseAnd(imageWindows[opd.ind1].MatImage, imageWindows[opd.ind2].MatImage, mat);
                        Otworz(mat);
                        break;
                    case 4:
                        CvInvoke.BitwiseOr(imageWindows[opd.ind1].MatImage, imageWindows[opd.ind2].MatImage, mat);
                        Otworz(mat);
                        break;
                    case 5:
                        CvInvoke.BitwiseNot(imageWindows[opd.ind1].MatImage, imageWindows[opd.ind2].MatImage, mat);
                        Otworz(mat);
                        break;
                    case 6:
                        CvInvoke.BitwiseXor(imageWindows[opd.ind1].MatImage, imageWindows[opd.ind2].MatImage, mat);
                        Otworz(mat);
                        break;
                }
                
            }
            else {
                MessageBox.Show("Coś poszło nie tak");
            }

        }

        private void Filtracja1i2Etap_Click(object sender, RoutedEventArgs e)
        {
            Mat image = LastImage.MatImage;
            if (image.NumberOfChannels != 1)
            {
                MessageBox.Show("Operacja wymaga obrazu szarocieniowego.");
                return;
            }

            Filtracja1i2Etap fe = new Filtracja1i2Etap();
            if (fe.ShowDialog() == true)
            {
                Matrix<double> matrix1 = new Matrix<double>(3, 3)
                {
                    Data = new double[3, 3] {
                        { fe.tb301, fe.tb302, fe.tb303 },
                        { fe.tb304, fe.tb305, fe.tb306 },
                        { fe.tb307, fe.tb308, fe.tb309 } }
                };

                Matrix<double> matrix2 = new Matrix<double>(3, 3)
                {
                    Data = new double[3, 3] {
                        { fe.tb311, fe.tb312, fe.tb313 },
                        { fe.tb314, fe.tb315, fe.tb316 },
                        { fe.tb317, fe.tb318, fe.tb319 } }
                };

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
            imageWindows.Add(imageWindow);
            imageWindow.Closing += (w , j) => imageWindows.Remove(imageWindow);
            imageWindow.Title = path;
            imageWindow.Show();
        }

        private void Otworz(Mat mat) 
        {
            
            ImageWindow imageWindow = new ImageWindow(mat);
            imageWindows.Add(imageWindow);
            imageWindow.Closing += (w, j) => imageWindows.Remove(imageWindow);
            imageWindow.Title = $"Obrazek {this.i}";
            this.i += this.i + this.i * this.i;
            imageWindow.Show();
        }

        private void OpenColorScale(string path)
        {
            Mat MatColor = ToColor(path);

            ImageWindow imageWindow = new ImageWindow(MatColor);
            imageWindows.Add(imageWindow);
            imageWindow.Closing += (w, j) => imageWindows.Remove(imageWindow);
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
