using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
using System.Xml.Serialization;
using System.Xml.XPath;
using xml_diff.ViewModels;

namespace xml_diff
{
    /// <summary>
    /// XmlDiff01.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class XmlDiff01 : UserControl
    {
        public XmlDiff01()
        {
            InitializeComponent();

            if(App.Current.MainWindow is MainWindow mainWindow)
            {
                (App.Current.Resources["ViewModelProvider"] as ViewModelProvider)!.MainViewModel!.XmlDataInit += () =>
                {
                    this.xXmlDiff.XmlDataInit();
                };

                (App.Current.Resources["ViewModelProvider"] as ViewModelProvider)!.MainViewModel!.FromFileLoaded += (filePath) => {
                    this.xXmlDiff.LoadXmlFile2(filePath, XmlDiffLib.XmlDiffControl.EDiffType.from);
                };
            }
        }

        private void xBrowse_Click(object sender, RoutedEventArgs e)
        {
            string? xmlFilePath = null;
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Xml files (*.xml)|*.xml";
            if (openFileDialog.ShowDialog() == true)
            {
                xmlFilePath = openFileDialog.FileName;
            }
            else
            {
                return;
            }

            this.xPath.Text = xmlFilePath;

            this.xXmlDiff.LoadXmlFile2(xmlFilePath, XmlDiffLib.XmlDiffControl.EDiffType.from);


        }

        private void xXmlDiff_ExpanderChanged(object sender, XmlDiffLib.Models.Group fromGroup, XmlDiffLib.Models.Group matchingGroupByTo)
        {
            if (fromGroup is null || matchingGroupByTo is null) return;
            matchingGroupByTo.IsExpanded = fromGroup.IsExpanded;
        }

        private void xSave_Click(object sender, RoutedEventArgs e)
        {
            string? saveFilePath = null;
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "Xml files (*.xml)|*.xml";
            if (saveFileDialog.ShowDialog() == true)
            {
                saveFilePath = saveFileDialog.FileName;
                this.xXmlDiff.SaveXmlFile(saveFilePath);
            }
        }
    }
}
