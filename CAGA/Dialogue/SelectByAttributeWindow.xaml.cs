using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

using CAGA.Map;

namespace CAGA.Dialogue
{
    /// <summary>
    /// Interaction logic for SelectByAttributeWindow.xaml
    /// </summary>
    public partial class SelectByAttributeWindow : Window
    {
        private ArcMapManager _mapMgr;
        private ArrayList _layerNames;
        private string selectedLayerName;
        private ArrayList _fieldNames;
        private string _whereClause;

        public string WhereClause
        {
            get { return _whereClause; }
        }

        public string LayerName
        {
            get { return selectedLayerName; }
        }

        public SelectByAttributeWindow(ArcMapManager mapMgr)
        {
            InitializeComponent();
            selectedLayerName = "";
            _mapMgr = mapMgr;
            _layerNames = _mapMgr.GetLayerNames();
            LayerNameCB.ItemsSource = _layerNames;
        }

        private void addContentToWhereClause(string content)
        {
            if (WhereClauseTB.Text == "")
            {
                WhereClauseTB.Text = content;
            }
            else
            {
                WhereClauseTB.SelectedText = " " + content + " ";
            }
            Dispatcher.BeginInvoke((ThreadStart)delegate
            {
                WhereClauseTB.Focus();

                WhereClauseTB.SelectionStart = WhereClauseTB.Text.Length;
                //Keyboard.Focus(WhereClause);
            });
        }

        private void EqualBtn_Click(object sender, RoutedEventArgs e)
        {
            addContentToWhereClause("=");   
        }

        private void NotEqualBtn_Click(object sender, RoutedEventArgs e)
        {
            addContentToWhereClause("<>");   
        }

        private void GreaterBtn_Click(object sender, RoutedEventArgs e)
        {
            addContentToWhereClause(">");   

        }

        private void GreaterEqualBtn_Click(object sender, RoutedEventArgs e)
        {
            addContentToWhereClause(">=");   
        }

        private void LessBtn_Click(object sender, RoutedEventArgs e)
        {
            addContentToWhereClause("<");   
        }

        private void LessEqualBtn_Click(object sender, RoutedEventArgs e)
        {
            addContentToWhereClause("<=");  
        }

        private void AndBtn_Click(object sender, RoutedEventArgs e)
        {
            addContentToWhereClause("AND");  
        }

        private void OrBtn_Click(object sender, RoutedEventArgs e)
        {
            addContentToWhereClause("OR");  
        }

        private void NotBtn_Click(object sender, RoutedEventArgs e)
        {
            addContentToWhereClause("NOT");  
        }

        private void LikeBtn_Click(object sender, RoutedEventArgs e)
        {
            addContentToWhereClause("LIKE");  
        }

        private void IsBtn_Click(object sender, RoutedEventArgs e)
        {
            addContentToWhereClause("IS");  
        }

        private void QuestionBtn_Click(object sender, RoutedEventArgs e)
        {
            addContentToWhereClause("?");  
        }

        private void StarBtn_Click(object sender, RoutedEventArgs e)
        {
            addContentToWhereClause("*");  
        }

        private void BracketsBtn_Click(object sender, RoutedEventArgs e)
        {
            addContentToWhereClause("()");  
        }

        private void OKBtn_Click(object sender, RoutedEventArgs e)
        {
            _whereClause = WhereClauseTB.Text;
            if (selectedLayerName != "" && _whereClause != "")
            {
                _mapMgr.SelectFeaturesByAttributes(selectedLayerName, _whereClause);
            }
            this.Close();
        }

        private void LayerNameCB_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            selectedLayerName = e.AddedItems[0].ToString();
            _fieldNames = _mapMgr.GetFieldNames(selectedLayerName);
            FieldsLB.ItemsSource = _fieldNames;
            SelectClauseTB.Text = "SELECT * FROM " + selectedLayerName + " WHERE: ";
        }

        private void FieldItem_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            string fieldName = (sender as ListBoxItem).Content.ToString();
            if (WhereClauseTB.Text == "")
            {
                WhereClauseTB.Text = fieldName;
            }
            else
            {
                WhereClauseTB.SelectedText = " " + fieldName + " ";
            }
            Dispatcher.BeginInvoke((ThreadStart)delegate
            {
                WhereClauseTB.Focus();
                
                WhereClauseTB.SelectionStart = WhereClauseTB.Text.Length;
                //Keyboard.Focus(WhereClause);
            });
        }
    }
}
