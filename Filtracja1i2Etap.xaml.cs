using Emgu.CV;
using Emgu.CV.CvEnum;
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
        public double tb301 { get; set; }
        public double tb302 { get; set; }
        public double tb303 { get; set; }
        public double tb304 { get; set; }
        public double tb305 { get; set; }
        public double tb306 { get; set; }
        public double tb307 { get; set; }
        public double tb308 { get; set; }
        public double tb309 { get; set; }

        public double tb311 { get; set; }
        public double tb312 { get; set; }
        public double tb313 { get; set; }
        public double tb314 { get; set; }
        public double tb315 { get; set; }
        public double tb316 { get; set; }
        public double tb317 { get; set; }
        public double tb318 { get; set; }
        public double tb319 { get; set; }

        public BorderType bt { get; set; }

        public Filtracja1i2Etap()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (double.TryParse(textBox11.Text, out double tb301) &&
                double.TryParse(textBox12.Text, out double tb302) &&
                double.TryParse(textBox13.Text, out double tb303) &&
                double.TryParse(textBox21.Text, out double tb304) &&
                double.TryParse(textBox22.Text, out double tb305) &&
                double.TryParse(textBox23.Text, out double tb306) &&
                double.TryParse(textBox31.Text, out double tb307) &&
                double.TryParse(textBox32.Text, out double tb308) &&
                double.TryParse(textBox33.Text, out double tb309) &&
                double.TryParse(textBox41.Text, out double tb311) &&
                double.TryParse(textBox42.Text, out double tb312) &&
                double.TryParse(textBox43.Text, out double tb313) &&
                double.TryParse(textBox51.Text, out double tb314) &&
                double.TryParse(textBox52.Text, out double tb315) &&
                double.TryParse(textBox53.Text, out double tb316) &&
                double.TryParse(textBox61.Text, out double tb317) &&
                double.TryParse(textBox62.Text, out double tb318) &&
                double.TryParse(textBox63.Text, out double tb319))
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

                this.bt = BorderTypeVal.SelectedIndex switch
                {
                    0 => BorderType.Isolated,
                    1 => BorderType.Reflect,
                    2 => BorderType.Replicate
                };


                this.DialogResult = true;
            }
            else
            {
                MessageBox.Show("Wprowadź poprawne liczby w pola maski.");
            }
        }
    }
}
