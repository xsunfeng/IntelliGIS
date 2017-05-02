using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.Controls;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.CartoUI;
using ESRI.ArcGIS.Display;
using ESRI.ArcGIS.Geoprocessing;
using ESRI.ArcGIS.Geoprocessor;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.DataSourcesFile;


namespace CAGA.Map
{
    public class ArcMapManager : MapManager, IGeoProcessor
    {
        private System.Windows.Controls.Panel _mapContainer;
        private System.Windows.Controls.Panel _tocContainer;
        private System.Windows.Controls.Panel _layoutContainer;
        private AxMapControl _axMapCtrl;
        private AxTOCControl _axTOCCtrl;
        private AxPageLayoutControl _axLayoutCtrl;
        private MapAndLayoutSynchronizer _mapAndLayoutSync;
        private Geoprocessor _geoProcessor;
        private MapFunctionMode _mode;
        private SymbologyWindow _symWindow;
        private string _mapFile;
        private string _polygonName;
        private IWorkspace _tempWorkspace;
        private IMap _map = null;

        public event Action<string> PolygonDrawn;

        double minSymbolSize;
        double symbolSizeStep;

        public ArcMapManager(System.Windows.Controls.Panel mapContainer, System.Windows.Controls.Panel layoutContainer, System.Windows.Controls.Panel tocContainer)
        {
            this._mapContainer = mapContainer;
            this._layoutContainer = layoutContainer;
            this._tocContainer = tocContainer;
            this._mode = MapFunctionMode.None;
            this._geoProcessor = new Geoprocessor();
            this._geoProcessor.OverwriteOutput = true;
            this._symWindow = null;
            this._mapFile = "";
            this._polygonName = "";
            this._tempWorkspace = null;
            this._axLayoutCtrl = null;
            this._axTOCCtrl = null;
            this._axMapCtrl = null;
            this._mapAndLayoutSync = null;
            this._map = null;

            minSymbolSize = 12;
            symbolSizeStep = 4;
        }

        public System.Windows.Controls.Panel MapContainer
        {
            get
            {
                return _mapContainer;
            }
            set
            {
                _mapContainer = value;
            }
        }

        public override void Initialize()
        {
            System.Windows.Forms.Integration.WindowsFormsHost host;
            // Initialize the map control.
            host = new System.Windows.Forms.Integration.WindowsFormsHost();
            this._axMapCtrl = new AxMapControl();
            this._axMapCtrl.BeginInit();
            host.Child = this._axMapCtrl;
            this._mapContainer.Children.Add(host);
            this._axMapCtrl.OnMouseDown += new ESRI.ArcGIS.Controls.IMapControlEvents2_Ax_OnMouseDownEventHandler(this.axMapControl_OnMouseDown);
            this._axMapCtrl.EndInit();

            // Initialize the layout control
            host = new System.Windows.Forms.Integration.WindowsFormsHost();
            this._axLayoutCtrl = new AxPageLayoutControl();
            this._axLayoutCtrl.BeginInit();
            host.Child = this._axLayoutCtrl;
            this._layoutContainer.Children.Add(host);
            this._axLayoutCtrl.EndInit();

            // Initialize the toc control
            host = new System.Windows.Forms.Integration.WindowsFormsHost();
            // Create an object of the legend control.
            this._axTOCCtrl = new AxTOCControl();
            this._axTOCCtrl.BeginInit();
            host.Child = this._axTOCCtrl;
            this._tocContainer.Children.Add(host);
            this._axTOCCtrl.OnMouseDown += new ITOCControlEvents_Ax_OnMouseDownEventHandler(this._axTOCCtrl_OnMouseDown);
            this._axTOCCtrl.EndInit();

            //initialize the map and layout synchronization class
            //get a reference to the MapControl and the PageLayoutControl
            IMapControl3 mapCtrl = (IMapControl3)this._axMapCtrl.Object;
            IPageLayoutControl2 layoutCtrl = (IPageLayoutControl2)this._axLayoutCtrl.Object;
            this._mapAndLayoutSync = new MapAndLayoutSynchronizer(mapCtrl, layoutCtrl);
            //bind the controls together (both point at the same map) and set the MapControl as the active control
            this._mapAndLayoutSync.BindControls(true);
            //add the framework controls (TOC and Toolbars) in order to synchronize then when the
            //active control changes (call SetBuddyControl)
            this._mapAndLayoutSync.AddFrameworkControl(this._axTOCCtrl.Object);


            this._symWindow = new SymbologyWindow();
            this._symWindow.Initialize();

            ShapefileWorkspaceFactoryClass tempWSFactory = new ShapefileWorkspaceFactoryClass();
            string tempDir = System.IO.Path.GetTempPath();
            this._tempWorkspace = tempWSFactory.OpenFromFile(tempDir, 0);
                
            /*
            Type factoryType = Type.GetTypeFromProgID("esriDataSourcesGDB.AccessWorkspaceFactory");
            IWorkspaceFactory workspaceFactory = (IWorkspaceFactory)Activator.CreateInstance(factoryType);

            string tempWSPath = System.IO.Path.GetTempPath();
            string tempWSName = "ArcMapMgr_" + this.GetHashCode().ToString() + "_WS.mdb";

            string tempWSFile = System.IO.Path.Combine(tempWSPath, tempWSName);
            if (System.IO.File.Exists(tempWSFile))
            {
                System.IO.File.Delete(tempWSFile);
            }

            IWorkspaceName workspaceName = workspaceFactory.Create(tempWSPath, tempWSName, null, 0);
            // Cast the workspace name object to the IName interface and open the workspace.
            IName name = (IName)workspaceName;
            this._tempWorkspace = (IWorkspace)name.Open();        
            */
        }

        public override void AddTOC(System.Windows.Controls.Panel tocContainer)
        {
            this._tocContainer = tocContainer;
            System.Windows.Forms.Integration.WindowsFormsHost host = new System.Windows.Forms.Integration.WindowsFormsHost();
            // Create an object of the legend control.
            this._axTOCCtrl = new AxTOCControl();
            this._axTOCCtrl.BeginInit();
            host.Child = this._axTOCCtrl;
            this._tocContainer.Children.Add(host);
            this._axTOCCtrl.SetBuddyControl(this._axMapCtrl);
            this._axTOCCtrl.OnMouseDown += new ITOCControlEvents_Ax_OnMouseDownEventHandler(this._axTOCCtrl_OnMouseDown);
            this._axTOCCtrl.EndInit();
        }

        public void AddLayout(System.Windows.Controls.Panel layoutContainer)
        {
            this._layoutContainer = layoutContainer;
            System.Windows.Forms.Integration.WindowsFormsHost host = new System.Windows.Forms.Integration.WindowsFormsHost();
            // Create an object of the legend control.
            this._axLayoutCtrl = new AxPageLayoutControl();
            this._axLayoutCtrl.BeginInit();
            host.Child = this._axLayoutCtrl;
            this._layoutContainer.Children.Add(host);
            this._axLayoutCtrl.EndInit();
        }


        public override string GetMapFile()
        {
            return this._mapFile;
        }

