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
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Projekt_edytora_graficznego
{
    /// <summary>
    /// Logika interakcji dla klasy DetekcjaKrawedzi.xaml
    /// </summary>
    public partial class DetekcjaKrawedzi : Window
    {
        public string? SobelDirection { get; set; }
        public string? PrewittDirection { get; set; }
        public double? TresholdValue1 { get; set; }
        public double? TresholdValue2 { get; set; }
        public string? pointerGet { get; set; }
        public BorderType bt { get; set; }

        public DetekcjaKrawedzi(string pointer)
        {
            InitializeComponent();
            this.pointerGet = pointer;
            switch (pointer) {
                case "Sobel":
                    SobelType.Visibility = Visibility.Visible; 
                    break;
                case "Prewitt":
                    PrewittType.Visibility = Visibility.Visible;
                    break;
                case "Canny":
                    Treshold1.Visibility = Visibility.Visible;
                    Treshold2.Visibility = Visibility.Visible;
                    break;
                case "Laplacian":
                    break;
                default: throw new ArgumentException("Coś nie działa ;)");
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (this.pointerGet == "Canny") {
                if (!double.TryParse(Treshold1.Text, out _) || !double.TryParse(Treshold2.Text, out _)) {
                    if (double.Parse(Treshold1.Text) < 0 || double.Parse(Treshold1.Text) > 255 || double.Parse(Treshold2.Text) < 0 || double.Parse(Treshold2.Text) > 255) {
                        MessageBox.Show("Entered invalid numbers");
                        return;
                    }
                }
                this.TresholdValue1 = double.Parse(Treshold1.Text);
                this.TresholdValue2 = double.Parse(Treshold2.Text);

            }
            this.SobelDirection = SobelType.SelectedIndex switch
            {
                0 => "X",
                1 => "Y"
            };

            this.PrewittDirection = PrewittType.SelectedIndex switch
            {
                0 => "N",
                1 => "NE",
                2 => "E",
                3 => "SE",
                4 => "S",
                5 => "SW",
                6 => "W",
                7 => "NW"

            };

            this.bt = BorderTypeVal.SelectedIndex switch
            {
                0 => BorderType.Isolated,
                1 => BorderType.Reflect,
                2 => BorderType.Replicate

            };

            this.DialogResult = true;
        }

    }
}
