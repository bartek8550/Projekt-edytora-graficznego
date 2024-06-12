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
using System.Security.Cryptography;
using Emgu.CV.Util;
using System.Numerics;


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
                    if (pixel < min) min = pixel;
                    if (pixel > max) max = pixel;
                }
            }

            // Wykonywanie rozciągania histogramu
            for (int y = 0; y < grayImage.Height; ++y)
            {
                for (int x = 0; x < grayImage.Width; ++x)
                {
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
            for (int i = 0; i < 256; ++i)
            {
                sumask += histogram[i];
                tab[i] = (byte)(sumask * skala);
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
            if (selekcjaRoz.ShowDialog() == true)
            {
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
                CvInvoke.GaussianBlur(image, mat, new System.Drawing.Size(3, 3), 1.5, 0, bt);
                LastImage.UpdateImageAndHistogram(mat);
            }

        }

        private void Sobel_Click(object sender, RoutedEventArgs e)
        {
            Mat image = LastImage.MatImage;
            if (image.NumberOfChannels != 1)
            {
                MessageBox.Show("Operacja wymaga obrazu szarocieniowego.");
                return;
            }

            DetekcjaKrawedzi det = new DetekcjaKrawedzi("Sobel");

            if (det.ShowDialog() == true)
            {
                Mat mat1 = new Mat(new System.Drawing.Size(image.Width, image.Height), DepthType.Cv64F, 1);
                if (det.SobelDirection == "X")
                {
                    CvInvoke.Sobel(image, mat1, DepthType.Cv64F, 1, 0, 3, 1, 0, det.bt);
                }
                else
                {
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
            else
            {
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
                CvInvoke.CopyMakeBorder(image, mat, 1, 1, 1, 1, det.bt);
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
            if (ol.ShowDialog() == true)
            {
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
                else if (tab[0] == "7x7")
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
            else
            {
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
                if (fe.maskv == 0)
                {
                    CvInvoke.Filter2D(image, image, fe.resultv, new System.Drawing.Point(-1, -1), 0, fe.bt);
                    LastImage.UpdateImageAndHistogram(image);
                }
                else
                {
                    CvInvoke.Filter2D(image, image, fe.m1v, new Point(-1, -1), 0, fe.bt);
                    CvInvoke.Filter2D(image, image, fe.m2v, new Point(-1, -1), 0, fe.bt);
                    LastImage.UpdateImageAndHistogram(image);
                }
            }

        }
        #endregion lab2

        #region lab3

        private void Operacje_morfologiczne_Click(object sender, RoutedEventArgs e)
        {
            Mat image = LastImage.MatImage;

            if (image.NumberOfChannels != 1)
            {
                MessageBox.Show("Operacja wymaga obrazu szarocieniowego.");
                return;
            }

            OperacjeMorfologiczne om = new OperacjeMorfologiczne();
            if (om.ShowDialog() == true)
            {
                Mat kernel = CvInvoke.GetStructuringElement(om.elementShape, new System.Drawing.Size(3, 3), new Point(-1, -1));
                CvInvoke.MorphologyEx(image, image, om.morphOp, kernel, new Point(-1, -1), 1, om.bt, new MCvScalar());
                LastImage.UpdateImageAndHistogram(image);
            }

        }

        private void Szkieletyzacja_Click(object sender, RoutedEventArgs e)
        {
            Mat image = LastImage.MatImage;

            if (image.NumberOfChannels != 1)
            {
                MessageBox.Show("Operacja wymaga obrazu szarocieniowego.");
                return;
            }

            Mat skel = new Mat(image.Size, DepthType.Cv8U, 1);
            Mat imCopy = image.Clone();

            Mat element = CvInvoke.GetStructuringElement(ElementShape.Cross, new System.Drawing.Size(3, 3), new System.Drawing.Point(-1, -1));

            while (true)
            {

                Mat imOpen = new Mat();
                CvInvoke.MorphologyEx(imCopy, imOpen, MorphOp.Open, element, new System.Drawing.Point(1, 1), 1, BorderType.Default, new MCvScalar());

                Mat imTemp = new Mat();
                CvInvoke.Subtract(imCopy, imOpen, imTemp);

                Mat imEroded = new Mat();
                CvInvoke.Erode(imCopy, imEroded, element, new System.Drawing.Point(1, 1), 1, BorderType.Default, new MCvScalar());

                CvInvoke.BitwiseOr(skel, imTemp, skel);

                imCopy = imEroded.Clone();

                if (CvInvoke.CountNonZero(imCopy) == 0)
                    break;

            }

            LastImage.UpdateImageAndHistogram(skel);
        }

        private void PiramidkowanieUp_Click(object sender, RoutedEventArgs e)
        {
            Mat image = LastImage.MatImage;

            if (image.NumberOfChannels != 1)
            {
                MessageBox.Show("Operacja wymaga obrazu szarocieniowego.");
                return;
            }

            CvInvoke.PyrUp(image, image);
            LastImage.UpdateImageAndHistogram(image);

        }

        private void PiramidkowanieDown_Click(object sender, RoutedEventArgs e)
        {
            Mat image = LastImage.MatImage;

            if (image.NumberOfChannels != 1)
            {
                MessageBox.Show("Operacja wymaga obrazu szarocieniowego.");
                return;
            }

            CvInvoke.PyrDown(image, image);
            LastImage.UpdateImageAndHistogram(image);
        }

        private void TransformataHugha_Click(object sender, RoutedEventArgs e)
        {
            Mat image = LastImage.MatImage;

            if (image.NumberOfChannels != 1)
            {
                MessageBox.Show("Operacja wymaga obrazu szarocieniowego.");
                return;
            }

            Mat edge = new Mat();

            // Wykrywanie krawędzi za pomocą operatora Canny
            CvInvoke.Canny(image, edge, 50, 100, 3, false);

            // Wykonywanie transformacji Hougha
            LineSegment2D[] lines = CvInvoke.HoughLinesP(edge, 1, Math.PI / 180, 20, 30, 10);

            // Rysowanie linii na obrazie
            foreach (LineSegment2D line in lines)
            {
                CvInvoke.Line(image, line.P1, line.P2, new Bgr(System.Drawing.Color.Red).MCvScalar, 2);
            }

            LastImage.UpdateImageAndHistogram(image);

        }



        #endregion lab3

        #region lab 4
        private void InPainting_Click(object sender, RoutedEventArgs e)
        {

            InPainting inPainting = new InPainting(imageWindows);
            if (inPainting.ShowDialog() == true)
            {
                Mat image = inPainting.obraz.MatImage;
                Mat mask = inPainting.maska.MatImage;
                CvInvoke.Inpaint(image, mask, image, 3, InpaintType.NS);
                LastImage.UpdateImageAndHistogram(image);
            }

        }


        #endregion lab4 

        #region Projekt wykładowy
        private void OtoczkaWypukla_Click(object sender, RoutedEventArgs e)
        {
            //Pobranie ostatnio klikniętego obrazka
            Mat image = LastImage.MatImage;

            //Zmienne potrzebne do zblurowania zdjecia i wykonania thresholdu
            Mat imageBlur = new Mat();
            Mat thresholdImage = new Mat();

            //Aplikowanie blura na obrazie szarocieniowym
            CvInvoke.GaussianBlur(image, imageBlur, new System.Drawing.Size(3, 3), 0);
            //Aplikowanie thresholda na zblurowanym obrazie w celu uzyskania binarnego obrazu
            CvInvoke.Threshold(imageBlur, thresholdImage, 200, 255, ThresholdType.Binary);

            //Lista do przechowania konturów oraz hierarchi
            using (VectorOfVectorOfPoint contours = new VectorOfVectorOfPoint())
            using (Mat hierarchy = new Mat())
            {
                //Znajdowanie konturów
                CvInvoke.FindContours(thresholdImage, contours, hierarchy, RetrType.Tree, ChainApproxMethod.ChainApproxSimple);

                //Lista do znajdywania otoczek wypukłych
                List<VectorOfPoint> hull = new List<VectorOfPoint>(contours.Size);

                //Pętla przechodząca po wektorach konturów
                for (int i = 0; i < contours.Size; i++)
                {
                    //Tworzenie tablicy wektorów dla pojedynczych wektórów na których następnie będzie wykonywana operacja
                    using (VectorOfPoint contour = contours[i])
                    {
                        //Tworzenie wektora dla punktów otoczki
                        VectorOfPoint hullPoints = new VectorOfPoint();

                        //Wykonanie metody ConvexHull dla danego wektora i wzrócenie w hullPoints otoczki 
                        CvInvoke.ConvexHull(contour, hullPoints, false);

                        //Dodanie otoczki do listy otoczek
                        hull.Add(hullPoints);
                    }
                }

                //Tworzenie pustego obrazu w celu rysowania po nim
                Mat drawing = new Mat(thresholdImage.Size, DepthType.Cv8U, 3);
                drawing.SetTo(new MCvScalar(0, 0, 0));

                // Rysowanie konturów i otoczek wypukłych
                for (int i = 0; i < contours.Size; i++)
                {
                    // Zielony kolor dla konturów
                    MCvScalar colorContours = new MCvScalar(0, 255, 0);
                    // Niebieski kolor dla otoczek wypukłych
                    MCvScalar colorHull = new MCvScalar(255, 0, 0);

                    // Rysowanie konturów
                    CvInvoke.DrawContours(drawing, contours, i, colorContours, 1, LineType.EightConnected, hierarchy, 0, new System.Drawing.Point());

                    // Rysowanie otoczek wypukłych
                    CvInvoke.DrawContours(drawing, new VectorOfVectorOfPoint(hull[i]), -1, colorHull, 1, LineType.EightConnected);
                }
                Otworz(drawing);

            }

        }
        private void OtoczkaWypukla2_Click(object sender, RoutedEventArgs e)
        {
            //Pobranie ostatnio klikniętego obrazka
            Mat image = LastImage.MatImage;

            //Zmienne potrzebne do zblurowania zdjecia i wykonania thresholdu
            Mat blur_image = new Mat();
            Mat threshold_output = new Mat();

            //Aplikowanie blura na obrazie szarocieniowym
            CvInvoke.GaussianBlur(image, blur_image, new System.Drawing.Size(3, 3), 0);
            //Aplikowanie thresholda na zblurowanym obrazie w celu uzyskania binarnego obrazu
            CvInvoke.Threshold(blur_image, threshold_output, 200, 255, ThresholdType.Binary);

            //Wektor do przechowania konturów i obiekt Mat do przechowania hierarchii
            VectorOfVectorOfPoint contours = new VectorOfVectorOfPoint();
            Mat hierarchy = new Mat();
            
            //Znajdowanie konturów
            CvInvoke.FindContours(threshold_output, contours, hierarchy, RetrType.Tree, ChainApproxMethod.ChainApproxSimple);

            VectorOfVectorOfPoint filteredContours = new VectorOfVectorOfPoint();
            for (int i = 0; i < contours.Size; i++)
            {
                using (VectorOfPoint contour = contours[i])
                {
                    if (contour.Size > 1)
                    {
                        filteredContours.Push(contour);
                    }
                }
            }

            //Lista do znajdywania otoczek wypukłych
            List<VectorOfPoint> hull = new List<VectorOfPoint>(contours.Size);

            for (int i = 0; i < filteredContours.Size; i++)
            {
                using (VectorOfPoint contour = filteredContours[i])
                {
                    List<Point> contourPoints = new List<Point>();
                    for (int j = 0; j < contour.Size; j++)
                    {
                        contourPoints.Add(contour[j]);
                    }

                    List<Point> hullPoints = ConvexHullAlgorithm.GrahamScan(contourPoints);
                    VectorOfPoint vectorHullPoints = new VectorOfPoint(hullPoints.ToArray());
                    hull.Add(vectorHullPoints);
                }
            }

            //Tworzenie pustego obrazu w celu rysowania po nim
            Mat drawing = new Mat(threshold_output.Size, DepthType.Cv8U, 3);
            drawing.SetTo(new MCvScalar(0, 0, 0));

            // Rysowanie konturów i otoczek wypukłych
            for (int i = 0; i < filteredContours.Size; i++)
            {
                // Zielony kolor dla konturów
                MCvScalar colorContours = new MCvScalar(0, 255, 0);
                // Niebieski kolor dla otoczek wypukłych
                MCvScalar colorHull = new MCvScalar(255, 0, 0);

                // Rysowanie konturów
                CvInvoke.DrawContours(drawing, filteredContours, i, colorContours, 1, LineType.EightConnected, hierarchy, 0, new System.Drawing.Point());

                // Rysowanie otoczek wypukłych
                CvInvoke.DrawContours(drawing, new VectorOfVectorOfPoint(hull[i]), -1, colorHull, 1, LineType.EightConnected);
            }
            Otworz(drawing);

        }
        /*
        private void OtoczkaWypukla3_Click(object sender, RoutedEventArgs e)
        {
            // Pobranie ostatnio klikniętego obrazka (już w odcieniach szarości)
            Mat image = LastImage.MatImage;

            // Threshold the image to binary
            Mat binary = new Mat();
            CvInvoke.Threshold(image, binary, 200, 255, ThresholdType.Binary);

            // Inicjalizacja czterech tablic Xi0 = A
            Mat[] Xi = new Mat[4];
            for (int i = 0; i < 4; i++)
            {
                Xi[i] = binary.Clone();
            }

            // Definicja elementów strukturalnych
            List<Emgu.CV.Matrix<double>> Bi = new List<Emgu.CV.Matrix<double>>();

            Emgu.CV.Matrix<double> B1 = new Emgu.CV.Matrix<double>(3, 3)
            {
                Data = new double[3, 3] {
                { 1, 0, 0 },
                { 1, 0, 0 },
                { 1, 0, 0 } }
            };

            Emgu.CV.Matrix<double> B2 = new Emgu.CV.Matrix<double>(3, 3)
            {
                Data = new double[3, 3] {
                { 1, 1, 1 },
                { 0, 0, 0 },
                { 0, 0, 0 } }
            };

            Emgu.CV.Matrix<double> B3 = new Emgu.CV.Matrix<double>(3, 3)
            {
                Data = new double[3, 3] {
                { 0, 0, 1 },
                { 0, 0, 1 },
                { 0, 0, 1 } }
            };

            Emgu.CV.Matrix<double> B4 = new Emgu.CV.Matrix<double>(3, 3)
            {
                Data = new double[3, 3] {
                { 0, 0, 0 },
                { 0, 0, 0 },
                { 1, 1, 1 } }
            };

            Bi.Add(B1);
            Bi.Add(B2);
            Bi.Add(B3);
            Bi.Add(B4);

            


            for (int i = 0; i < 4; i++) {
                Mat previousConvexHull = new Mat();
                Mat hm = new Mat();
                while (true) {
                    previousConvexHull = Xi[i].Clone();
                    //HitOrMiss 
                    CvInvoke.MorphologyEx(Xi[i], hm, MorphOp.HitMiss, Bi[i], new System.Drawing.Point(-1,-1), 1, BorderType.Constant, new MCvScalar(0));

                    Mat newConvexHull = new Mat();
                    CvInvoke.BitwiseOr(Xi[i], hm, newConvexHull);

                    Mat difference = new Mat();
                    CvInvoke.AbsDiff(previousConvexHull, Xi[i], difference);
                    if (CvInvoke.CountNonZero(difference) == 0)
                    {

                        break;
                    }
                }
                hm.Dispose();
            }

            // Unie wszystkich Xi, aby uzyskać ostateczny wynik
            Mat finalConvexHull = new Mat(binary.Size, binary.Depth, binary.NumberOfChannels);
            finalConvexHull.SetTo(new MCvScalar(0));
            foreach (Mat mat in Xi)
            {
                CvInvoke.BitwiseOr(finalConvexHull, mat, finalConvexHull);
            }

            // Wyświetlenie wyniku
            CvInvoke.Imshow("Convex Hull", finalConvexHull);


        }
        */



        #endregion

        private void Informacja_Click(object sender, RoutedEventArgs e)
        {
            Informacja inf = new Informacja();
            inf.Show();
        }

        private void Zapisz_Click(object sender, RoutedEventArgs e)
        {
            Mat image = LastImage.MatImage;

            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "Bitmap (*.bmp)|*.bmp|JPEG (*.jpg)|*.jpg|JPEG (*.jpeg)|*.jpeg|PNG (*.png)|*.png|Wszystkie pliki (*.*)|*.*";
            ImageFormat format = ImageFormat.Jpeg;
            if (sfd.ShowDialog() == true)
            {
                try
                {
                    Bitmap bitmap = image.ToBitmap();
                    format = sfd.FilterIndex switch
                    {
                        1 => ImageFormat.Bmp,
                        2 => ImageFormat.Jpeg,
                        3 => ImageFormat.Jpeg,
                        4 => ImageFormat.Png,
                        _ => throw new Exception("Nieobsługiwany format obrazu")
                    };

                    bitmap.Save(sfd.FileName, format);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Wystąpił błąd w czasie zapisywania obrazu");
                }
                
            }
            else
            {
                MessageBox.Show("Nie wybrano żadnego obrazu");
            }

        }
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
            this.i += this.i;
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

        #region lab4



        #endregion lab 4

        #endregion

        
    }

}