        public override bool LoadMap(string filePath)
        {
            IMapDocument mapDoc = new MapDocumentClass();
            if (mapDoc.get_IsPresent(filePath) && !mapDoc.get_IsPasswordProtected(filePath))
            {
                mapDoc.Open(filePath, string.Empty);
                // set the first map as the active view
                IMap map = mapDoc.get_Map(0);
                this._map = map;
                mapDoc.SetActiveView((IActiveView)map);
                this._axLayoutCtrl.PageLayout = mapDoc.PageLayout;
                this._mapAndLayoutSync.ReplaceMap(map);
                mapDoc.Close();
                this._mapFile = filePath;
                return true;
            }
            else
            {
                return false;
            }

            /*
            if (this._axMapCtrl.CheckMxFile(filePath))
            {
                


                this._axMapCtrl.LoadMxFile(filePath, Type.Missing, Type.Missing);
                this._axMapCtrl.Enabled = true;
                this._mapFile = filePath;
                return true;
            }
            else
            {
                return false;
            }
            */ 
        }

        public void ActivateMapView(bool syncExtent = false)
        {
            this._mapAndLayoutSync.ActivateMap(syncExtent);
        }

        public void ActivateLayoutView(bool syncExtent = false)
        {
            this._mapAndLayoutSync.ActivatePageLayout(syncExtent);
        }

        public void AddGraphToLayout(string filePath, double XMin, double YMin, double XMax, double YMax)
        {
            BmpPictureElementClass bmpGraph = new BmpPictureElementClass();
            bmpGraph.ImportPictureFromFile(filePath);

            //this._axLayoutCtrl.ActiveView.ScreenDisplay.

            tagRECT screenRect = this._axLayoutCtrl.ActiveView.ScreenDisplay.DisplayTransformation.get_DeviceFrame();

            IEnvelope bounds = this._axLayoutCtrl.ActiveView.ScreenDisplay.DisplayTransformation.VisibleBounds;

            double ratio = bounds.Width / this._layoutContainer.ActualWidth;

            XMin = bounds.XMin + XMin * ratio;
            XMax = XMin + (XMax - XMin) * ratio;
            YMin = bounds.YMin + YMin * ratio;
            YMax = YMin + (YMax - YMin) * ratio;
            
            //IPoint minPt = this._axLayoutCtrl.ActiveView.ScreenDisplay.DisplayTransformation.ToMapPoint((int)XMin, (int)YMin);
            //IPoint maxPt = this._axLayoutCtrl.ActiveView.ScreenDisplay.DisplayTransformation.ToMapPoint((int)XMax, (int)YMax);
            IEnvelope envelope = new EnvelopeClass();
            envelope.PutCoords(XMin, YMin, XMax, YMax);
            IElement element = bmpGraph as IElement;
            element.Geometry = envelope;
            this._axLayoutCtrl.GraphicsContainer.AddElement(bmpGraph, 0);
        }
        
        public override bool SaveMap(string filePath)
        {
            IMapDocument mapDocument = new MapDocumentClass();
            if (System.IO.File.Exists(filePath))
            {
                System.IO.File.Delete(filePath);
            }
            mapDocument.New(filePath);
            mapDocument.ReplaceContents((IMxdContents)this._axMapCtrl.Map);
            mapDocument.Save(mapDocument.UsesRelativePaths, true);
            mapDocument.Close();
            
            return true;
        }

        public override bool AddLayer(string filePath, Int32 index = 10)
        {
            Console.WriteLine("add layer " + filePath);
            string workDir = System.IO.Path.GetDirectoryName(filePath);
            string extension = System.IO.Path.GetExtension(filePath);
            if (extension.ToLower() == ".shp")
            {
                this._axMapCtrl.AddShapeFile(workDir, System.IO.Path.GetFileName(filePath));
                ILayer layer = _map.get_Layer(0);
                _map.MoveLayer(layer, index);
                return true;
            }
            else if (extension.ToLower() == ".lyr")
            {
                this._axMapCtrl.AddLayerFromFile(filePath);
                ILayer layer = _map.get_Layer(0);
                _map.MoveLayer(layer, index);
                return true;
            }
            return false;
        }

        public void HideLayer(string layerName)
        {
            ILayer layer = this._getLayerByName(layerName);
            if (layer != null)
            {
                layer.Visible = false;
                //this._axMapCtrl.ActiveView.PartialRefresh(esriViewDrawPhase.esriViewAll, null, null);
            }
        }

        public override void RemoveLayer()
        {
            ILayer layer = this._getSelectedLayer();
            if (layer != null)
            {
                this._axMapCtrl.Map.DeleteLayer(layer);
            }
        }

        public void MoveLayer (string LayerName, int toIndex)
        {
            int index = GetIndexNumberFromLayerName(LayerName);
            ILayer layer = GetLayeFromName(LayerName);
            _map.MoveLayer(layer, toIndex);
        }

        public override System.Collections.Hashtable GetMapExtent()
        {
            Hashtable extent = new Hashtable();
            extent.Add("XMin", this._axMapCtrl.Extent.XMin);
            extent.Add("XMax", this._axMapCtrl.Extent.XMax);
            extent.Add("YMin", this._axMapCtrl.Extent.YMin);
            extent.Add("YMax", this._axMapCtrl.Extent.YMax);
            return extent;
        }

        public override void ZoomToMaxExtent()
        {
            this._axMapCtrl.Extent = this._axMapCtrl.FullExtent;
            //this._mapAndLayoutSync.SyncMapExtent(true);
        }

       
        public override void ZoomToPrev()
        {
            //Get the extent stack
            IExtentStack pMapExtent = (IExtentStack)this._axMapCtrl.ActiveView.ExtentStack;

            //Undo the extent view	
            if (pMapExtent.CanUndo())
            {
                pMapExtent.Undo();
                //this._mapAndLayoutSync.SyncMapExtent(true);
            }
        }

        public override void ZoomToNext()
        {
            //Get the extent stack
			IExtentStack pMapExtent = (IExtentStack) this._axMapCtrl.ActiveView.ExtentStack;

			//Undo the extent view	
			if (pMapExtent.CanRedo())
			{
				pMapExtent.Redo();
                //this._mapAndLayoutSync.SyncMapExtent(true);
			}
        }

        public override void ZoomToLayer()
        {
            ILayer layer = this._getSelectedLayer();
            if (layer != null)
            {
                this._axMapCtrl.Extent = layer.AreaOfInterest;
                //this._mapAndLayoutSync.SyncMapExtent(true);
            }
        }

        public override void ClearMapSelection()
        {
            this._axMapCtrl.ActiveView.PartialRefresh(esriViewDrawPhase.esriViewGeoSelection, null, null);
            this._axMapCtrl.Map.ClearSelection();
            this._axMapCtrl.ActiveView.PartialRefresh(esriViewDrawPhase.esriViewGeoSelection, null, null);
        }

        public override void SelectFeaturesByGraphics()
        {
            IGraphicsContainerSelect pGC = (IGraphicsContainerSelect)this._axMapCtrl.ActiveView.GraphicsContainer;

            // find first selected graphic object
            if (pGC.ElementSelectionCount > 0)
            {
                this._selectFeaturesByShape(pGC.SelectedElement(0).Geometry);
            }
        }

