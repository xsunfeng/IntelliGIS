using System;
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

using ESRI.ArcGIS.Controls;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Display;

namespace CAGA.Map
{
    /// <summary>
    /// Interaction logic for SymbologyWindow.xaml
    /// </summary>
    public partial class SymbologyWindow : Window
    {
        private AxSymbologyControl _axSymCtrl;
        private IStyleGalleryItem _styleGalleryItem;
        public SymbologyWindow()
        {
            InitializeComponent();
        }

        public void Initialize()
        {
            // Create an object of the symbology control.
            
            System.Windows.Forms.Integration.WindowsFormsHost host = new System.Windows.Forms.Integration.WindowsFormsHost();
            // Create an object of the map control.
            this._axSymCtrl = new AxSymbologyControl();
            this._axSymCtrl.BeginInit();
            
            host.Child = this._axSymCtrl;
            this.SymbolCtrlGrid.Children.Add(host);
            this._axSymCtrl.OnItemSelected += new ESRI.ArcGIS.Controls.ISymbologyControlEvents_Ax_OnItemSelectedEventHandler(this.axSymCtrl_OnItemSelected);
            this._axSymCtrl.EndInit();
            this.LoadStyleFile();
        }

        private void OKBtn_Click(object sender, RoutedEventArgs e)
        {
            this.Hide();
        }

        private void CancelBtn_Click(object sender, RoutedEventArgs e)
        {
            this._styleGalleryItem = null;
            this.Hide();
        }

        private void axSymCtrl_OnItemSelected(object sender, ESRI.ArcGIS.Controls.ISymbologyControlEvents_OnItemSelectedEvent e)
        {
            this._styleGalleryItem = (IStyleGalleryItem)e.styleGalleryItem;
            ShowPreviewImage();
        }

        private void ShowPreviewImage()
        {
            //Get and set the style class 
            ISymbologyStyleClass symbologyStyleClass = this._axSymCtrl.GetStyleClass(_axSymCtrl.StyleClass);

            //Preview an image of the symbol
            stdole.IPictureDisp picture = symbologyStyleClass.PreviewItem(this._styleGalleryItem, (int)PreviewImage.Width, (int)PreviewImage.Height);
            System.Drawing.Bitmap bitmap = System.Drawing.Image.FromHbitmap(new System.IntPtr(picture.Handle));
            PreviewImage.Source = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(bitmap.GetHbitmap(), IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
            
        }

        public IStyleGalleryItem GetItem(esriSymbologyStyleClass styleClass, ISymbol symbol)
        {
            
            this._styleGalleryItem = null;

            //Get and set the style class
            this._axSymCtrl.StyleClass = styleClass;
            ISymbologyStyleClass symbologyStyleClass = this._axSymCtrl.GetStyleClass(styleClass);

            //Create a new server style gallery item with its style set
            IStyleGalleryItem styleGalleryItem = new ServerStyleGalleryItem();
            styleGalleryItem.Item = symbol;
            styleGalleryItem.Name = "Current";

            //Add the item to the style class and select it
            symbologyStyleClass.AddItem(styleGalleryItem, 0);
            symbologyStyleClass.SelectItem(0);

            //Show the modal form
            this.ShowDialog();

            return this._styleGalleryItem;
        }

        private void LoadStyleFile()
        {
            //Get the ArcGIS install location
            string sInstall = ESRI.ArcGIS.RuntimeManager.ActiveRuntime.Path;

            //Load the ESRI.ServerStyle file into the SymbologyControl
            this._axSymCtrl.LoadStyleFile(sInstall + "\\Styles\\ESRI.ServerStyle");

        }
    }
}
