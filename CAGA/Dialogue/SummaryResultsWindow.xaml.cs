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

using ESRI.ArcGIS.DataSourcesFile;
using ESRI.ArcGIS.Geodatabase;
using ArcDataBinding;
using CAGA.Map;
namespace CAGA.Dialogue
{
    /// <summary>
    /// Interaction logic for SummaryResultsWindow.xaml
    /// </summary>
    public partial class SummaryResultsWindow : Window
    {
        private Hashtable _sumResults;
        public SummaryResultsWindow(Hashtable sumResults)
        {
            InitializeComponent();
            this._sumResults = sumResults;
            
            ShapefileWorkspaceFactoryClass tempWSFactory = new ShapefileWorkspaceFactoryClass();
            IWorkspace tempWS = tempWSFactory.OpenFromFile(this._sumResults["workspace"].ToString(), 0);

            IFeatureWorkspace featureWS = tempWS as IFeatureWorkspace;
            
            // Bind dataset to the binding source
            TableWrapper tableWrapper = new ArcDataBinding.TableWrapper(featureWS.OpenTable(this._sumResults["table"].ToString()));
            this.SummaryTableGridView.DataSource = tableWrapper;
            
            // Create source.
            BitmapImage bi = new BitmapImage();
            // BitmapImage.UriSource must be in a BeginInit/EndInit block.
            bi.BeginInit();
            bi.UriSource = new Uri(this._sumResults["graph"].ToString(), UriKind.RelativeOrAbsolute);
            bi.EndInit();
            // Set the image source.
            this.SummaryGraph.Width = bi.PixelWidth;
            this.SummaryGraph.Height = bi.PixelHeight;
            this.SummaryGraph.Source = bi;
        }

        private void AddGraphToLayout_Click(object sender, RoutedEventArgs e)
        {
            double XMin = 0;
            double YMin = 0;
            double XMax = this.SummaryGraph.Width;
            double YMax = this.SummaryGraph.Height;
            ((MainWindow)this.Owner).MapManager.AddGraphToLayout(this._sumResults["graph"].ToString(), XMin, YMin, XMax, YMax);
        }
    }
}