        public void SelectFeaturesByGraphics(string graphicsName)
        {
            IGraphicsContainer pGC = (IGraphicsContainer)this._axMapCtrl.ActiveView.GraphicsContainer;
            pGC.Reset();
            IElement element = null;
            while ((element = pGC.Next()) != null)
            {
                IElementProperties elementProp = element as IElementProperties;
                if (elementProp != null && elementProp.Name.ToLower() == graphicsName.ToLower())
                {
                    Console.WriteLine("OOOOO elementProp.Name.ToLower()=" + elementProp.Name.ToLower() + ",graphicsName.ToLower()=" + graphicsName.ToLower());
                    this.ClearMapSelection();
                    this._selectFeaturesByShape(element.Geometry);
                    return;
                }
            }
        }

        public void SelectFeaturesByAttributes(string layerName, string whereClause)
        {
            ILayer layer = this._getLayerByName(layerName);
            if (layer != null)
            {
                // Create the query filter.
                IQueryFilter queryFilter = new QueryFilterClass();
                // Set the filter to return only restaurants.
                queryFilter.WhereClause = whereClause;
                IFeatureSelection pFeatureSelection = layer as IFeatureSelection;
                pFeatureSelection.SelectFeatures(queryFilter, esriSelectionResultEnum.esriSelectionResultNew, false);
                this._axMapCtrl.ActiveView.PartialRefresh(esriViewDrawPhase.esriViewGeoSelection, null, null);
            }
        }

        public void SelectFeaturesByLocation(string in_layer_name, string select_features_name, string overlap_type = "INTERSECT", string selection_type = "NEW_SELECTION")
        {
            //create a new instance of a SelectByLocation tool
            ESRI.ArcGIS.DataManagementTools.SelectLayerByLocation SelectByLocation = new ESRI.ArcGIS.DataManagementTools.SelectLayerByLocation();
            SelectByLocation.in_layer = this._getLayerByName(in_layer_name) as IFeatureLayer2;
            SelectByLocation.select_features = this._getLayerByName(select_features_name) as IFeatureLayer2;
            SelectByLocation.overlap_type = overlap_type;
            SelectByLocation.selection_type = selection_type;
            //execute the geoprocessing tool
            try
            {
                IGeoProcessorResult results = (IGeoProcessorResult)this._geoProcessor.Execute(SelectByLocation, null);

                //RunTool(this._geoProcessor, SelectByLocation, null);
                if (results.Status == esriJobStatus.esriJobSucceeded)
                {
                    this._axMapCtrl.ActiveView.PartialRefresh(esriViewDrawPhase.esriViewGeoSelection, null, null);
                }
            }
            catch (Exception err)
            {
                Console.WriteLine(err.Message);
            }
            
        }

        private static void RunTool(Geoprocessor geoprocessor, IGPProcess process, ITrackCancel TC)
        {

            // Set the overwrite output option to true
            geoprocessor.OverwriteOutput = true;

            // Execute the tool            
            try
            {
                geoprocessor.Execute(process, null);
                ReturnMessages(geoprocessor);

            }
            catch (Exception err)
            {
                Console.WriteLine(err.Message);
                ReturnMessages(geoprocessor);
            }
        }

        // Function for returning the tool messages.
        private static void ReturnMessages(Geoprocessor gp)
        {
            if (gp.MessageCount > 0)
            {
                for (int Count = 0; Count <= gp.MessageCount - 1; Count++)
                {
                    Console.WriteLine(gp.GetMessage(Count));
                }
            }

        }

        public ArrayList GetLayerNames()
        {
            ArrayList layerNames = new ArrayList(); 
            //get the layers from the map
            IEnumLayer layers = this._axMapCtrl.Map.get_Layers();
            layers.Reset();

            ILayer layer = null;
            while ((layer = layers.Next()) != null)
            {
                layerNames.Add(layer.Name);
            }
            return layerNames;
        }

        public ArrayList GetFieldNames(string layerName)
        {
            ArrayList fieldNames = new ArrayList();
            ILayer layer = this._getLayerByName(layerName);
            if (layer != null)
            {
                // Get the Fields collection from the feature class.
                IFeatureLayer featureLayer = layer as IFeatureLayer;

                IFields fields = featureLayer.FeatureClass.Fields;
                IField field = null;

                // On a zero based index, iterate through the fields in the collection.
                for (int i = 0; i < fields.FieldCount; i++)
                {
                    // Get the field at the given index.
                    field = fields.get_Field(i);
                    fieldNames.Add(field.Name);
                }
            }
            return fieldNames;
        }

        public int GetTotalSelectedFeaturesInLayer(string layerName)
        {
            ILayer layer = this._getLayerByName(layerName);
            if (layer != null)
            {
                IFeatureSelection featureSelection = (IFeatureSelection)layer; // Explicit Cast
                return featureSelection.SelectionSet.Count;
            }
            else
            {
                return -1;
            }
        }

        public Hashtable GetFieldStatistics(string layerName, string fieldName, bool selectedOnly=false)
        {
            Hashtable result = new Hashtable();
            ILayer layer = this._getLayerByName(layerName);
            if (layer != null && layer is IFeatureLayer)
            {
                ICursor cursor;
                if (selectedOnly == true)
                {
                    ((IFeatureSelection)layer).SelectionSet.Search(null, false, out cursor);
                }
                else
                {
                    IFeatureLayer featureLayer = layer as IFeatureLayer;
                    cursor = (ICursor)featureLayer.FeatureClass.Search(null, false);
                }

                Console.WriteLine("数据统计");
                Console.WriteLine("layerName=" + layerName);
                Console.WriteLine("fieldName=" + fieldName);

                result.Add("layer_name", layerName);
                result.Add("field_name", fieldName);

                IDataStatistics dataStatistics = new DataStatisticsClass();
                dataStatistics.Cursor = cursor;
                dataStatistics.Field = fieldName;
                IStatisticsResults statResults = dataStatistics.Statistics;
                Hashtable stats = new Hashtable();
                stats.Add("count", statResults.Count);
                stats.Add("sum", statResults.Sum);
                stats.Add("mean", statResults.Mean);
                stats.Add("std", statResults.StandardDeviation);
                stats.Add("minimum", statResults.Minimum);
                stats.Add("maximum", statResults.Maximum);
                result.Add("statistics", stats);

                // Create a data graph.
                IDataGraphT dataGraphT = new DataGraphTClass();
                // Add the histogram series.
                ISeriesProperties seriesProps = dataGraphT.AddSeries("bar:histogram");
                seriesProps.SourceData = (IFeatureLayer)layer; //((IAttributeTable)layer).AttributeTable;
                seriesProps.SetField(0, fieldName);
                
                // Set the histogram properties.
                IHistogramSeriesProperties histogramSeriesProps = (IHistogramSeriesProperties)seriesProps;
                histogramSeriesProps.BinCount = 20;

                if (selectedOnly == true)
                {
                    dataGraphT.UseSelectedSet = true;
                    dataGraphT.HighlightSelection = false;
                    //seriesProps.SourceData = ((IFeatureSelection)layer).SelectionSet;
                }
                else
                {
                    dataGraphT.UseSelectedSet = true;
                    dataGraphT.HighlightSelection = true;
                    //seriesProps.SourceData = ((IFeatureLayer)layer).FeatureClass;
                }
                // Set titles.
                dataGraphT.GeneralProperties.Title = "Histogram of " + fieldName;
                dataGraphT.AxisProperties[1].Title = "COUNT";
                // Update the data graph.
                dataGraphT.Update(null);
                // Export the graph to file (the format depends on the file extension).
                string tempDir = System.IO.Path.GetTempPath();
                string outImageFile = System.IO.Path.Combine(tempDir, layerName+ "_" + fieldName + DateTime.Now.ToString("_MM_dd_yy_H_mm_ss_") + "_histogram.bmp");

                if (System.IO.File.Exists(outImageFile))
                {
                    System.IO.File.Delete(outImageFile);
                }

                dataGraphT.ExportToFile(outImageFile);

                result.Add("histogram", outImageFile);
            }
            return result;
        }

