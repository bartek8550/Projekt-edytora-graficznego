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
    /// Logika interakcji dla klasy FiltracjaMedianowa.xaml
    /// </summary>
    public partial class FiltracjaMedianowa : Window
    {
        public double value1 { get; set; }
        public double value2 { get; set; }
        public double value3 { get; set; }
        public double value4 { get; set; }
        public double value5 { get; set; }
        public double value6 { get; set; }
        public double value7 { get; set; }
        public double value8 { get; set; }
        public double value9 { get; set; }
        public BorderType bt { get; set; }

        public FiltracjaMedianowa()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (double.TryParse(textBox1.Text, out double value1) &&
               double.TryParse(textBox2.Text, out double value2) &&
               double.TryParse(textBox3.Text, out double value3) &&
               double.TryParse(textBox4.Text, out double value4) &&
               double.TryParse(textBox5.Text, out double value5) &&
               double.TryParse(textBox6.Text, out double value6) &&
               double.TryParse(textBox7.Text, out double value7) &&
               double.TryParse(textBox8.Text, out double value8) &&
               double.TryParse(textBox9.Text, out double value9))
            {
                this.value1 = value1;
                this.value2 = value2;
                this.value3 = value3;
                this.value4 = value4;
                this.value5 = value5;
                this.value6 = value6;
                this.value7 = value7;
                this.value8 = value8;
                this.value9 = value9;

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
