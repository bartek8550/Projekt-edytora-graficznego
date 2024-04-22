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
    /// Logika interakcji dla klasy SelekcjaRoz.xaml
    /// </summary>
    public partial class SelekcjaRoz : Window
    {
        public int p1Value { get; set; }
        public int p2Value { get; set; }
        public int q3Value { get; set; }
        public int q4Value { get; set; }

        public SelekcjaRoz()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            p1Value = int.Parse(P1.Text);
            p2Value = int.Parse(P2.Text);
            q3Value = int.Parse(Q3.Text);
            q4Value = int.Parse(Q4.Text);
            this.DialogResult = true;

        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            bool isEnable = false; 
            if (int.TryParse(P1.Text, out int p1) && int.TryParse(P2.Text, out int p2) && int.TryParse(Q3.Text, out int q3) && int.TryParse(Q4.Text, out int q4)) {
                if ((p1 >= 0 && p1 <= 255) && (p2 >= 0 && p2 <= 255) && (q3 >= 0 && q3 <= 255) && (q4 >= 0 && q4 <= 255)) { 
                    isEnable = true;
                }
            }
            Zatwierdz.IsEnabled = isEnable;

        }
    }
}