        public Hashtable GetDataSummary(string layerName, string summaryFields, string dissolveField, bool selectedOnly = false)
        {
            Hashtable result = new Hashtable();
            ILayer layer = this._getLayerByName(layerName);
            if (layer != null && layer is IFeatureLayer)
            {
                ITable inputTable = (ITable)layer;
                IWorkspaceName wsName =  ((IDataset)(this._tempWorkspace)).FullName as IWorkspaceName;

                /*
                ShapefileWorkspaceFactoryClass tempWSFactory = new ShapefileWorkspaceFactoryClass();
                IWorkspace tempWS = tempWSFactory.OpenFromFile(System.IO.Path.GetTempPath(), 0);
                IWorkspaceName wsName = ((IDataset)(tempWS)).FullName as IWorkspaceName;
                */
                TableNameClass outputName = new TableNameClass();
                outputName.Name = layerName + "_summary_" + DateTime.Now.ToString("MM_dd_yy_H_mm_ss");
                outputName.WorkspaceName = wsName;

                if (inputTable.FindField(dissolveField) > 0)
                {
                    BasicGeoprocessorClass basicGeopro = new BasicGeoprocessorClass();
                    ITable resultTable = basicGeopro.Dissolve(inputTable, selectedOnly, dissolveField, summaryFields, outputName);
                    
                    result.Add("workspace", this._tempWorkspace.PathName);
                    result.Add("table", outputName.Name);

                    // Create a data graph.
                    IDataGraphT dataGraphT = new DataGraphTClass();
                    // Add the graph series.
                    ISeriesProperties seriesProps = dataGraphT.AddSeries("bar:vertical");
                    seriesProps.SourceData = resultTable;

                    seriesProps.SetField(0, resultTable.Fields.get_Field(1).Name);
                    seriesProps.SetField(1, resultTable.Fields.get_Field(2).Name);
                    seriesProps.LabelField = resultTable.Fields.get_Field(1).Name;

                    // Set titles.
                    dataGraphT.GeneralProperties.Title = "Bar Chart";
                    dataGraphT.AxisProperties[1].Title = dissolveField;
                    
                    // Update the data graph.
                    dataGraphT.Update(null);
                    // Export the graph to file (the format depends on the file extension).
                    string tempDir = System.IO.Path.GetTempPath();
                    string outImageFile = System.IO.Path.Combine(tempDir, outputName.Name + ".bmp");

                    if (System.IO.File.Exists(outImageFile))
                    {
                        System.IO.File.Delete(outImageFile);
                    }

                    dataGraphT.ExportToFile(outImageFile);

                    result.Add("graph", outImageFile);

                }
                
                
            }
            return result;
        }

        public override void ShowAttributeTable()
        {
            ILayer layer = this._getSelectedLayer();
            if (layer is IAttributeTable)
            {
                IAttributeTable layerTable = (IAttributeTable)layer;
                AttrTableWindow window = new AttrTableWindow(layerTable.AttributeTable);
                window.Show();
                
            }

            
            //throw new NotImplementedException();
        }

        public override void Dispose()
        {
            ESRI.ArcGIS.ADF.COMSupport.AOUninitialize.Shutdown();
        }

        public override void SetFunctionMode(MapFunctionMode mode)
        {
            this._mode = mode;
            /*
            if (this._mode == MapFunctionMode.ZoomIn)
            {
                this._mapContainer.Cursor = System.Windows.Input.Cursors.Cross;
            }
            else if (this._mode == MapFunctionMode.Pan)
            {
                this._mapContainer.Cursor = System.Windows.Input.Cursors.Hand;
            }
            else
            {
                this._mapContainer.Cursor = System.Windows.Input.Cursors.Arrow;
            }
             */ 
        }

        public void DrawPolygon(string name="")
        {
            this._polygonName = name;
            this.SetFunctionMode(MapFunctionMode.DrawPolygon);
        }

        private void axMapControl_OnMouseDown(object sender, ESRI.ArcGIS.Controls.IMapControlEvents2_OnMouseDownEvent e)
        {
            //If left mouse button
            if (e.button == 1)
            {
                if (this._mode == MapFunctionMode.ZoomIn)
                {
                    this._zoomIn();
                }
                else if (this._mode == MapFunctionMode.ZoomOut)
                {
                    this._zoomOut();
                }
                else if (this._mode == MapFunctionMode.Pan)
                {
                    this._pan();
                }
                else if (this._mode == MapFunctionMode.SelectByRectangle)
                {
                    this._selectByRectangle();
                }
                else if (this._mode == MapFunctionMode.SelectByPolygon)
                {
                    this._selectByPolygon();
                }
                else if (this._mode == MapFunctionMode.SelectByCircle)
                {
                    this._selectByCircle();
                }
                else if (this._mode == MapFunctionMode.SelectByLine)
                {
                    this._selectByLine();
                }
                else if (this._mode == MapFunctionMode.DrawPolygon)
                {
                    this._drawPolygon();
                }
            }
        }

        private void _pan()
        {
            this._axMapCtrl.Pan();
        }

        private void _zoomIn()
        {
            this._axMapCtrl.Extent = this._axMapCtrl.TrackRectangle();
        }

        private void _zoomOut()
        {
            IEnvelope feedExt = this._axMapCtrl.TrackRectangle();
            double newWidth = this._axMapCtrl.Extent.Width * (this._axMapCtrl.Extent.Width / feedExt.Width);
            double newHeight = this._axMapCtrl.Extent.Height * (this._axMapCtrl.Extent.Height / feedExt.Height);

            //Set the new extent coordinates
            IEnvelope newExt = new EnvelopeClass();
            newExt.PutCoords(this._axMapCtrl.Extent.XMin - ((feedExt.XMin - this._axMapCtrl.Extent.XMin) * (this._axMapCtrl.Extent.Width / feedExt.Width)),
                this._axMapCtrl.Extent.YMin - ((feedExt.YMin - this._axMapCtrl.Extent.YMin) * (this._axMapCtrl.Extent.Height / feedExt.Height)),
                (this._axMapCtrl.Extent.XMin - ((feedExt.XMin - this._axMapCtrl.Extent.XMin) * (this._axMapCtrl.Extent.Width / feedExt.Width))) + newWidth,
                (this._axMapCtrl.Extent.YMin - ((feedExt.YMin - this._axMapCtrl.Extent.YMin) * (this._axMapCtrl.Extent.Height / feedExt.Height))) + newHeight);
            this._axMapCtrl.Extent = newExt;
        }

        private void _selectFeaturesByShape(object shape)
        {
            this._axMapCtrl.ActiveView.PartialRefresh(esriViewDrawPhase.esriViewGeoSelection, null, null);
            ISelectionEnvironment pSelectionEnvironment = new SelectionEnvironmentClass();
            pSelectionEnvironment.CombinationMethod =
                esriSelectionResultEnum.esriSelectionResultNew;
            this._axMapCtrl.Map.SelectByShape((IGeometry)shape, pSelectionEnvironment, false);
            this._axMapCtrl.ActiveView.PartialRefresh(esriViewDrawPhase.esriViewGeoSelection, null, null);
        }

