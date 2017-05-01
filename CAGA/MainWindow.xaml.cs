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
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Windows.Controls.Ribbon;
using System.Speech.Synthesis;
using System.Xml;
using Newtonsoft.Json.Linq;

using CAGA.NUI;
using CAGA.Dialogue;
using CAGA.Map;

namespace CAGA
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : RibbonWindow
    {
        // input
        private KinectManager kinectMgr;

        // output
        private ArcMapManager mapMgr;
        private SpeechSynthesizer speechSyn;      
        
        // dialogue manager
        private DialogueManager dlgMgr;

        // UI controls
        private Microsoft.Win32.OpenFileDialog openFileDlg;
        private ListPlainOptionsWindow listPlainOptionWindow;
        private ListMapLayerOptionsWindow listMapLayerOptionWindow;
        private ListOptionsWithExamplesWindow listOptionsWithExampleWindow;
        private StatResultsWindow statResultsWindow;
        private SummaryResultsWindow sumResultsWindow;

        public MainWindow()
        {
            InitializeComponent();
            openFileDlg = new Microsoft.Win32.OpenFileDialog();
            listPlainOptionWindow = null;
            listMapLayerOptionWindow = null;
            listOptionsWithExampleWindow = null;
            statResultsWindow = null;
            sumResultsWindow = null;
        }

        private void Log(string content, string type)
        {
            Run newTimeLine = new Run("[" + System.DateTime.Now.ToShortDateString() + " " + System.DateTime.Now.ToLongTimeString() + "]");
            newTimeLine.FontSize = 10;           

            Run newContentLine = new Run(content);
            newContentLine.FontSize = 15;

            Run newBlankLine = new Run(" ");
            newBlankLine.FontSize = 8;

            switch (type)
            {
                case "error":
                    newContentLine.Foreground = Brushes.Red;
                    break;
                case "info":
                    newContentLine.Foreground = Brushes.Green;
                    break;
                case "warning":
                    newContentLine.Foreground = Brushes.Yellow;
                    break;
                default:
                    newContentLine.Foreground = Brushes.Black;
                    break;
            }  
            InlineCollection inlines = statusTB.Inlines;
            if (inlines.Count == 0)
            {
                inlines.Add(newContentLine);
                inlines.Add(new LineBreak());
                return;
            }
            inlines.InsertBefore(inlines.FirstInline, new LineBreak());
            inlines.InsertBefore(inlines.FirstInline, newTimeLine);
            inlines.InsertBefore(inlines.FirstInline, new LineBreak());
            inlines.InsertBefore(inlines.FirstInline, newContentLine);
            inlines.InsertBefore(inlines.FirstInline, new LineBreak());
            inlines.InsertBefore(inlines.FirstInline, newBlankLine);
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Initialize();
        }

        private void Initialize()
        {
            // Load the map control
            mapMgr = new ArcMapManager(this.mapGrid, this.layoutGrid, this.tocGrid);           
            mapMgr.Initialize();
            MapPanel.Activate();
            
            //mapMgr.LoadMap(@"C:\Work\Data\GISLAB\Data\Oleader.mxd");
            //ArrayList inputLayers = new ArrayList();
            //inputLayers.Add("parcels");
            //inputLayers.Add("FloodAreas");
            //string outputFile = mapMgr.Overlay(inputLayers);

            /*
            Hashtable result = mapMgr.GetFieldStatistics("parcels", "Acreage", true);
            this.statResultsWindow = new StatResultsWindow(result);
            this.statResultsWindow.Owner = this;
            this.statResultsWindow.Show();
            */

            /*
            Hashtable result = mapMgr.GetDataSummary("parcels", "Minimum.UseCode, Sum.Acreage", "UseCode", true);
            this.sumResultsWindow = new SummaryResultsWindow(result);
            this.sumResultsWindow.Owner = this;
            this.sumResultsWindow.Show();
            */
            mapMgr.PolygonDrawn += Polygon_Drawn;         
            
            // Load the Kinect sensor
            kinectMgr = new KinectManager();
            kinectMgr.KinectStatusChanged += Kinects_StatusChanged;
            if (kinectMgr.LoadKinectSensor() == false)
            {
                Log("Kinect sensor is not ready.", "error");
            }
            else
            {               
                Log("Kinect sensor is ready.", "info");
            }

            // Load the managers to display respective images
            //kinectMgr.LoadColorManager(colorDisplay);
            //kinectMgr.LoadDepthManager(depthDisplay);
            //kinectMgr.LoadSkeletonDisplayManager(skeletonCanvas);

            // Load and start the speech recognition
            string grammarPath = System.IO.Path.Combine(Environment.CurrentDirectory, @"NUI\Speech\grammar.xml");
            kinectMgr.LoadSpeechRecognizer(grammarPath);
            kinectMgr.SpeechRecognized += Speech_Recognized;
            kinectMgr.StartSpeechRecognition();
            Log("Speech recognition is started.", "info");

            // Load and start the gesture recognition (not functing yet)
            //kinectMgr.LoadGestureRecognizer();
            //kinectMgr.GestureRecognzied += Gesture_Recognized;
            //kinectMgr.StartGestureRecognition();
            //Log("Gesture recognition is started.", "info");

            // Load the speech synthesizer
            speechSyn = new SpeechSynthesizer();
            //speechSyn.SetOutputToDefaultAudioDevice();
            //speechSyn.SpeakStarted += new EventHandler<SpeakStartedEventArgs>(speechSyn_SpeakStarted);
            
            // Start the dialogue manager
            dlgMgr = new DialogueManager("CAGA", mapMgr, @"Dialogue\kb_caga.db");
            // use machine name + user name as id, user name as name
            dlgMgr.NewParticipant(Environment.MachineName + "-" + Environment.UserName, Environment.UserName);
            Log("New Participant " + Environment.UserName + " is involved", "info");
        }

        public ArcMapManager MapManager
        {
            get
            {
                return this.mapMgr;
            }
        }

        void Kinects_StatusChanged(string status)
        {
            Log("Kinect status changed to " + status, "info");
        }

        void Speech_Recognized(SortedList result)
        {
            Console.WriteLine("--------------------------------------------------------------");
            Console.WriteLine("MainWindow: Speech_Recognized");
            Dispatcher.Invoke(new Action(() =>
            {
                if ((dlgMgr.IsRunning == true && speechSyn.State != SynthesizerState.Speaking) || (isSimulated))
                {
                    Log("Speech is recognized and sent to the dialogue manager", "info");
                    //Update
                    ArrayList respList = dlgMgr.Update(result);
                    //Response
                    Process_Response(respList);
                }
            }));
        }

        void Gesture_Recognized(SortedList result)
        {

        }

        void Polygon_Drawn(string name)
        {
            if (name != "")
            {
                mapMgr.SetFunctionMode(MapFunctionMode.None);
                
                SortedList result = new SortedList();
                result.Add("Specify Region By Drawing", name);
                Log("Drawing is finished and sent to the dialogue manager", "info");

                ArrayList respList = dlgMgr.Update(result);
                Process_Response(respList);
            }

        }

        void Process_Response(ArrayList respList)
        {
            //Options: Mapping Inside, Nearby and Density
            if (this.listPlainOptionWindow != null)this.listPlainOptionWindow.Close();
            if (this.listMapLayerOptionWindow != null)this.listMapLayerOptionWindow.Close();
            if (this.listOptionsWithExampleWindow != null)this.listOptionsWithExampleWindow.Close();
            if (this.statResultsWindow != null)this.statResultsWindow.Close();
            if (this.sumResultsWindow != null)this.sumResultsWindow.Close();

            foreach (DialogueResponse resp in respList)
            {
                switch (resp.DlgRespType) { 
                    case DialogueResponseType.speechError:
                        speechSyn.SpeakAsync(resp.RespContent.ToString());
                        Log(resp.RespContent.ToString(), "error");
                        break;
                    case DialogueResponseType.speechInfo:
                        speechSyn.SpeakAsync(resp.RespContent.ToString());
                        Log(resp.RespContent.ToString(), "info");
                        break;
                    case DialogueResponseType.speechQuestion:
                        speechSyn.SpeakAsync(resp.RespContent.ToString());
                        Log("Question: " + resp.RespContent.ToString(), "info");
                        break;
                    case DialogueResponseType.mapLayerAdded:
                        mapMgr.AddLayer(resp.RespContent.ToString(),4);
                        Log("A new map layer is added: " + resp.RespContent.ToString(), "info");
                        break;
                    case DialogueResponseType.bufferZoneAdded:
                        
                        
                        string layername = mapMgr.GetLayeNameFromPath(resp.RespContent.ToString());
                        speechSyn.SpeakAsync("A buffer zone is added");
                        Log("A buffer zone is added: " + resp.RespContent.ToString(), "info");
                        break;
                    case DialogueResponseType.mapLayerRemoved:
                        mapMgr.HideLayer(resp.RespContent.ToString());
                        Log("A new map layer is hidden: " + resp.RespContent.ToString(), "info");
                        break;
                    case DialogueResponseType.mapDocumentOpened:
                        mapMgr.LoadMap(resp.RespContent.ToString());
                        Log("A new map document is opened: " + resp.RespContent.ToString(), "info");
                        break;
                    case DialogueResponseType.featureLayerInfo:
                        Console.WriteLine("****************************");
                        string feature_class = resp.RespContent.ToString();
                        Console.WriteLine("_mapMgr.GetTotalSelectedFeaturesInLayer=" + MapManager.GetTotalSelectedFeaturesInLayer(feature_class));
                        MapManager.SelectFeaturesByAttributes(feature_class, "");
                        int count = this.MapManager.GetTotalSelectedFeaturesInLayer(feature_class);
                        MapManager.ClearMapSelection();
                        feature_class = String.Join(" ", feature_class.Split('_'));
                        Log("There are total of " + count + " " + feature_class + ".", "info");
                        speechSyn.SpeakAsync("There are total of " + count + " " + feature_class + ".");
                        break;
                    case DialogueResponseType.listPlainOptions:
                        PlainOptionListData optionListData = resp.RespContent as PlainOptionListData;
                        if (optionListData != null)
                        {
                            this.listPlainOptionWindow = new ListPlainOptionsWindow(optionListData);
                            this.speechSyn.SpeakAsync(optionListData.Opening);
                            this.listPlainOptionWindow.Owner = this;
                            this.listPlainOptionWindow.Show();
                            this.listPlainOptionWindow.Closed += this.OnlistPlainOptionWindowClosed;
                        }
                        break;
                    case DialogueResponseType.listMapLayerOptions:
                        MapLayerOptionListData optionListData2 = resp.RespContent as MapLayerOptionListData;
                        if (optionListData2 != null)
                        {
                            this.listMapLayerOptionWindow = new ListMapLayerOptionsWindow(optionListData2);
                            this.speechSyn.SpeakAsync(optionListData2.Opening);
                            this.listMapLayerOptionWindow.Show();
                            this.listMapLayerOptionWindow.Owner = this;
                            this.listMapLayerOptionWindow.Closed += this.OnlistMapLayerOptionWindowClosed;
                            this.listMapLayerOptionWindow.LoadMap(mapMgr.GetMapFile());                          
                        }
                        break;
                    case DialogueResponseType.listOptionsWithExamples:
                        OptionWithExampleListData optionListData3 = resp.RespContent as OptionWithExampleListData;
                        if (optionListData3 != null)
                        {
                            this.listOptionsWithExampleWindow = new ListOptionsWithExamplesWindow(optionListData3);
                            this.speechSyn.SpeakAsync(optionListData3.Opening);
                            this.listOptionsWithExampleWindow.Owner = this;
                            this.listOptionsWithExampleWindow.Show();
                            this.listOptionsWithExampleWindow.Closed += this.OnlistOptionsWithExampleWindowClosed;
                        }
                        break;
                    case DialogueResponseType.drawPolygonStarted:
                        mapMgr.DrawPolygon(resp.RespContent.ToString());
                        break;
                    case DialogueResponseType.selectByAttributes:
                        if (mapMgr != null)
                        {
                            SelectByAttributeWindow selectWindow = new SelectByAttributeWindow(mapMgr);
                            selectWindow.Show();
                            selectWindow.Owner = this;
                            selectWindow.Closed += this.OnSelectByAttributeWindowClosed;
                        }
                        break;
                    case DialogueResponseType.statisticResults:
                        Hashtable statResults = resp.RespContent as Hashtable;
                        if (statResults != null)
                        {
                            this.statResultsWindow = new StatResultsWindow(statResults);
                            this.statResultsWindow.Owner = this;
                            this.statResultsWindow.Show();
                        }
                        break;
                    case DialogueResponseType.summaryResults:
                        Hashtable sumResults = resp.RespContent as Hashtable;
                        if (sumResults != null)
                        {
                            this.sumResultsWindow = new SummaryResultsWindow(sumResults);
                            this.sumResultsWindow.Owner = this;
                            this.sumResultsWindow.Show();
                        }
                        break;
                    case DialogueResponseType.debugError:
                        Log(resp.RespContent.ToString(), "error");
                        break;
                    case DialogueResponseType.debugInfo:
                        Log(resp.RespContent.ToString(), "info");
                        break;
                    case DialogueResponseType.debugWarning:
                        Log(resp.RespContent.ToString(), "warning");
                        break;
                    default: 
                        break;
                }
            }
        }

        private void _uncheckAllButMe(object btn)
        {
            ArrayList btnGroup = new ArrayList();
            btnGroup.Add(PanMapBtn);
            btnGroup.Add(ZoomInBtn);
            btnGroup.Add(ZoomOutBtn);
            btnGroup.Add(SelectFeatureBtn);
            btnGroup.Add(IdentifyFeatureBtn);
            btnGroup.Add(DrawPolygonBtn);

            foreach (object button in btnGroup)
            {
                if (button != btn)
                {
                    if (button is RibbonToggleButton)
                    {
                        ((RibbonToggleButton)button).IsChecked = false;
                    }
                    else if (button is RibbonSplitButton)
                    {
                        ((RibbonSplitButton)button).IsChecked = false;
                    }
                }
            }


        }
        private void OpenMapBtn_Click(object sender, RoutedEventArgs e)
        {
            this.OpenMapFileDlg();
        }

        private void AddLayerBtn_Click(object sender, RoutedEventArgs e)
        {
            this.AddLayerFileDlg();
        }

        private void RemoveLayerBtn_Click(object sender, RoutedEventArgs e)
        {
            mapMgr.RemoveLayer();
        }

        private void PanMapBtn_Checked(object sender, RoutedEventArgs e)
        {
            _uncheckAllButMe(PanMapBtn);
            mapMgr.SetFunctionMode(MapFunctionMode.Pan);
        }

        private void PanMapBtn_Unchecked(object sender, RoutedEventArgs e)
        {
            mapMgr.SetFunctionMode(MapFunctionMode.None);
        }

        private void ZoomInBtn_Checked(object sender, RoutedEventArgs e)
        {
            _uncheckAllButMe(ZoomInBtn);
            
            mapMgr.SetFunctionMode(MapFunctionMode.ZoomIn);
        }

        private void ZoomInBtn_Unchecked(object sender, RoutedEventArgs e)
        {
            mapMgr.SetFunctionMode(MapFunctionMode.None);
        }

        private void ZoomOutBtn_Checked(object sender, RoutedEventArgs e)
        {
            _uncheckAllButMe(ZoomOutBtn);
            
            mapMgr.SetFunctionMode(MapFunctionMode.ZoomOut);
        }

        private void ZoomOutBtn_Unchecked(object sender, RoutedEventArgs e)
        {
            mapMgr.SetFunctionMode(MapFunctionMode.None);
        }

        private void ZoomToExtentBtn_Click(object sender, RoutedEventArgs e)
        {
            mapMgr.ZoomToMaxExtent();
        }

        private void ZoomToPrevBtn_Click(object sender, RoutedEventArgs e)
        {
            mapMgr.ZoomToPrev();
        }

        private void ZoomToNextBtn_Click(object sender, RoutedEventArgs e)
        {
            mapMgr.ZoomToNext();
        }        
        
        
        private void ZoomToLayerBtn_Click(object sender, RoutedEventArgs e)
        {
            mapMgr.ZoomToLayer();
        }

        private void SelectFeatureBtn_Click(object sender, RoutedEventArgs e)
        {
            if (SelectFeatureBtn.IsChecked == true)
            {
                _uncheckAllButMe(SelectFeatureBtn);
                _selectFeature();

            }
            else 
            {
                mapMgr.SetFunctionMode(MapFunctionMode.None);
            }
        }

        private void SelectFeatureItem_Selected(object sender, RoutedEventArgs e)
        {
            SelectFeatureBtn.IsChecked = true;
            _uncheckAllButMe(SelectFeatureBtn);
            _selectFeature();
        }

        private void SelectByGraphicsBtn_Click(object sender, RoutedEventArgs e)
        {
            this.mapMgr.SelectFeaturesByGraphics();
        }

        private void _selectFeature()
        {
            switch(SelectFeatureGallery.SelectedValue.ToString()){
                case "By Rectangle":
                    mapMgr.SetFunctionMode(MapFunctionMode.SelectByRectangle);
                    break;
                case "By Polygon":
                    mapMgr.SetFunctionMode(MapFunctionMode.SelectByPolygon);
                    break;
                case "By Circle":
                    mapMgr.SetFunctionMode(MapFunctionMode.SelectByCircle);
                    break;
                case "By Line":
                    mapMgr.SetFunctionMode(MapFunctionMode.SelectByLine);
                    break;
            }
        }

        private void UnselectFeatureBtn_Click(object sender, RoutedEventArgs e)
        {
            mapMgr.ClearMapSelection();
        }

        private void IdentifyFeatureBtn_Checked(object sender, RoutedEventArgs e)
        {
            _uncheckAllButMe(IdentifyFeatureBtn);

            mapMgr.SetFunctionMode(MapFunctionMode.Info);
        }

        private void IdentifyFeatureBtn_Unchecked(object sender, RoutedEventArgs e)
        {
            mapMgr.SetFunctionMode(MapFunctionMode.None);
        }

        private void AttrTableBtn_Click(object sender, RoutedEventArgs e)
        {
            mapMgr.ShowAttributeTable();
        }

        private void DrawPolygonBtn_Checked(object sender, RoutedEventArgs e)
        {
            _uncheckAllButMe(DrawPolygonBtn);
            this.mapMgr.SetFunctionMode(MapFunctionMode.DrawPolygon);
        }

        private void DrawPolygonBtn_Unchecked(object sender, RoutedEventArgs e)
        {
            this.mapMgr.SetFunctionMode(MapFunctionMode.None);
        }

        //Dialogue Manager Toggle
        private void ToggleDlgBtn_Click(object sender, RoutedEventArgs e)
        {
            if (dlgMgr.IsRunning == false)
            {
                ArrayList respList = dlgMgr.Start();
                Log("A new dialogue is started!", "info");
                Process_Response(respList);

                ToggleDlgBtn.Label = "Stop Dialogue";
                ToggleDlgBtn.LargeImageSource = new BitmapImage(new Uri(@"Images\stop_dlg.png", UriKind.RelativeOrAbsolute));
                ToggleDlgBtn.ToolTip = "Stop the current dialogue";
            }
            else
            {
                dlgMgr.Stop();
                Log("The current dialogue is stopped!", "info");

                ToggleDlgBtn.Label = "Start Dialogue";
                ToggleDlgBtn.LargeImageSource = new BitmapImage(new Uri(@"Images\start_dlg.png", UriKind.RelativeOrAbsolute));
                ToggleDlgBtn.ToolTip = "Start a new dialogue";
            }

        }

        private void ToggleSpeechBtn_Click(object sender, RoutedEventArgs e)
        {
            if (kinectMgr.speechRecognizer.isRunning == false)
            {
                kinectMgr.StartSpeechRecognition();
                Log("Speech recognition is started!", "info");

                ToggleSpeechBtn.Label = "Stop Recognition";
                ToggleSpeechBtn.LargeImageSource = new BitmapImage(new Uri(@"Images\stop_speech.png", UriKind.RelativeOrAbsolute));
                ToggleSpeechBtn.ToolTip = "Stop the speech recognition";
            }
            else 
            {
                kinectMgr.StopSpeechRecognition();
                Log("Speech recognition is stopped!", "info");

                ToggleSpeechBtn.Label = "Start Recognition";
                ToggleSpeechBtn.LargeImageSource = new BitmapImage(new Uri(@"Images\start_speech.png", UriKind.RelativeOrAbsolute));
                ToggleSpeechBtn.ToolTip = "Start the speech recognition";
            }
        }

        private bool isSimulated = false;

        private void SimSpeechBtn_Click(object sender, RoutedEventArgs e)
        {
            isSimulated = true;
            SpeechSimWindow simWindow = new SpeechSimWindow(this.kinectMgr);
            simWindow.Show();
        }

        private void RibbonWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            this.dlgMgr.Stop();
            this.mapMgr.Dispose();
            this.kinectMgr.Dispose();
            this.speechSyn.Dispose();
        }

        private void OpenMapFileDlg()
        {
            this.openFileDlg.Title = "Browse Map Document";
            this.openFileDlg.Filter = "Map Documents (*.mxd)|*.mxd";
            Nullable<bool> result = this.openFileDlg.ShowDialog();

            //Exit if no map document is selected
            string sFilePath = this.openFileDlg.FileName;
            if (sFilePath == "" || result == false)
            {
                return;
            }

            //Validate and load map document
            if (mapMgr.LoadMap(sFilePath) == true)
            {
                Log("The map document (" + sFilePath + ") is loaded", "info");
            }
            else
            {
                Log(sFilePath + " is not a valid map document", "error");
            }
            this.openFileDlg.Reset();
        }

        private void AddLayerFileDlg()
        {
            this.openFileDlg.Title = "Add Layer";
            this.openFileDlg.Filter = "Shape File (*.shp)|*.shp|Layer Files (*.lyr)|*.lyr";
            Nullable<bool> result = this.openFileDlg.ShowDialog();

            //Exit if no map document is selected
            string sFilePath = this.openFileDlg.FileName;
            if (sFilePath == "" || result == false)
            {
                return;
            }

            //Validate and load map document
            
            if (mapMgr.AddLayer(sFilePath) == true)
            {
                Log("The layer (" + sFilePath + ") is added", "info");
            }
            else
            {
                Log(sFilePath + " is not a valid layer file", "error");
            }

            this.openFileDlg.Reset();
        }

        private void SaveMapBtn_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.SaveFileDialog saveFileDlg = new Microsoft.Win32.SaveFileDialog();
            saveFileDlg.Title = "Save Map";
            saveFileDlg.Filter = "Map Documents|*.mxd|All Files|*.*";
            saveFileDlg.RestoreDirectory = true;
            saveFileDlg.OverwritePrompt = true;
            saveFileDlg.AddExtension = true;

            //get the layer name from the user
            Nullable<bool> result = saveFileDlg.ShowDialog();
            string sFilePath = saveFileDlg.FileName;

            if (sFilePath != "" && result == true)
            {
                if (System.IO.File.Exists(sFilePath))
                {
                    System.IO.File.Delete(sFilePath);
                }

                this.mapMgr.SaveMap(sFilePath);
            }
        }

        private void OnlistPlainOptionWindowClosed(object sender, EventArgs e)
        {
            PlainOptionItemData itemData = ((ListPlainOptionsWindow)sender).SelectedItem;
            if (itemData != null)
            {
                this.kinectMgr.speechRecognizer.Simulate(itemData.Title);
            }
        }

        private void OnlistMapLayerOptionWindowClosed(object sender, EventArgs e)
        {
        }

        private void OnlistOptionsWithExampleWindowClosed(object sender, EventArgs e)
        {
            OptionWithExampleItemData itemData = ((ListOptionsWithExamplesWindow)sender).SelectedItem;
            if (itemData != null)
            {
                this.kinectMgr.speechRecognizer.Simulate(itemData.Title);
            }
        }
        

        private void OnSelectByAttributeWindowClosed(object sender, EventArgs e)
        {
            SelectByAttributeWindow window = sender as SelectByAttributeWindow;

            if (window.LayerName != "")
            {
                SortedList result = new SortedList();
                result.Add("Specify Region By Attributes", window.LayerName);
                Log("Specify Region By Attributes is finished and sent to the dialogue manager", "info");
                ArrayList respList = dlgMgr.Update(result);
                Process_Response(respList);
            }
        }
        
        private void DocumentPane_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0)
            {
                AvalonDock.DocumentContent activeContent = e.AddedItems[0] as AvalonDock.DocumentContent;
                if (activeContent.Title == "Layout")
                {
                    if (this.mapMgr != null)
                    {
                        this.mapMgr.ActivateLayoutView(true);
                    }
                }
                else if (activeContent.Title == "Map")
                {
                    if (this.mapMgr != null)
                    {
                        this.mapMgr.ActivateMapView(true);
                    }
                }
            }
        }

        private void ribbon_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void About_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("CAGA is a Collaborative Agent for GIS Analysis.", "About");
        }

        private void depthDisplay_ImageFailed(object sender, ExceptionRoutedEventArgs e)
        {

        }

        private void KinectCtrlPanel_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void DockablePane_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void DevCtrlPanel_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }
    }
}
