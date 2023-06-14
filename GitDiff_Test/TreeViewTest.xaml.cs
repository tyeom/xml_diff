using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Xml;
using GitDiff_Test.Models;

namespace GitDiff_Test
{
    /// <summary>
    /// TreeViewTest.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class TreeViewTest : Window
    {
        public TreeViewTest()
        {
            InitializeComponent();

            this.Loaded += TreeViewTest_Loaded;
        }

        private void TreeViewTest_Loaded(object sender, RoutedEventArgs e)
        {
            this.LoadXmlFile2("C:\\Users\\banaple_43\\Documents\\카카오톡 받은 파일\\file111.xml");
        }

        public void LoadXmlFile2(string filePath)
        {
            try
            {
                var xmlDoc = new XmlDocument();
                xmlDoc.Load(filePath);

                var chidNodes = xmlDoc.ChildNodes;
                var rootNode = chidNodes.Cast<XmlNode>().FirstOrDefault(p => p.Name.ToLower() == "root");
                if (rootNode is null)
                {
                    throw new Exception("root 노드를 찾을 수 없습니다.");
                }

                Models.Root root = new Models.Root();
                root.Groups = new List<Group>();

                this.xTreeView.Items.Clear();

                foreach (XmlNode groupNode in rootNode.ChildNodes)
                {
                    if (groupNode.Name != "group")
                        continue;

                    var group = ParseGroup(groupNode, null);

                    root.Groups.Add(group);
                }
            }
            catch
            {
                throw;
            }
        }

        private Group ParseGroup(XmlNode xmlNode, TreeViewItem parentGroup)
        {
            Group group = new Group();
            group.Processes = new ObservableCollection<Process>();
            group.NestedGroup = new ObservableCollection<Group>();
            group.Name = xmlNode.Attributes["name"].Value;

            TreeViewItem groupNode = new TreeViewItem();
            if (parentGroup is null)
            {
                groupNode.Header = group;
                groupNode.DataContext = group;
                groupNode.ItemContainerStyle = this.Resources["sTreeViewItem"] as Style;
                this.xTreeView.Items.Add(groupNode);
            }
            else
            {
                groupNode.Header = group;
                groupNode.DataContext = group;
                groupNode.ItemContainerStyle = this.Resources["sTreeViewItem"] as Style;

                group.ParentTreeViewItem = parentGroup;
                group.ParentGroup = parentGroup.DataContext as Group;
                parentGroup.Items.Add(groupNode);
            }

            foreach (XmlNode node in xmlNode.ChildNodes)
            {
                if (node.Name == "group")
                {
                    Group nestedGroup = ParseGroup(node, groupNode);
                    group.NestedGroup.Add(nestedGroup);
                }
                else if (node.Name == "process")
                {
                    Process process = ParseProcess(node, groupNode);
                    group.Processes.Add(process);
                }
            }

            return group;
        }

        private Process ParseProcess(XmlNode xmlNode, TreeViewItem treeViewItem)
        {
            Process process = new Process();
            process.Items = new ObservableCollection<Item>();
            process.Name = xmlNode.Attributes["name"].Value;
            process.Type = xmlNode.Attributes["type"].Value;
            process.ParentTreeViewItem = treeViewItem;
            process.ParentGroup = treeViewItem.DataContext as Group;

            foreach (XmlNode node in xmlNode.ChildNodes)
            {
                if (node.Name == "item")
                {
                    Item item = new Item();
                    item.Name = node.Attributes["name"].Value;
                    item.Value = node.Attributes["value"].Value;
                    item.Type = node.Attributes["type"].Value;

                    process.Items.Add(item);
                }
            }

            TreeViewItem processNode = new TreeViewItem();
            processNode.Header = process;
            processNode.DataContext = process;
            processNode.ItemContainerStyle = this.Resources["sTreeViewItem"] as Style;
            treeViewItem.Items.Add(processNode);

            return process;
        }
    }
}