        private void _selectByRectangle()
        {
            IGeometry geom = this._axMapCtrl.TrackRectangle();
            this._selectFeaturesByShape(geom);
        }

        private void _selectByPolygon()
        {
            IGeometry geom = this._axMapCtrl.TrackPolygon();
            this._selectFeaturesByShape(geom); 
        }

        private void _selectByCircle()
        {
            IGeometry geom = this._axMapCtrl.TrackCircle();
            this._selectFeaturesByShape(geom);
        }

        private void _selectByLine()
        {
            IGeometry geom = this._axMapCtrl.TrackLine();
            this._selectFeaturesByShape(geom);
        }

        private void _drawPolygon()
        {
            if (this._axMapCtrl.ActiveView == null)
            {
                return;
            }

            ESRI.ArcGIS.Display.IScreenDisplay screenDisplay = this._axMapCtrl.ActiveView.ScreenDisplay;

            // Constant.
            screenDisplay.StartDrawing(screenDisplay.hDC, (System.Int16)
                ESRI.ArcGIS.Display.esriScreenCache.esriNoScreenCache); // Explicit cast.
            ESRI.ArcGIS.Display.IRgbColor rgbColor = new ESRI.ArcGIS.Display.RgbColorClass();
            rgbColor.Red = 255;

            ESRI.ArcGIS.Display.IColor color = rgbColor; // Implicit cast.
            ESRI.ArcGIS.Display.ISimpleFillSymbol simpleFillSymbol = new
                ESRI.ArcGIS.Display.SimpleFillSymbolClass();
            simpleFillSymbol.Color = color;
            

            ESRI.ArcGIS.Display.ISymbol symbol = simpleFillSymbol as
                ESRI.ArcGIS.Display.ISymbol; // Dynamic cast.
            ESRI.ArcGIS.Display.IRubberBand rubberBand = new
                ESRI.ArcGIS.Display.RubberPolygonClass();
            ESRI.ArcGIS.Geometry.IGeometry geometry = rubberBand.TrackNew(screenDisplay,
                symbol);
            screenDisplay.SetSymbol(symbol);

            //screenDisplay.DrawPolygon(geometry);
            screenDisplay.FinishDrawing();
            
            PolygonElementClass element = new PolygonElementClass();
            element.Geometry = geometry;
            // Create lineSymbol
            ISimpleLineSymbol lineSymbol = new SimpleLineSymbolClass();
            // set color & style,....
            ESRI.ArcGIS.Display.IRgbColor lineColor = new ESRI.ArcGIS.Display.RgbColorClass();
            lineColor.Red = 255;
            lineSymbol.Color = lineColor;
            lineSymbol.Width = 3;

            // Create fillSymbol
            ISimpleFillSymbol fillSymbol = new SimpleFillSymbolClass();
            fillSymbol.Outline = lineSymbol;
            ESRI.ArcGIS.Display.IRgbColor fillColor = new ESRI.ArcGIS.Display.RgbColorClass();
            fillColor.Red = 255;
            fillColor.Green = 255;
            fillSymbol.Color = fillColor;
            fillSymbol.Style = esriSimpleFillStyle.esriSFSHollow;
            element.Symbol = fillSymbol;

            this._axMapCtrl.ActiveView.GraphicsContainer.AddElement(element, 0);
            
            //((IGraphicsContainerSelect)(this._axMapCtrl.ActiveView.GraphicsContainer)).SelectElement(element);
            this._axMapCtrl.ActiveView.PartialRefresh(esriViewDrawPhase.esriViewGraphics, null, null);

            if (this._polygonName != "")
            {
                element.Name = this._polygonName;
                this._polygonName = "";
            }
            /*
            else
            {
                int count = 0;
                IGraphicsContainer container = this._axMapCtrl.ActiveView.GraphicsContainer;
                container.Reset();
                while (container.Next() != null)
                {
                    count++;
                }
                element.Name = "Element " + count;
            }
            */
            if (PolygonDrawn != null)
            {
                PolygonDrawn(element.Name);
            }
        }

        private ILayer _getSelectedLayer()
        {
            esriTOCControlItem item = esriTOCControlItem.esriTOCControlItemNone;
            IBasicMap map = null; ILayer layer = null;
            object other = null; object index = null;

            this._axTOCCtrl.GetSelectedItem(ref item, ref map, ref layer, ref other, ref index);
            // check whether the selected item is a layer
            if (item == esriTOCControlItem.esriTOCControlItemLayer)
            {
                return layer;
            }
            else
            {
                return null;
            }
        }

        private ILayer _getLayerByName(string layerName)
        {
            //get the layers from the map
            IEnumLayer layers = this._axMapCtrl.Map.get_Layers();

            layers.Reset();

            ILayer layer = null;
            while ((layer = layers.Next()) != null)
            {
                if (layer.Name.ToLower() == layerName.ToLower())
                    return layer;
            }

            return null;
        }


        private void _axTOCCtrl_OnMouseDown(object sender, ESRI.ArcGIS.Controls.ITOCControlEvents_OnMouseDownEvent e)
        {
            esriTOCControlItem item = esriTOCControlItem.esriTOCControlItemNone;
            IBasicMap map = null; ILayer layer = null;
            object legendGrp = null; object index = null;

            //Determine what kind of item has been clicked on
            this._axTOCCtrl.HitTest(e.x, e.y, ref item, ref map, ref layer, ref legendGrp, ref index);
            if (layer == null) return;
                
            if (e.button == 1) // left click
            {
                if (item == esriTOCControlItem.esriTOCControlItemLegendClass)
                {
                    if (legendGrp == null || (int)index < 0)
                        return;
                    ILegendClass legendItem = ((ILegendGroup)legendGrp).Class[(int)index];
                    this._ChangeLegendItemSymbology(layer, legendItem, (int)index);

                }
            }
            else if (e.button == 2) // right click
            {
                if (item == esriTOCControlItem.esriTOCControlItemLayer)
                {

                    
                }
            }
        }

