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

using CAGA.NUI;

namespace CAGA
{
    /// <summary>
    /// Interaction logic for SpeechSimWindow.xaml
    /// </summary>
    public partial class SpeechSimWindow : Window
    {
        private ArrayList scriptDataList;
        private KinectManager _kinectMgr;
        public SpeechSimWindow(KinectManager kinectMgr)
        {
            this._kinectMgr = kinectMgr;
            InitializeComponent();
        }

        private void AddNewSimInput_Click(object sender, RoutedEventArgs e)
        {
            if (NewSimInput.Text.Length > 0)
            {
                scriptDataList.Add(NewSimInput.Text);
                ApplyDataBinding();
                ScriptList.SelectedItem = ScriptList.Items[ScriptList.Items.Count - 1];
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            scriptDataList = LoadListBoxData();
            // Bind ArrayList with the ListBox
            ScriptList.ItemsSource = scriptDataList;
            NewSimInput.Focus();
        }

        private ArrayList LoadListBoxData()
        {
            ArrayList itemsList = new ArrayList();
            itemsList.Add("I want to generate a map of parcels within a region");
            itemsList.Add("it is city of Oleader");
            return itemsList;
        }

        /// <summary>
        /// Refreshes data binding
        /// </summary>
        private void ApplyDataBinding()
        {
            ScriptList.ItemsSource = null;
            // Bind ArrayList with the ListBox
            ScriptList.ItemsSource = scriptDataList;
        }

        private void SimulateBtn_Click(object sender, RoutedEventArgs e)
        {
            if (ScriptList.SelectedValue != null)
            {
                string simText = ScriptList.SelectedValue.ToString();
                this._kinectMgr.speechRecognizer.Simulate(simText);
            }
        }

    }
}
