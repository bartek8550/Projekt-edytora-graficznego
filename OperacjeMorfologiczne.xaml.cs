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
    /// Logika interakcji dla klasy OperacjeMorfologiczne.xaml
    /// </summary>
    public partial class OperacjeMorfologiczne : Window
    {
        public BorderType bt { get; set; }
        public MorphOp morphOp { get; set; }
        public ElementShape elementShape { get; set; }
        public OperacjeMorfologiczne()
        {
            InitializeComponent();
        }

        private void acceptButton_Click(object sender, RoutedEventArgs e)
        {

            this.bt = borderTypeComboBox.SelectedIndex switch
            {
                0 => BorderType.Isolated,
                1 => BorderType.Reflect,
                2 => BorderType.Replicate

            };

            this.morphOp = operationComboBox.SelectedIndex switch
            {
                0 => MorphOp.Erode,
                1 => MorphOp.Dilate,
                2 => MorphOp.Open,
                3 => MorphOp.Close

            };

            this.elementShape = shapeComboBox.SelectedIndex switch
            {
                0 => ElementShape.Cross,
                1 => ElementShape.Rectangle
            };



            this.DialogResult = true;
        }
    }
}
