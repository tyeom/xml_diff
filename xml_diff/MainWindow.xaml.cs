using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml;
using xml_diff.ViewModels;

namespace xml_diff
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public delegate void XmlDataInitEventHandler();
        public event XmlDataInitEventHandler XmlDataInit;

        public delegate void FromFileLoadedEventHandler(string filePath);
        public event FromFileLoadedEventHandler FromFileLoaded;

        public delegate void ToFileLoadedEventHandler(string filePath);
        public event ToFileLoadedEventHandler ToFileLoaded;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void xBrowse_Click(object sender, RoutedEventArgs e)
        {
            string? xmlFilePath = null;
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Xml files (*.xml)|*.xml";
            if (openFileDialog.ShowDialog() == true)
            {
                this.xPath.Text = openFileDialog.FileName;
                xmlFilePath = openFileDialog.FileName;

                if(XmlDataInit is not null)
                {
                    XmlDataInit();
                }

                if(FromFileLoaded is not null)
                {
                    FromFileLoaded(xmlFilePath);
                }

                if (ToFileLoaded is not null)
                {
                    ToFileLoaded(xmlFilePath);
                }
            }
        }
    }
}
