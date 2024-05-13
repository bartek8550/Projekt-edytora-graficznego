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
    /// Logika interakcji dla klasy InPainting.xaml
    /// </summary>
    public partial class InPainting : Window
    {
        public int ind1 { get; set; }
        public int ind2 { get; set; }
        public ImageWindow obraz { get; set; }
        public ImageWindow maska { get; set; }
        public InPainting()
        {
            InitializeComponent();

        }

        public InPainting(List<ImageWindow> imageWindows)
        {
            InitializeComponent();
            foreach (ImageWindow imageWindow in imageWindows)
            {
                cb1.Items.Add(new ComboBoxItem(imageWindow.Title));
                cb2.Items.Add(new ComboBoxItem(imageWindow.Title));
            }
        }

        public class ComboBoxItem
        {
            public string Value { get; set; }
            public ComboBoxItem(string value)
            {
                Value = value;
            }
            public override string ToString() { return Value; }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            ind1 = cb1.SelectedIndex;
            ind2 = cb2.SelectedIndex;
            obraz = MainWindow.imageWindows[ind1];
            maska = MainWindow.imageWindows[ind2];
            if (obraz.MatImage.NumberOfChannels == 1)
            {
                this.DialogResult = true;
            }
            else
            {
                MessageBox.Show("Obraz musi być szarocieniowy");
            }


        }
    }
}
