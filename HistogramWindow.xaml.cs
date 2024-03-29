using LiveCharts.Wpf.Charts.Base;
using LiveCharts.Wpf;
using LiveCharts;
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
    /// Logika interakcji dla klasy HistogramWindow.xaml
    /// </summary>
    public partial class HistogramWindow : Window
    {
        public HistogramWindow()
        {
            InitializeComponent();
            SeriesCollection = new SeriesCollection();
        }

        public SeriesCollection SeriesCollection { get; set; }

        public void PrepareChartData(int[] histogram)
        {
            var values = new ChartValues<int>();
            foreach (var value in histogram)
            {
                values.Add(value);
            }
            this.Chart.Background = new SolidColorBrush(Colors.Black);
            var columnSeries = new ColumnSeries
            {
                ColumnPadding = 0,
                Values = values,
                Fill = new SolidColorBrush(Colors.White),
                
            };

            SeriesCollection.Add(columnSeries);
            Chart.Series = SeriesCollection;
        }
    }
}
