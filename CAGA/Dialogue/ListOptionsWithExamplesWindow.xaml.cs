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
    /// Interaction logic for ListOptionsWithExamplesWindow.xaml
    /// </summary>
    public partial class ListOptionsWithExamplesWindow : Window
    {
        private OptionWithExampleListData _optionListData;
        private OptionWithExampleItemData _selectedItem;

        public OptionWithExampleItemData SelectedItem
        {
            get { return _selectedItem; }
        }

        public ListOptionsWithExamplesWindow(OptionWithExampleListData optionListData)
        {
            InitializeComponent();
            this._optionListData = optionListData;
            this.HeadingTB.Text = this._optionListData.Opening;
            this.OptionLB.ItemsSource = this._optionListData.Options;
            this._selectedItem = null;
        }

        private void ListBoxItem_Selected(object sender, RoutedEventArgs e)
        {
            this._selectedItem = ((ListBoxItem)(e.Source)).Content as OptionWithExampleItemData;
            this.Close();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {

        }
    }


    public class OptionWithExampleListData
    {
        private ObservableCollection<OptionWithExampleItemData> _options;

        public OptionWithExampleListData()
        {
            this._options = new ObservableCollection<OptionWithExampleItemData>();
        }

        //string of words spoken with dialogue window opening
        public string Opening { get; set; }
        public ObservableCollection<OptionWithExampleItemData> Options
        {
            get
            {
                return this._options;
            }
        }

        public void AddOption(string title, string description, string image)
        {
            this._options.Add(new OptionWithExampleItemData(title, description, image));
        }

        public void AddOption(OptionWithExampleItemData option)
        {
            this._options.Add(option);
        }

    }

    /// <summary>
    /// data class
    /// </summary>
    public class OptionWithExampleItemData : INotifyPropertyChanged
    {
        private string _title;
        private string _description;
        private string _exampleImage;
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
        /// Gets or sets the title.
        /// </summary>
        public string ExampleImage
        {
            get
            {
                return this._exampleImage;
            }
            set
            {
                this._exampleImage = value;
                NotifyPropertyChanged("ExampleImage");
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

        public OptionWithExampleItemData(string title, string description, string image)
        {
            this.Title = title;
            this.Description = description;
            this.ExampleImage = image;
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
