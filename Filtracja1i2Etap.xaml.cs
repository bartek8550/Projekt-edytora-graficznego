using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.ML;
using Emgu.CV.Structure;
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

namespace Projekt_edytora_graficznego
{
    /// <summary>
    /// Logika interakcji dla klasy Filtracja1i2Etap.xaml
    /// </summary>
    public partial class Filtracja1i2Etap : Window
    {
        public float tb301 { get; set; }
        public float tb302 { get; set; }
        public float tb303 { get; set; }
        public float tb304 { get; set; }
        public float tb305 { get; set; }
        public float tb306 { get; set; }
        public float tb307 { get; set; }
        public float tb308 { get; set; }
        public float tb309 { get; set; }

        public float tb311 { get; set; }
        public float tb312 { get; set; }
        public float tb313 { get; set; }
        public float tb314 { get; set; }
        public float tb315 { get; set; }
        public float tb316 { get; set; }
        public float tb317 { get; set; }
        public float tb318 { get; set; }
        public float tb319 { get; set; }

        public BorderType bt { get; set; }

        public int maskv { get; set; }

        public float sumv1 { get; set; }
        public float sumv2 { get; set; }
        public Matrix<float> resultv { get; set; }
        public Matrix<float> m1v { get; set; }
        public Matrix<float> m2v { get; set; }
        public Filtracja1i2Etap()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (float.TryParse(textBox11.Text, out float tb301) &&
                float.TryParse(textBox12.Text, out float tb302) &&
                float.TryParse(textBox13.Text, out float tb303) &&
                float.TryParse(textBox21.Text, out float tb304) &&
                float.TryParse(textBox22.Text, out float tb305) &&
                float.TryParse(textBox23.Text, out float tb306) &&
                float.TryParse(textBox31.Text, out float tb307) &&
                float.TryParse(textBox32.Text, out float tb308) &&
                float.TryParse(textBox33.Text, out float tb309) &&
                float.TryParse(textBox41.Text, out float tb311) &&
                float.TryParse(textBox42.Text, out float tb312) &&
                float.TryParse(textBox43.Text, out float tb313) &&
                float.TryParse(textBox51.Text, out float tb314) &&
                float.TryParse(textBox52.Text, out float tb315) &&
                float.TryParse(textBox53.Text, out float tb316) &&
                float.TryParse(textBox61.Text, out float tb317) &&
                float.TryParse(textBox62.Text, out float tb318) &&
                float.TryParse(textBox63.Text, out float tb319))
            {
                this.tb301 = tb301;
                this.tb302 = tb302;
                this.tb303 = tb303;
                this.tb304 = tb304;
                this.tb305 = tb305;
                this.tb306 = tb306;
                this.tb307 = tb307;
                this.tb308 = tb308;
                this.tb309 = tb309;
                this.tb311 = tb311;
                this.tb312 = tb312;
                this.tb313 = tb313;
                this.tb314 = tb314;
                this.tb315 = tb315;
                this.tb316 = tb316;
                this.tb317 = tb317;
                this.tb318 = tb318;
                this.tb319 = tb319;

                float sum1 = tb301 + tb302 + tb303 + tb304 + tb305 + tb306 + tb307 + tb308 + tb309;
                float sum2 = tb311 + tb312 + tb313 + tb314 + tb315 + tb316 + tb317 + tb318 + tb319;
                this.sumv1 = sum1;
                this.sumv2 = sum1;
                this.bt = BorderTypeVal.SelectedIndex switch
                {
                    0 => BorderType.Isolated,
                    1 => BorderType.Reflect,
                    2 => BorderType.Replicate
                };

                this.maskv = MaskVal.SelectedIndex switch
                {
                    0 => 0,
                    1 => 1
                };

                float[,] matrix1 = new float[,] { { tb301 / sumv1, tb302 / sumv1, tb303 / sumv1 }, { tb304 / sumv1, tb305 / sumv1, tb306 / sumv1 }, { tb307 / sumv1, tb308 / sumv1, tb309 / sumv1 } };
                float[,] matrix2 = new float[,] { { tb311 / sumv2, tb312 / sumv2, tb313 / sumv2 }, { tb314 / sumv2, tb315 / sumv2, tb316 / sumv2 }, { tb317 / sumv2, tb318 / sumv2, tb319 / sumv2 } };

                Matrix<float> m1 = new(matrix1);
                Matrix<float> m2 = new(matrix2);

                Matrix<float> m15 = new(new System.Drawing.Size(5, 5));
                Matrix<float> m25 = new(new System.Drawing.Size(5, 5));
                CvInvoke.CopyMakeBorder(m1, m15, 1, 1, 1, 1, BorderType.Constant, new MCvScalar(0.0, 0.0, 0.0, 0.0));
                CvInvoke.CopyMakeBorder(m2, m25, 1, 1, 1, 1, BorderType.Constant, new MCvScalar(0.0, 0.0, 0.0, 0.0));
                Matrix<float> result = new(new System.Drawing.Size(5, 5));

                CvInvoke.Filter2D(m15, result, m25, new System.Drawing.Point(-1, -1), 0, BorderType.Isolated);

                this.resultv = result;
                
                float[,] result1 = result.Data;
                
                textBox91.Text = result1[0, 0].ToString(); textBox92.Text = result1[0, 1].ToString(); textBox93.Text = result1[0, 2].ToString(); textBox94.Text = result1[0, 3].ToString(); textBox95.Text = result1[0, 4].ToString();
                textBox101.Text = result1[1, 0].ToString(); textBox102.Text = result1[1, 1].ToString(); textBox103.Text = result1[1, 2].ToString(); textBox104.Text = result1[1, 3].ToString(); textBox105.Text = result1[1, 4].ToString();
                textBox111.Text = result1[2, 0].ToString(); textBox112.Text = result1[2, 1].ToString(); textBox113.Text = result1[2, 2].ToString(); textBox114.Text = result1[2, 3].ToString(); textBox115.Text = result1[2, 4].ToString();
                textBox121.Text = result1[3, 0].ToString(); textBox122.Text = result1[3, 1].ToString(); textBox123.Text = result1[3, 2].ToString(); textBox124.Text = result1[3, 3].ToString(); textBox125.Text = result1[3, 4].ToString();
                textBox131.Text = result1[4, 0].ToString(); textBox132.Text = result1[4, 1].ToString(); textBox133.Text = result1[4, 2].ToString(); textBox134.Text = result1[4, 3].ToString(); textBox135.Text = result1[4, 4].ToString();


            }
            else
            {
                MessageBox.Show("Wprowadź poprawne liczby w pola maski.");
            }
        }
    }
}
