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

using ESRI.ArcGIS.Geodatabase;
using ArcDataBinding;

namespace CAGA.Map
{
    /// <summary>
    /// Interaction logic for AttrTableWindow.xaml
    /// </summary>
    public partial class AttrTableWindow : Window
    {
        public AttrTableWindow(ITable attrTable)
        {
            InitializeComponent();
            // Bind dataset to the binding source
            TableWrapper tableWrapper = new ArcDataBinding.TableWrapper(attrTable);
            //this.AttrTableGrid.ItemsSource = tableWrapper;
            //this.AttrTableGrid.Items.Refresh();  
            this.AttrTableGridView.DataSource = tableWrapper;

        }
    }
}