        private void _ChangeLegendItemSymbology(ILayer layer, ILegendClass legendItem, int index)
        {
            bool symbolChanged = false;
            IFeatureLayer featureLayer = layer as IFeatureLayer;
            if (featureLayer == null) return;
            //Get the IStyleGalleryItem
            IStyleGalleryItem styleGalleryItem = null;

            //Select SymbologyStyleClass based upon feature type
            switch (featureLayer.FeatureClass.ShapeType)
            {
                case esriGeometryType.esriGeometryPoint:
                    styleGalleryItem = this._symWindow.GetItem(esriSymbologyStyleClass.esriStyleClassMarkerSymbols, legendItem.Symbol);
                    break;
                case esriGeometryType.esriGeometryPolyline:
                    styleGalleryItem = this._symWindow.GetItem(esriSymbologyStyleClass.esriStyleClassLineSymbols, legendItem.Symbol);
                    break;
                case esriGeometryType.esriGeometryPolygon:
                    styleGalleryItem = this._symWindow.GetItem(esriSymbologyStyleClass.esriStyleClassFillSymbols, legendItem.Symbol);
                    break;
            }
            if (styleGalleryItem == null) return;

            if (featureLayer is IGeoFeatureLayer)
            {
                IGeoFeatureLayer geoFeatureLayer = (IGeoFeatureLayer)featureLayer;
                if (geoFeatureLayer.Renderer is ISimpleRenderer)
                {
                    ISimpleRenderer simpleRenderer = (ISimpleRenderer)(geoFeatureLayer.Renderer);
                    //Set its symbol from the styleGalleryItem
                    simpleRenderer.Symbol = (ISymbol)styleGalleryItem.Item;
                    symbolChanged = true;
                }
                else if (geoFeatureLayer.Renderer is IUniqueValueRenderer)
                {
                    IUniqueValueRenderer uniqueValueRenderer = (IUniqueValueRenderer)(geoFeatureLayer.Renderer);
                    uniqueValueRenderer.Symbol[uniqueValueRenderer.Value[index]] = (ISymbol)styleGalleryItem.Item;
                    symbolChanged = true;
                }
            }
            if (symbolChanged == true)
            {
                //Fire contents changed event that the TOCControl listens to
                this._axMapCtrl.ActiveView.ContentsChanged();
                //Refresh the display
                this._axMapCtrl.Refresh(esriViewDrawPhase.esriViewGeography, null, null);
            }
        }

        public string Overlay(ArrayList inLayerNames, string outLayerName = "", string overlayType = "INTERSECT")
        {
            string inLayers = "";
            foreach(string layerName in inLayerNames)
            {
                ILayer layer = this._getLayerByName(layerName);
                IDataset dataset = ((IFeatureLayer)layer).FeatureClass as IDataset; 
                //ESRI.ArcGIS.Geodatabase.IDataset dataset = (ESRI.ArcGIS.Geodatabase.IDataset)(layer); // Explicit Cast
                Console.WriteLine("dataset is " + dataset == null);
                this._geoProcessor.SetEnvironmentValue("workspace", dataset.Workspace.PathName);
                Console.WriteLine("workspace is " + dataset.Workspace.PathName);
                //inLayers += featureClass.FeatureDataset.Name + "\\" + featureClass.AliasName + ";";
                inLayers += dataset.Workspace.PathName+"\\"+layerName + ".shp;";
            }
            
            if (inLayers == "")
            {
                Console.WriteLine("6");
                return "";
            }

            string tempDir = System.IO.Path.GetTempPath();
            Console.WriteLine("tempDir" + tempDir.ToString());
            string outLayerFile = "";
            if (outLayerName == "")
            {
                Console.WriteLine("5");
                string filename = "";
                foreach(string layerName in inLayerNames)
                {
                    filename += layerName + "_";    
                }
                filename += "overlay_" + DateTime.Now.ToString("MM_dd_yy_H_mm_ss") + ".shp";
                outLayerFile = System.IO.Path.Combine(tempDir, filename);
            }
            else
            {
                Console.WriteLine("4");           
                outLayerFile = System.IO.Path.Combine(tempDir, outLayerName + ".shp");
            }
            if (System.IO.File.Exists(outLayerFile))
            {
                Console.WriteLine("3");
                return outLayerFile;
            }

            Console.WriteLine("overlayType is " + overlayType);
            //create a new instance of an overlay tool
            IGeoProcessorResult results = null;
            if (overlayType == "INTERSECT")
            {
                Console.WriteLine("2");
                ESRI.ArcGIS.AnalysisTools.Intersect process = new ESRI.ArcGIS.AnalysisTools.Intersect();
                process.in_features = inLayers;
                process.out_feature_class = outLayerFile;
                results = (IGeoProcessorResult)this._geoProcessor.Execute(process, null);
                //RunTool(this._geoProcessor, process, null);
            }
            else if (overlayType == "UNION")
            {
                Console.WriteLine("1");
                Console.WriteLine("inLayers is " + inLayers);
                Console.WriteLine("outLayerName is " + outLayerName);               
                ESRI.ArcGIS.AnalysisTools.Union process = new ESRI.ArcGIS.AnalysisTools.Union(inLayers, outLayerFile);
               
                results = (IGeoProcessorResult)this._geoProcessor.Execute(process, null);
                AddLayer(outLayerFile);
            }

            if (results != null && results.Status == esriJobStatus.esriJobSucceeded)
            {
                return outLayerFile;
            }
            return "";
        }

        public void DefineClassBreaksRenderer2(string layerName, string fieldName, int numClasses, string normalizeField, string classification_scheme)
        {
            //numClasses = 3;
            //normalizeField = "none";
            //classifyMethod = "equal interval";

            IFeatureLayer pFeatureLayer = null;
            IFeatureClass pFeatureClass = null;

            if (null != (pFeatureLayer = _getLayerByName(layerName) as IFeatureLayer))
            {
                pFeatureClass = pFeatureLayer.FeatureClass;
            }

            //Create a Class Break Renderer
            ITable pTable = (ITable)pFeatureClass;
            ITableHistogram pTableHistogram = new BasicTableHistogramClass();
            IBasicHistogram pHistogram = (IBasicHistogram)pTableHistogram;
            pTableHistogram.Field = fieldName;
            if (normalizeField.ToLower() != "none") pTableHistogram.NormField = normalizeField;
            pTableHistogram.Table = pTable;
            object dataFrequency;
            object dataValues;
            pHistogram.GetHistogram(out dataValues, out dataFrequency);

            IClassifyGEN pClassify = new NaturalBreaksClass();
            Console.WriteLine("classifyMethod = " + classification_scheme);
            switch (classification_scheme.ToLower())
            {
                case "equal interval":
                    pClassify = new EqualIntervalClass();
                    break;
                case "quantile":
                    pClassify = new QuantileClass();
                    break;
                case "natural breaks":
                    pClassify = new NaturalBreaksClass();
                    break;
                case "geometrical interval":
                    pClassify = new GeometricalIntervalClass();
                    break;
                default:
                    break;
            }

            pClassify.Classify(dataValues, dataFrequency, ref numClasses);

            double[] gClassbreaks = null;
            gClassbreaks = (double[])pClassify.ClassBreaks;

            ClassBreaksRenderer pClassBreaksRenderer = new ClassBreaksRenderer();
            pClassBreaksRenderer.Field = fieldName;
            pClassBreaksRenderer.BreakCount = numClasses;
            pClassBreaksRenderer.MinimumBreak = gClassbreaks[0];
            if (normalizeField.ToLower() != "none") pClassBreaksRenderer.NormField = normalizeField;

            //Create a color ramp
            IAlgorithmicColorRamp pAlgoRamp = new AlgorithmicColorRamp();
            pAlgoRamp.Algorithm = ESRI.ArcGIS.Display.esriColorRampAlgorithm.esriCIELabAlgorithm;
            pAlgoRamp.ToColor = GetRGBColor(255,0,0);
            pAlgoRamp.FromColor = GetRGBColor(255,255,0);
            pAlgoRamp.Size = numClasses;
            bool bOK;
            pAlgoRamp.CreateRamp(out bOK);
            IEnumColors pColors = pAlgoRamp.Colors;

            //Assign a color symbol, break, and label to each of the classes
            IFillSymbol pFillSymbol;
            for (int i = 0; i < numClasses; i++)
            {
                pFillSymbol = new SimpleFillSymbol();
                pFillSymbol.Color = pColors.Next();

                pClassBreaksRenderer.set_Symbol(i, pFillSymbol as ISymbol);
                pClassBreaksRenderer.set_Break(i, gClassbreaks[i + 1]);
                pClassBreaksRenderer.set_Label(i, string.Format("{0:0.00} - {1:0.00}", gClassbreaks[i], gClassbreaks[i + 1]));
            }

            //draw the graduated color map
            ILayer pLayer = _getLayerByName(layerName);
            IGeoFeatureLayer pGeoFeatureLayer = pLayer as IGeoFeatureLayer;
            pGeoFeatureLayer.Renderer = pClassBreaksRenderer as IFeatureRenderer;



            this._axMapCtrl.ActiveView.PartialRefresh(esriViewDrawPhase.esriViewGeography, pLayer, null);
            this._axMapCtrl.Update();
            _axTOCCtrl.SetBuddyControl(_axMapCtrl);

            //string source_layer_filter = "HISPANIC > POP2000*0.7";
            //this.SelectFeaturesByAttributes(pFeatureLayer.Name, source_layer_filter);
            //CreateLayerFromSel("Hispanic Themed Food Stores");

        }

