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

namespace CAGA.Dialogue
{
    /// <summary>
    /// Interaction logic for OptionListWindow.xaml
    /// </summary>
    public partial class ListPlainOptionsWindow : Window
    {
        private PlainOptionListData _optionListData;
        private PlainOptionItemData _selectedItem;

        public PlainOptionItemData SelectedItem
        {
            get { return _selectedItem; }
        }

        public ListPlainOptionsWindow(PlainOptionListData optionListData)
        {
            InitializeComponent();
            this._optionListData = optionListData;
            this.HeadingTB.Text = this._optionListData.Opening;
            this.OptionLB.ItemsSource = this._optionListData.Options;
            this._selectedItem = null;
            
        }

        private void ListBoxItem_Selected(object sender, RoutedEventArgs e)
        {
            this._selectedItem = ((ListBoxItem)(e.Source)).Content as PlainOptionItemData;
            this.Close();
        }

        public PlainOptionItemData GetOption()
        {
            this._selectedItem = null;

            this.ShowDialog();

            return _selectedItem;
        }

        

    }

    public class PlainOptionListData
    {
        private ObservableCollection<PlainOptionItemData> _options;

        public PlainOptionListData()
        {
            this._options = new ObservableCollection<PlainOptionItemData>();
        }

        //string of words spoken with dialogue window opening
        public string Opening {get; set;}

        public ObservableCollection<PlainOptionItemData> Options
        {
            get
            {
                return this._options;
            }
        }

        public void AddOption(string title, string description)
        {
            this._options.Add(new PlainOptionItemData(title, description));
        }

        public void AddOption(PlainOptionItemData option)
        {
            this._options.Add(option);
        }

    }

    /// <summary>
    /// data class
    /// </summary>
    public class PlainOptionItemData : INotifyPropertyChanged
    {
        private string _title;
        private string _description;
        /// <summary>
        /// Gets or sets the title.
        /// </summary>
        public string Title 
        {
            get
            {
                return this._title;
            }
            set
            {
                this._title = value;
                NotifyPropertyChanged("Title");
            }
        }

        /// <summary>
        /// Gets or sets the description.
        /// </summary>
        public string Description 
        {
            get
            {
                return this._description;
            }
            set
            {
                this._description = value;
                NotifyPropertyChanged("Description");
            }
        }

        /// <summary>
        /// ToString override, often used in bindings.
        /// </summary>
        /// <returns>title.</returns>
        public override string ToString()
        {
            return this.Title;
        }

        public PlainOptionItemData(string title, string description)
        {
            this.Title = title;
            this.Description = description;
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
