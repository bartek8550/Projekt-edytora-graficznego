using Emgu.CV;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
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
    /// Logika interakcji dla klasy OperacjePunktoweDwuargumentowe.xaml
    /// </summary>
    public partial class OperacjePunktoweDwuargumentowe : Window
    {
        public int ind1 { get; set; }
        public int ind2 { get; set; }
        public int operacja { get; set; }
        public float ciezar1 { get; set; }
        public float ciezar2 { get; set; }

        public OperacjePunktoweDwuargumentowe()
        {
            InitializeComponent();
        }

        public OperacjePunktoweDwuargumentowe(List<ImageWindow> imageWindows)
        {
            InitializeComponent();
            foreach (var imageWindow in imageWindows)
            {

                if (imageWindow.MatImage.NumberOfChannels == 1)
                {
                    cb1.Items.Add(new ComboBoxItem(imageWindow.Title));
                    cb2.Items.Add(new ComboBoxItem(imageWindow.Title));
                }
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
            operacja = cb3.SelectedIndex;
            if (c1 != null && c2 != null) {
                ciezar1 = float.Parse(c1.Text);
                ciezar2 = float.Parse(c2.Text);
            }

            if (MainWindow.imageWindows[ind1].MatImage.Size == MainWindow.imageWindows[ind2].MatImage.Size)
            {
                this.DialogResult = true;
            }
            else {
                MessageBox.Show("Obrazy muszą mieć takie same rozmiary");
            }

            
        }

        private void cb3_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            
            if (cb3.SelectedIndex == 2)
            {
                c1.Visibility = Visibility.Visible;
                c2.Visibility = Visibility.Visible;
            }
            else
            {
                this.c1.Visibility = Visibility.Hidden;
                this.c2.Visibility = Visibility.Hidden;
            } 
        }
    }
}
