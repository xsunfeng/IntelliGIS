using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace CAGA.Dialogue
{
    /// <summary>
    /// Interaction logic for StatResultsWindow.xaml
    /// </summary>
    public partial class StatResultsWindow : Window
    {
        private Hashtable _statResults;
        public StatResultsWindow(Hashtable statResults)
        {
            InitializeComponent();
            this._statResults = statResults;
            this.LayerNameTB.Text = this._statResults["layer_name"].ToString();
            this.FieldNameTB.Text = this._statResults["field_name"].ToString();
            this.StatisticsLB.ItemsSource = this._statResults["statistics"] as Hashtable;

            // Create source.
            BitmapImage bi = new BitmapImage();
            // BitmapImage.UriSource must be in a BeginInit/EndInit block.
            bi.BeginInit();
            bi.UriSource = new Uri(this._statResults["histogram"].ToString(), UriKind.RelativeOrAbsolute);
            bi.EndInit();
            // Set the image source.
            this.statGraph.Width = bi.PixelWidth;
            this.statGraph.Height = bi.PixelHeight;
            this.statGraph.Source = bi;
        }

        private void AddGraphToLayout_Click(object sender, RoutedEventArgs e)
        {
            double XMin = 0;
            double YMin = 0;
            double XMax = this.statGraph.Width;
            double YMax = this.statGraph.Height;
            ((MainWindow)this.Owner).MapManager.AddGraphToLayout(this._statResults["histogram"].ToString(), XMin, YMin, XMax, YMax);
        }
    }
}