        public void SelectMapFeaturesByAttributeQuery(ESRI.ArcGIS.Carto.IActiveView activeView, ESRI.ArcGIS.Carto.IFeatureLayer featureLayer, System.String whereClause)
        {
            if (activeView == null || featureLayer == null || whereClause == null)
            {
                return;
            }
            ESRI.ArcGIS.Carto.IFeatureSelection featureSelection = featureLayer as ESRI.ArcGIS.Carto.IFeatureSelection; // Dynamic Cast

            // Set up the query
            ESRI.ArcGIS.Geodatabase.IQueryFilter queryFilter = new ESRI.ArcGIS.Geodatabase.QueryFilterClass();
            queryFilter.WhereClause = whereClause;

            // Invalidate only the selection cache. Flag the original selection
            activeView.PartialRefresh(ESRI.ArcGIS.Carto.esriViewDrawPhase.esriViewGeoSelection, null, null);

            // Perform the selection
            featureSelection.SelectFeatures(queryFilter, ESRI.ArcGIS.Carto.esriSelectionResultEnum.esriSelectionResultNew, false);

            // Flag the new selection
            activeView.PartialRefresh(ESRI.ArcGIS.Carto.esriViewDrawPhase.esriViewGeoSelection, null, null);
        }

        public void CreateLayerFromSel(string layerName, string newLayerName ){
            IFeatureLayer pFeatureLayer = null;
            IFeatureClass pFeatureClass = null;
            if (null != (pFeatureLayer = _getLayerByName(layerName) as IFeatureLayer))
            {
                pFeatureClass = pFeatureLayer.FeatureClass;
            }
            IFeatureLayerDefinition pFLDefinition = pFeatureLayer as IFeatureLayerDefinition;
            IFeatureLayer pNewFeatureLayer = pFLDefinition.CreateSelectionLayer(pFeatureLayer.Name, true, null, null);
            pNewFeatureLayer.Name = newLayerName;
            pNewFeatureLayer.MaximumScale = pFeatureLayer.MaximumScale;
            pNewFeatureLayer.MinimumScale = pFeatureLayer.MinimumScale;
            pNewFeatureLayer.Selectable = pFeatureLayer.Selectable;
            pNewFeatureLayer.Visible = pFeatureLayer.Visible;
            pNewFeatureLayer.ScaleSymbols = pFeatureLayer.ScaleSymbols;
            this._axMapCtrl.Map.AddLayer(pNewFeatureLayer);
            
            //IEnumFieldError fieldErrors;
            //IEnumInvalidObject invalidObjects;
            //ExportLayerToShapefile("C:\\", "test.shp", (ILayer)pNewFeatureLayer, out fieldErrors, out invalidObjects);
        }

        //public void RasterToShapefile(string layerName)
        //{
        //    FeatureLayer pLayer = (FeatureLayer)_getLayerByName(layerName);
        //    IFeatureClass sfeatClass = pLayer.FeatureClass;
        //    IDataset sdataset = (IDataset)sfeatClass;

        //    IRasterLayer pInputRL = _getLayerByName(layerName) as IRasterLayer;
        //    IRaster pInputRaster = pInputRL.Raster;
        //    IWorkspaceFactory pWSF = new ShapefileWorkspaceFactory();
        //    IWorkspace pWS = pWSF.OpenFromFile("C:\\DATA\\CHAP6", 0);
        //    IConversionOp pConversionOp = new RasterConversionOp() as IConversionOp;
        //    //IFeatureClass pOutFClass = pConversionOp.RasterDataToPolygonFeatureData((IGeoDataset)sdataset,(IWorkspace)pWS, "roads.shp", true);
        //}

        public static bool ExportLayerToShapefile(
        string shapePath,
        string shapeName,
        ILayer source,
        out IEnumFieldError fieldErrors,
        out IEnumInvalidObject invalidObjects)
        {
            IGeoFeatureLayer sourceFeatLayer = (IGeoFeatureLayer)source;
            IFeatureLayer sfeatlayer = (IFeatureLayer)sourceFeatLayer;
            IFeatureClass sfeatClass = sfeatlayer.FeatureClass;
            IDataset sdataset = (IDataset)sfeatClass;
            IDatasetName sdatasetName = (IDatasetName)sdataset.FullName;

            ISelectionSet sSelectionSet = (
                (ITable)source).Select(new QueryFilter(),
                esriSelectionType.esriSelectionTypeHybrid,
                esriSelectionOption.esriSelectionOptionNormal,
                sdataset.Workspace);

            IWorkspaceFactory factory;
            factory = new ShapefileWorkspaceFactory();
            IWorkspace targetWorkspace = factory.OpenFromFile(shapePath, 0);
            IDataset targetDataset = (IDataset)targetWorkspace;

            IName targetWorkspaceName = targetDataset.FullName;
            IWorkspaceName tWorkspaceName = (IWorkspaceName)targetWorkspaceName;

            IFeatureClassName tFeatClassname = (IFeatureClassName)new FeatureClassName();
            IDatasetName tDatasetName = (IDatasetName)tFeatClassname;
            tDatasetName.Name = shapeName;
            tDatasetName.WorkspaceName = tWorkspaceName;

            IFieldChecker fieldChecker = new FieldChecker();
            IFields sFields = sfeatClass.Fields;
            IFields tFields = null;

            fieldChecker.InputWorkspace = sdataset.Workspace;
            fieldChecker.ValidateWorkspace = targetWorkspace;

            fieldChecker.Validate(sFields, out fieldErrors, out tFields);
            if (fieldErrors != null)
            {
                IFieldError fieldError = null;
                while ((fieldError = fieldErrors.Next()) != null)
                {
                    Console.WriteLine(fieldError.FieldError + " : " + fieldError.FieldIndex);
                }
                Console.WriteLine("[ExportDataViewModel.cs] Errors encountered during field validation");
            }

            string shapefieldName = sfeatClass.ShapeFieldName;
            int shapeFieldIndex = sfeatClass.FindField(shapefieldName);
            IField shapefield = sFields.get_Field(shapeFieldIndex);
            IGeometryDef geomDef = shapefield.GeometryDef;
            IClone geomDefClone = (IClone)geomDef;
            IClone targetGeomDefClone = geomDefClone.Clone();
            IGeometryDef tGeomDef = (IGeometryDef)targetGeomDefClone;

            IFeatureDataConverter2 featDataConverter = (IFeatureDataConverter2)new FeatureDataConverter();
            invalidObjects = featDataConverter.ConvertFeatureClass(
                sdatasetName,
                null,
                sSelectionSet,
                null,
                tFeatClassname,
                tGeomDef,
                tFields,
                "",
                1000, 0);

            string fullpath = System.IO.Path.Combine(shapePath, shapeName);
            return System.IO.File.Exists(fullpath);
        }

