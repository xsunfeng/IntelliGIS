using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using System.ComponentModel;

using ESRI.ArcGIS.Controls;
using ESRI.ArcGIS.Carto;
namespace CAGA.Dialogue
{
    /// <summary>
    /// Interaction logic for ListMapLayerOptionsWindow.xaml
    /// </summary>
    public partial class ListMapLayerOptionsWindow : Window
    {
        private MapLayerOptionListData _optionListData;
        private List<MapLayerOptionItemData> _selectedItems;
        private AxMapControl _axMapCtrl;

        public List<MapLayerOptionItemData> SelectedItems
        {
            get { return _selectedItems; }
        }

        public ListMapLayerOptionsWindow(MapLayerOptionListData optionListData)
        {
            InitializeComponent();
            this._optionListData = optionListData;
            this.HeadingTB.Text = this._optionListData.Opening;
            this.OptionLB.ItemsSource = this._optionListData.Options;
            this._selectedItems = new List<MapLayerOptionItemData>();

            System.Windows.Forms.Integration.WindowsFormsHost host = new System.Windows.Forms.Integration.WindowsFormsHost();
            // Create an object of the map control.
            this._axMapCtrl = new AxMapControl();
            host.Child = this._axMapCtrl;
            this.mapGrid.Children.Add(host);

        }
        
        public void LoadMap(string mapFile)
        {
            if (mapFile != "")
            {
                if (this._axMapCtrl.CheckMxFile(mapFile))
                {
                    this._axMapCtrl.LoadMxFile(mapFile, Type.Missing, Type.Missing);
                    this._axMapCtrl.Enabled = true;                    
                }
            }
        }
        

        private void ListBoxItem_Checked(object sender, RoutedEventArgs e)
        {
            MapLayerOptionItemData checkedItem = ((ListBoxItem)(e.Source)).Content as MapLayerOptionItemData;
            this.Close();
        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            string checkedLayerName = ((CheckBox)e.Source).Content.ToString();
            foreach(MapLayerOptionItemData option in this._optionListData.Options)
            {
                if (option.LayerName == checkedLayerName)
                {
                    this._selectedItems.Add(option);
                    string workDir = System.IO.Path.GetDirectoryName(option.LayerFile);
                    string extension = System.IO.Path.GetExtension(option.LayerFile);
                    if (extension.ToLower() == ".shp")
                    {
                        this._axMapCtrl.AddShapeFile(workDir, System.IO.Path.GetFileName(option.LayerFile));
                    }
                    else if (extension.ToLower() == ".lyr")
                    {
                        this._axMapCtrl.AddLayerFromFile(option.LayerFile);
                    }
                }
            }
        }

        private void CheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            string checkedLayerName = ((CheckBox)e.Source).Content.ToString();
            foreach (MapLayerOptionItemData option in this._selectedItems.ToArray())
            {
                if (option.LayerName == checkedLayerName)
                {
                    this._selectedItems.Remove(option);
                }
            }
            //get the layers from the map
            IEnumLayer layers = this._axMapCtrl.Map.get_Layers();
            layers.Reset();

            ILayer layer = null;
            while ((layer = layers.Next()) != null)
            {
                if (layer.Name.ToLower() == checkedLayerName.ToLower())
                {
                    this._axMapCtrl.Map.DeleteLayer(layer); 
                }
            }

        }
    }

    public class MapLayerOptionListData
    {
        private ObservableCollection<MapLayerOptionItemData> _options;

        public MapLayerOptionListData()
        {
            this._options = new ObservableCollection<MapLayerOptionItemData>();
        }

        public string Opening { get; set; }
        public ObservableCollection<MapLayerOptionItemData> Options
        {
            get
            {
                return this._options;
            }
        }

        public void AddOption(MapLayerOptionItemData option)
        {
            this._options.Add(option);
        }

    }

    /// <summary>
    /// data class
    /// </summary>
    public class MapLayerOptionItemData : INotifyPropertyChanged
    {
        private string _layerName;
        private string _layerFile;
        
        /// <summary>
        /// Gets or sets the title.
        /// </summary>
        public string LayerName
        {
            get
            {
                return this._layerName;
            }
            set
            {
                this._layerName = value;
                NotifyPropertyChanged("LayerName");
            }
        }

        /// <summary>
        /// Gets or sets the description.
        /// </summary>
        public string LayerFile
        {
            get
            {
                return this._layerFile;
            }
            set
            {
                this._layerFile = value;
                NotifyPropertyChanged("LayerFile");
            }
        }

        /// <summary>
        /// ToString override, often used in bindings.
        /// </summary>
        /// <returns>title.</returns>
        public override string ToString()
        {
            return this.LayerName;
        }

        public MapLayerOptionItemData(string layerName, string layerFile)
        {
            this.LayerName = layerName;
            this.LayerFile = layerFile;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void NotifyPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
