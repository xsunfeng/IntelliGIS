using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows.Forms;
using DotSpatial.Controls;
using DotSpatial.Data;
using DotSpatial.Projections;
namespace CAGA.Map
{
    class DSMapManager : MapManager, IGeoProcessor
    {
        private System.Windows.Controls.Panel _container;
        private DotSpatial.Controls.Map _dsMap;
    
        public DSMapManager(System.Windows.Controls.Panel mapContainer)
        {
            _container = mapContainer;
        }

        public System.Windows.Controls.Panel Container
        {
            get
            {
                return _container;
            }
            set
            {
                _container = value;
            }
        }

        public override void Initialize()
        {
            System.Windows.Forms.Integration.WindowsFormsHost host = new System.Windows.Forms.Integration.WindowsFormsHost();
            // Create an object of the map control.
            _dsMap = new DotSpatial.Controls.Map();
            _dsMap.Height = (int)_container.ActualHeight;
            _dsMap.Width = (int)_container.ActualWidth;
            host.Child = _dsMap;
            _container.Children.Add(host);
            _dsMap.ProjectionModeDefine = ActionMode.Always;
            //_dsMap.Projection = KnownCoordinateSystems.Projected.UtmWgs1984.;
            
        }

        public override void AddTOC(System.Windows.Controls.Panel legendContainer)
        {
            System.Windows.Forms.Integration.WindowsFormsHost host = new System.Windows.Forms.Integration.WindowsFormsHost();
            // Create an object of the legend control.
            DotSpatial.Controls.Legend legend = new DotSpatial.Controls.Legend();
            _dsMap.Legend = legend;
            legend.Height = (int)legendContainer.ActualHeight;
            legend.Width = (int)legendContainer.ActualWidth;
            host.Child = legend;
            legendContainer.Children.Add(host);
        }

        public override string GetMapFile()
        {
            throw new NotImplementedException();
        }

        public override System.Collections.Hashtable GetMapExtent()
        {
            throw new NotImplementedException();
        }


        public override bool LoadMap(string filePath)
        {
            throw new NotImplementedException();
        }

        public override bool SaveMap(string filePath)
        {
            throw new NotImplementedException();
        }

        public override bool AddLayer(string filePath)
        {
            foreach (IMapLayer lyr in _dsMap.GetFeatureLayers())
            {

                if (((IFeatureSet)(lyr.DataSet)).Filename.ToLower() == filePath.ToLower())
                {
                    _dsMap.Layers.Remove(lyr);
                    break;
                }
            }
            _dsMap.AddLayer(filePath);
            return true;
        }


        
        
        public override void RemoveLayer()
        {
            _dsMap.Layers.Remove(_dsMap.Layers.SelectedLayer);
        }

        

        public override void SetFunctionMode(MapFunctionMode mode)
        {
            if (mode == MapFunctionMode.Pan)
            {
                _dsMap.FunctionMode = FunctionMode.Pan;
            }
            else if (mode == MapFunctionMode.Info)
            {
               
                _dsMap.FunctionMode = FunctionMode.Info;
            }
            else if (mode == MapFunctionMode.Label)
            {

                _dsMap.FunctionMode = FunctionMode.Label;
            }
            else if (mode == MapFunctionMode.None)
            {

                _dsMap.FunctionMode = FunctionMode.None;
            }
            else if (mode == MapFunctionMode.SelectByRectangle)
            {

                _dsMap.FunctionMode = FunctionMode.Select;
            }
            else if (mode == MapFunctionMode.ZoomIn)
            {

                _dsMap.FunctionMode = FunctionMode.ZoomIn;
            }
            else if (mode == MapFunctionMode.ZoomOut)
            {
                _dsMap.FunctionMode = FunctionMode.ZoomOut;
            }
            else
            {
                _dsMap.FunctionMode = FunctionMode.None;
            }

        }

        public override void ZoomToMaxExtent()
        {
            _dsMap.ZoomToMaxExtent();
        }

        public override void ZoomToPrev()
        {
            _dsMap.MapFrame.ZoomToPrevious();
        }

        public override void ZoomToNext()
        {
            _dsMap.MapFrame.ZoomToNext();
        }

        public override void ZoomToLayer()
        {
            var layer = _dsMap.Layers.SelectedLayer;
            if (layer != null)
                _dsMap.ViewExtents = layer.DataSet.Extent;
        }

        public override void ClearMapSelection()
        {
            foreach (IMapLayer layer in _dsMap.MapFrame.GetAllLayers())
            {
                IMapFeatureLayer mapFeatureLayer = layer as IMapFeatureLayer;
                {
                    if (mapFeatureLayer != null)
                        mapFeatureLayer.UnSelectAll();
                }
            }
        }

        public override void SelectFeaturesByGraphics()
        {
            throw new NotImplementedException();
        }

        public override void ShowAttributeTable()
        {

            foreach (IMapLayer layer in _dsMap.MapFrame.GetAllLayers())
                {
                    IMapFeatureLayer fl = layer as IMapFeatureLayer;

                    if (fl == null) continue;
                    if (fl.IsSelected == false) continue;
                    fl.ShowAttributes();
                }
        }

        public override void Dispose()
        {
            _dsMap.Dispose();

        }

        public string Buffer(string inLayerName, string distString, string outLayerName="")
        {
            string inLayerFile = this._getLayerFileByName(inLayerName);

            string tempDir = System.IO.Path.GetTempPath();
            string outLayerFile = "";
            if (outLayerName == "")
            {
                outLayerFile = System.IO.Path.Combine(tempDir, inLayerName + "_buffer.shp");
            }
            else
            {
                outLayerFile = System.IO.Path.Combine(tempDir, outLayerName + ".shp");
            }

            IFeatureSet input = DataManager.DefaultDataManager.OpenVector(inLayerFile, true, null);
            FeatureSet output = new FeatureSet();
            int previous = 0;
            int maxNo = input.Features.Count;
            double distValue;
            if (double.TryParse(distString.Split(' ')[0], out distValue) && distValue > 0.0)
            {
                for (int i = 0; i < maxNo; i++)
                {
                    input.Features[i].Buffer(distValue, output);

                    // Here we update the progress
                    int current = Convert.ToInt32(i * 100 / maxNo);
                    if (current > previous)
                    {
                        previous = current;
                    }
                }

                output.SaveAs(outLayerFile, true);
                return outLayerFile;
            }
            else 
            {
                return "";
            }
        }


        private string _getLayerFileByName(string layer_name)
        {
            foreach (IMapLayer lyr in _dsMap.GetFeatureLayers())
            {

                if (lyr.DataSet.Name.ToLower() == layer_name.ToLower())
                {
                    return ((IFeatureSet)(lyr.DataSet)).Filename;
                }
            }
            return "";
        }

    }
}