        public void DefineClassBreaksRenderer(string layerName, string fieldName, int numClasses, string normalizeField, string classifyMethod, string isLarger)
        {
            //numClasses = 3;
            //normalizeField = "none";
            //classifyMethod = "equal interval";

            IFeatureLayer pFeatureLayer = null;
            IFeatureClass pFeatureClass = null;

            if (null != (pFeatureLayer = _getLayerByName(layerName) as IFeatureLayer))
            {
                pFeatureClass = pFeatureLayer.FeatureClass;
            }

            ITable pTable = (ITable)pFeatureClass;
            ITableHistogram pTableHistogram = new BasicTableHistogramClass();
            IBasicHistogram pHistogram = (IBasicHistogram)pTableHistogram;
            pTableHistogram.Field = fieldName;
            if (normalizeField.ToLower() != "none") pTableHistogram.NormField = normalizeField;
            pTableHistogram.Table = pTable;
            object dataFrequency;
            object dataValues;
            pHistogram.GetHistogram(out dataValues, out dataFrequency);

            IClassifyGEN pClassify = new NaturalBreaksClass();
            Console.WriteLine("classifyMethod = "+classifyMethod);
            switch (classifyMethod.ToLower())
            {
                case "equal interval":
                    pClassify = new EqualIntervalClass();
                    Console.WriteLine("f^URFVHTHTGMC^UC%R&GG&^TI" + classifyMethod);
                    break;
                case "quantile":
                    pClassify = new QuantileClass();
                    Console.WriteLine("f^URFVHTHTGMC^UC%R&GG&^TI" + classifyMethod);
                    break;
                case "natural breaks":
                    pClassify = new NaturalBreaksClass();
                    Console.WriteLine("f^URFVHTHTGMC^UC%R&GG&^TI" + classifyMethod);
                    break;
                case "geometrical interval":
                    pClassify = new GeometricalIntervalClass();
                    Console.WriteLine("f^URFVHTHTGMC^UC%R&GG&^TI" + classifyMethod);
                    break;
                default:
                    break;
            }

            pClassify.Classify(dataValues, dataFrequency, ref numClasses);

            double[] gClassbreaks = null;
            gClassbreaks = (double[])pClassify.ClassBreaks;

            ClassBreaksRenderer pClassBreaksRenderer = new ClassBreaksRenderer();
            pClassBreaksRenderer.Field = fieldName;
            pClassBreaksRenderer.BreakCount = numClasses;
            pClassBreaksRenderer.MinimumBreak = gClassbreaks[0];

            ISimpleMarkerSymbol pMarkerSymbol;

            double minSymbolSize;
            double symbolSizeStep;
            if (isLarger == "true")
            {
                this.minSymbolSize *= 1.2;
                this.symbolSizeStep *= 1.2;
            }
            else if (isLarger == "false")
            {
                this.minSymbolSize /= 1.2;
                this.symbolSizeStep /= 1.2;
            }
            for (int i = 0; i < numClasses; i++)
            {
                pMarkerSymbol = new SimpleMarkerSymbol();
                pMarkerSymbol.Color = GetRGBColor(255, 255, 0);
                pMarkerSymbol.Outline = true;
                pMarkerSymbol.OutlineColor = GetRGBColor(0, 0, 0);
                pMarkerSymbol.Size = this.minSymbolSize + this.symbolSizeStep * i;
                pMarkerSymbol.Style = ESRI.ArcGIS.Display.esriSimpleMarkerStyle.esriSMSCircle;

                pClassBreaksRenderer.set_Symbol(i, pMarkerSymbol as ISymbol);
                pClassBreaksRenderer.set_Break(i, gClassbreaks[i + 1]);
                pClassBreaksRenderer.set_Label(i, string.Format("{0} - {1}", gClassbreaks[i], gClassbreaks[i + 1]));
            }

            ILayer pLayer = _getLayerByName(layerName);
            IGeoFeatureLayer pGeoFeatureLayer = pLayer as IGeoFeatureLayer;
            pGeoFeatureLayer.Renderer = pClassBreaksRenderer as IFeatureRenderer;
            //pGeoFeatureLayer.DisplayField = "hahaha";
            //pGeoFeatureLayer.DisplayAnnotation = true;

            //this._axMapCtrl.ActiveView.Refresh();

            this._axMapCtrl.ActiveView.PartialRefresh(esriViewDrawPhase.esriViewGeography, pLayer, null);
            this._axMapCtrl.Update();
            _axTOCCtrl.SetBuddyControl(_axMapCtrl);
        }

        static private IRgbColor GetRGBColor(int R, int G, int B)
        {
            IRgbColor pColor = new RgbColor();
            pColor.Red = R;
            pColor.Green = G;
            pColor.Blue = B;
            return pColor;
        }

        public string Buffer(string inLayerName, string distString, string outLayerName = "")
        {
            
            IFeatureLayer inLayer = this._getLayerByName(inLayerName) as IFeatureLayer;

            if (inLayer == null)
            {
                return "";
            }

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
            if (this.GetLayeFromName(outLayerFile) != null)
            {
                this._axMapCtrl.Map.DeleteLayer(this.GetLayeFromName(outLayerFile));
            }

            //create a new instance of a buffer tool
            ESRI.ArcGIS.AnalysisTools.Buffer buffer = new ESRI.ArcGIS.AnalysisTools.Buffer(inLayer, outLayerFile, distString);

            //execute the buffer tool
            IGeoProcessorResult results = (IGeoProcessorResult)this._geoProcessor.Execute(buffer, null);
            if (results.Status == esriJobStatus.esriJobSucceeded)
            {
                return outLayerFile;
            }

            return "";
        }

        public System.Int32 GetIndexNumberFromLayerName(string layerName)
        {
            if (layerName == null)return -1;
            // Get the number of layers
            int numberOfLayers = this._map.LayerCount;
            // Loop through the layers and get the correct layer index
            for (System.Int32 i = 0; i < numberOfLayers; i++)
            {
                if (layerName == this._map.get_Layer(i).Name)
                {
                    // Layer was found
                    return i;
                }
            }
            // No layer was found
            return -1;
        }

        public ILayer GetLayeFromName(string layerName)
        {
            if (layerName == null) return null;
            // Get the number of layers
            int numberOfLayers = this._map.LayerCount;
            // Loop through the layers and get the correct layer index
            for (System.Int32 i = 0; i < numberOfLayers; i++)
            {
                if (layerName == this._map.get_Layer(i).Name)
                {
                    // Layer was found
                    return this._map.get_Layer(i);
                }
            }
            // No layer was found
            return null;
        }

        public string GetLayeNameFromPath(string path)
        {
            return (path.Substring(path.LastIndexOf("\\") + 1, path.IndexOf(".") - path.LastIndexOf("\\") - 1));
        }

    }
}
