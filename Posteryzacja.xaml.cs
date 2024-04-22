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
    /// Logika interakcji dla klasy Posteryzacja.xaml
    /// </summary>
    public partial class Posteryzacja : Window
    {
        public int lvl { get; set; }
        public Posteryzacja()
        {
            InitializeComponent();
        }

        private void btn_Posteryzuj_Click(object sender, RoutedEventArgs e)
        {
            lvl = int.Parse(lvlOfSzarosc.Text);
            this.DialogResult = true;
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            bool isEnable = false;
            if (int.TryParse(lvlOfSzarosc.Text, out int szaroscOfLvl))
            {
                if ((szaroscOfLvl >= 2 && szaroscOfLvl <= 255))
                {
                    isEnable = true;
                }
            }
            btn_Posteryzuj.IsEnabled = isEnable;
        }
    }
}
