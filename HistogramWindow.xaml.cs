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
using System.Diagnostics.Metrics;
using System.Globalization;
using Emgu.CV;

namespace Projekt_edytora_graficznego
{
    /// <summary>
    /// Logika interakcji dla klasy HistogramWindow.xaml
    /// </summary>
    public partial class HistogramWindow : Window
    {
        public SeriesCollection SeriesCollection { get; set; }
        public HistogramWindow()
        {
            InitializeComponent();
            
        }

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
            var SeriesCollection = new SeriesCollection
            {
                columnSeries
            };
            Chart.Series = SeriesCollection;
        }

        public class HisData {
            public int wartosc { get; set; }
            public int iloscZliczen { get; set; }
            public string normalizacja { get; set; }
        }

        public void PrepareHistogramTable(int[] histogram, double[] normalizedHistogram) {
            var dataList = new List<HisData>();
            for (int i = 0; i < histogram.Length; ++i)
            {
                dataList.Add(new HisData {
                    wartosc = i, 
                    iloscZliczen = histogram[i], 
                    normalizacja = normalizedHistogram[i].ToString("F6", CultureInfo.InvariantCulture)
                });
            }

            histogramDataGrid.ItemsSource = dataList;
        }  
    }
}
