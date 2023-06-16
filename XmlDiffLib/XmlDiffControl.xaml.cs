using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml;
using System.Xml.Serialization;
using XmlDiffLib.Models;
using XmlDiffLib.ViewModels;
using XmlDiffLib.Extensions;
using static XmlDiffLib.XmlDiffControl;
using Microsoft.Win32;
using System.Xml.Linq;

namespace XmlDiffLib
{
    /// <summary>
    /// XmlDiffControl.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class XmlDiffControl : UserControl
    {
        public enum EDiffType
        {
            from,
            to
        }

        public enum EModeType
        {
            Write,
            ReadOnly
        }

        public delegate void ExpanderChangedEventHandler(object sender, Group fromGroup, Group matchingGroupByTo);
        public event ExpanderChangedEventHandler ExpanderChanged;

        private Root? _rootModel;
        private static Root?[] diffRootArr = new Root[2];
        private static TreeView?[] diffTreeViewArr = new TreeView[2];
        private static Clipboard? _clipboard;
        private TreeViewItem? _tempRightClickNode;

        //private XmlDiffViewModel _xmlDiffViewModel;

        public XmlDiffControl()
        {
            InitializeComponent();

            //_xmlDiffViewModel = new XmlDiffViewModel();
            //this.DataContext = _xmlDiffViewModel;

            TreeViewExtensions.TreeViewItemStyle = this.Resources["sTreeViewItem"] as Style;
        }

        public EDiffType DiffType
        {
            get { return (EDiffType)base.GetValue(DiffTypeProperty); }
            set { base.SetValue(DiffTypeProperty, value); }
        }

        public static readonly DependencyProperty DiffTypeProperty =
          DependencyProperty.Register("DiffType",
              typeof(EDiffType),
              typeof(XmlDiffControl),
              new PropertyMetadata(EDiffType.from));

        public EModeType Mode
        {
            get { return (EModeType)base.GetValue(ModeProperty); }
            set { base.SetValue(ModeProperty, value); }
        }

        public static readonly DependencyProperty ModeProperty =
          DependencyProperty.Register("Mode",
              typeof(EModeType),
              typeof(XmlDiffControl),
              new PropertyMetadata(EModeType.Write, OnModeChanged));

        public bool OnlyProcessDisplay
        {
            get { return (bool)base.GetValue(OnlyProcessDisplayProperty); }
            set { base.SetValue(OnlyProcessDisplayProperty, value); }
        }

        public static readonly DependencyProperty OnlyProcessDisplayProperty =
          DependencyProperty.Register("OnlyProcessDisplay",
              typeof(bool),
              typeof(XmlDiffControl),
              new PropertyMetadata(false));

        public Brush WithoutColor
        {
            get { return (Brush)base.GetValue(WithoutColorProperty); }
            set { base.SetValue(WithoutColorProperty, value); }
        }

        public static readonly DependencyProperty WithoutColorProperty =
          DependencyProperty.Register("WithoutColor",
              typeof(Brush),
              typeof(XmlDiffControl),
              new PropertyMetadata((SolidColorBrush)(new BrushConverter().ConvertFrom("#FFA9A9A9")!)));

        public Brush DupColor
        {
            get { return (Brush)base.GetValue(DupColorProperty); }
            set { base.SetValue(DupColorProperty, value); }
        }

        public static readonly DependencyProperty DupColorProperty =
          DependencyProperty.Register("DupColor",
              typeof(Brush),
              typeof(XmlDiffControl),
              new PropertyMetadata((SolidColorBrush)(new BrushConverter().ConvertFrom("#FF008000")!)));

        public Brush DiffColor
        {
            get { return (Brush)base.GetValue(DiffColorProperty); }
            set { base.SetValue(DiffColorProperty, value); }
        }

        public static readonly DependencyProperty DiffColorProperty =
          DependencyProperty.Register("DiffColor",
              typeof(Brush),
              typeof(XmlDiffControl),
              new PropertyMetadata((SolidColorBrush)(new BrushConverter().ConvertFrom("#FFFF0000")!)));

        public Brush UncheckedColor
        {
            get { return (Brush)base.GetValue(UncheckedColorProperty); }
            set { base.SetValue(UncheckedColorProperty, value); }
        }

        public static readonly DependencyProperty UncheckedColorProperty =
          DependencyProperty.Register("UncheckedColor",
              typeof(Brush),
              typeof(XmlDiffControl),
              new PropertyMetadata((SolidColorBrush)(new BrushConverter().ConvertFrom("#FF0701FE")!)));

        public double ScrollVerticalOffset
        {
            get { return (double)base.GetValue(ScrollVerticalOffsetProperty); }
            set { base.SetValue(ScrollVerticalOffsetProperty, value); }
        }

        public static readonly DependencyProperty ScrollVerticalOffsetProperty =
          DependencyProperty.Register("ScrollVerticalOffset",
              typeof(double),
              typeof(XmlDiffControl),
              new FrameworkPropertyMetadata(Double.NaN, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnScrollVerticalOffsetChanged));

        public void LoadXmlFile(string filePath, EDiffType diffType)
        {
            try
            {
                DiffType = diffType;

                Root? root;
                var serializer = new XmlSerializer(typeof(Root));
                using (var stream = new FileStream(filePath, FileMode.Open))
                {
                    root = serializer.Deserialize(stream) as Root;
                }

                // XPath 표현식으로 특정 속성 추출
                //XPathNavigator navigator = ((IXPathNavigable)root).CreateNavigator();
                //string xpathExpression = "root[1]/group[@name=\"Group2\"][1]/process[@name=\"Process3\"][@type=\"pt2\"][1]/item[@name=\"p33\"][@value=\"0.1\"][@type=\"vt22\"][1]";
                //XPathNodeIterator iterator = navigator.Select(xpathExpression);

                //while (iterator.MoveNext())
                //{
                //    string value = iterator.Current.Value;
                //    Console.WriteLine("Value: " + value);
                //}

                //_xmlDiffViewModel.FilePath = filePath;

                _rootModel = root;
                if (diffType == EDiffType.from)
                {
                    diffRootArr[0] = root;
                }
                else
                {
                    diffRootArr[1] = root;
                }

                // start xml diff
                if (diffRootArr[0] is not null &&
                    diffRootArr[1] is not null)
                {
                    XmlDiffHelper.Diff2(diffRootArr[0], diffRootArr[1]);
                }

                //_xmlDiffViewModel.Root = root;
            }
            catch
            {
                throw;
            }
        }

        public void XmlDataInit()
        {
            this.xTreeView.Items.Clear();

            _rootModel = null;

            diffRootArr[0] = null;
            diffRootArr[1] = null;

            diffTreeViewArr[0] = null;
            diffTreeViewArr[1] = null;
        }

        public void LoadXmlFile2(string filePath, EDiffType diffType)
        {
            try
            {
                DiffType = diffType;

                var xmlDoc = new XmlDocument();
                xmlDoc.Load(filePath);

                var chidNodes = xmlDoc.ChildNodes;
                var rootNode = chidNodes.Cast<XmlNode>().FirstOrDefault(p => p.Name.ToLower() == "root");
                if (rootNode is null)
                {
                    throw new Exception("root 노드를 찾을 수 없습니다.");
                }

                Root root = new Root();
                root.Groups = new List<Group>();

                this.xTreeView.Items.Clear();

                foreach (XmlNode groupNode in rootNode.ChildNodes)
                {
                    if (groupNode.Name != "group")
                        continue;

                    var group = ParseGroup(groupNode, null);

                    root.Groups.Add(group);
                }

                //_xmlDiffViewModel.FilePath = filePath;

                if (diffType == EDiffType.from)
                {
                    diffRootArr[0] = root;
                    diffTreeViewArr[0] = this.xTreeView;
                }
                else
                {
                    diffRootArr[1] = root;
                    diffTreeViewArr[1] = this.xTreeView;
                }
                _rootModel = root;
                this.xTreeView.Tag = _rootModel;

                // start xml diff
                if (diffTreeViewArr[0] is not null &&
                    diffTreeViewArr[1] is not null)
                {
                    XmlDiffHelper.Diff2(diffRootArr[0], diffRootArr[1]);
                }
            }
            catch
            {
                throw;
            }
        }

        public void SaveXmlFile(string filePath)
        {
            this.xTreeView.SaveToXml(filePath);
        }

        private int? FindIndexTreeViewItemByTag(object tag, ItemsControl itemsControl)
        {
            for (int i = 0; i < itemsControl.Items.Count; i ++)
            {
                if (itemsControl.Items[i] is TreeViewItem treeViewItem && treeViewItem.Tag?.ToString() == tag?.ToString())
                    return i;
            }

            return null;
        }

        private TreeViewItem? FindTreeViewItemByTag(object tag, ItemsControl itemsControl)
        {
            for (int i = 0; i < itemsControl.Items.Count; i++)
            {
                if (itemsControl.Items[i] is TreeViewItem treeViewItem && treeViewItem.Tag?.ToString() == tag?.ToString())
                    return treeViewItem;
            }

            return null;
        }

        private TreeViewItem? FindTreeViewItemByIndex(int index, ItemsControl itemsControl)
        {
            if(itemsControl.Items.Count > index)
            {
                return itemsControl.Items[index] as TreeViewItem;
            }
            else
            {
                return null;
            }
        }

        public void SynchronizeTreeViewByLeft()
        {
            for (int i = 0; i < diffTreeViewArr[0]!.Items.Count; i ++)
            {
                if (diffTreeViewArr[0]!.Items[i] is TreeViewItem treeViewItem1)
                {
                    if(treeViewItem1.Tag is not null && treeViewItem1.Tag.ToString() == "003000.000000")
                    {
                        //
                    }

                    // 오른쪽 트리뷰에서 Tag(Id)에 해당 되는 위치를 찾는다.
                    int? index = this.FindIndexTreeViewItemByTag(treeViewItem1.Tag, diffTreeViewArr[1]!);
                    if(index is not null && i == index)
                    {
                        // 위치 동일, 하위 노드 높이 동기 처리
                        this.SynchronizeTreeViewItems(diffTreeViewArr[0]!.Items[i] as TreeViewItem, diffTreeViewArr[1]!.Items[index.Value] as TreeViewItem);

                    }
                    else if(index is not null)
                    {
                        //TreeViewItem emptyNodeItem = new();
                        //// 마지막 노드는 표시 되지 않도록 트리거 설정이 되어 있어 임의로 자식 노드를 추가
                        //Group emptyGroup = new() { IsEmpty = true };
                        //emptyNodeItem.Items.Add(new());
                        //emptyNodeItem.Header = emptyGroup;
                        //emptyNodeItem.DataContext = emptyGroup;
                        //emptyNodeItem.ItemContainerStyle = this.Resources["sTreeViewItem"] as Style;

                        //diffTreeViewArr[0]!.Items.Insert(i, emptyNodeItem);
                        //diffRootArr[0].Groups.Insert(i, emptyGroup);


                        // 빈 노드 추가
                        TreeViewItem? treeViewItem = this.FindTreeViewItemByIndex(i, diffTreeViewArr[1]!);
                        if (treeViewItem is null) return;

                        var emptyTreeViewItemTarget = this.DeepCopyTreeViewItem(treeViewItem, null, null, true);
                        Group emptyGroup = new() { IsEmpty = true, MatchingGroupByTo = treeViewItem.Header as Group };
                        (treeViewItem.Header as Group).DiffFromGroup = emptyGroup;
                        emptyGroup.IsExpanded = emptyGroup.MatchingGroupByTo.IsExpanded;
                        emptyTreeViewItemTarget.Header = emptyGroup;
                        emptyTreeViewItemTarget.DataContext = emptyGroup;
                        emptyTreeViewItemTarget.ItemContainerStyle = this.Resources["sTreeViewItem"] as Style;

                        diffTreeViewArr[0]!.Items.Insert(i, emptyTreeViewItemTarget);
                        diffRootArr[0].Groups.Insert(i, emptyGroup);

                        i++;
                    }
                }
            }
        }

        public void SynchronizeTreeViewByRight()
        {
            for (int i = 0; i < diffTreeViewArr[1]!.Items.Count; i++)
            {
                if (diffTreeViewArr[1]!.Items[i] is TreeViewItem treeViewItem1)
                {
                    if (treeViewItem1.Tag is not null && treeViewItem1.Tag.ToString() == "001000.000000")
                    {
                        //
                    }

                    // 왼쪽 트리뷰에서 Tag(Id)에 해당 되는 위치를 찾는다.
                    int? index = this.FindIndexTreeViewItemByTag(treeViewItem1.Tag, diffTreeViewArr[0]!);
                    if (index is not null && i == index)
                    {
                        // 위치 동일, 하위 노드 높이 동기 처리
                        this.SynchronizeTreeViewItems(diffTreeViewArr[1]!.Items[i] as TreeViewItem, diffTreeViewArr[0]!.Items[index.Value] as TreeViewItem);

                    }
                    else if (index is not null)
                    {
                        //TreeViewItem emptyNodeItem = new();
                        //// 마지막 노드는 표시 되지 않도록 트리거 설정이 되어 있어 임의로 자식 노드를 추가
                        //Group emptyGroup = new() { IsEmpty = true };
                        //emptyNodeItem.Items.Add(new());
                        //emptyNodeItem.Header = emptyGroup;
                        //emptyNodeItem.DataContext = emptyGroup;
                        //emptyNodeItem.ItemContainerStyle = this.Resources["sTreeViewItem"] as Style;

                        //diffTreeViewArr[1]!.Items.Insert(i, emptyNodeItem);
                        //diffRootArr[1].Groups.Insert(i, emptyGroup);


                        // 빈 노드 추가
                        TreeViewItem? treeViewItem = this.FindTreeViewItemByIndex(i, diffTreeViewArr[0]!);
                        if (treeViewItem is null) return;

                        var emptyTreeViewItemTarget = this.DeepCopyTreeViewItem(treeViewItem, null, null, true);
                        Group emptyGroup = new() { IsEmpty = true, DiffFromGroup = treeViewItem.Header as Group };
                        (treeViewItem.Header as Group).MatchingGroupByTo = emptyGroup;
                        emptyGroup.IsExpanded = emptyGroup.DiffFromGroup.IsExpanded;
                        emptyTreeViewItemTarget.Header = emptyGroup;
                        emptyTreeViewItemTarget.DataContext = emptyGroup;
                        emptyTreeViewItemTarget.ItemContainerStyle = this.Resources["sTreeViewItem"] as Style;

                        diffTreeViewArr[1]!.Items.Insert(i, emptyTreeViewItemTarget);
                        diffRootArr[1].Groups.Insert(i, emptyGroup);
                        i++;
                    }
                }
            }
        }

        private void SynchronizeTreeViewItems(TreeViewItem left, TreeViewItem right)
        {
            for (int i = 0; i < left.Items.Count; i ++)
            {
                if (left.Items[i] is TreeViewItem treeViewItem1)
                {
                    if(treeViewItem1.Header is Item) continue;
                    if (treeViewItem1.Tag is null) continue;
                    // 오른쪽 트리뷰에서 Tag(Id)에 해당 되는 위치를 찾는다.
                    int? index = this.FindIndexTreeViewItemByTag(treeViewItem1.Tag, right);
                    if (index is not null && i == index)
                    {
                        // 위치 동일, 하위 노드 높이 동기 처리
                        this.SynchronizeTreeViewItems(left.Items[i] as TreeViewItem, right.Items[i] as TreeViewItem);

                    }
                    else if (index is not null)
                    {
                        //TreeViewItem emptyNodeItem = new();
                        //// 마지막 노드는 표시 되지 않도록 트리거 설정이 되어 있어 임의로 자식 노드를 추가
                        //Process emptyProcess = new() { IsEmpty = true };
                        //emptyNodeItem.Items.Add(new());
                        //emptyNodeItem.Header = emptyProcess;
                        //emptyNodeItem.DataContext = emptyProcess;
                        //emptyNodeItem.ItemContainerStyle = this.Resources["sTreeViewItem"] as Style;

                        //left.Items.Insert(i, emptyNodeItem);
                        //var parentGroup = left.Header as Group;
                        //parentGroup.Processes.Insert(i, emptyProcess);


                        // 빈 노드 추가
                        TreeViewItem? treeViewItem = this.FindTreeViewItemByIndex(i, right);
                        if (treeViewItem is null) return;

                        var emptyTreeViewItemTarget = this.DeepCopyTreeViewItem(treeViewItem, null, null, true);

                        left.Items.Insert(i, emptyTreeViewItemTarget);
                        var parentGroup = left.Header as Group;
                        if(emptyTreeViewItemTarget.Header is Group)
                        {
                            var group = emptyTreeViewItemTarget.Header as Group;
                            group.IsEmpty = true;
                            group.IsExpanded = treeViewItem.IsExpanded;
                            parentGroup.NestedGroup.Add(group);
                        }
                        else
                        {
                            var process = emptyTreeViewItemTarget.Header as Process;
                            process.IsEmpty = true;
                            parentGroup.Processes.Insert(i, process);
                        }

                        i++;
                    }
                }
            }
        }




        private Group ParseGroup(XmlNode xmlNode, TreeViewItem parentGroup)
        {
            Group group = new Group();
            group.Processes = new ObservableCollection<Process>();
            group.NestedGroup = new ObservableCollection<Group>();
            group.Id = xmlNode.Attributes["id"].Value;
            group.Name = xmlNode.Attributes["name"].Value;

            TreeViewItem groupNode = new TreeViewItem();
            if (parentGroup is null)
            {
                groupNode.Header = group;
                groupNode.DataContext = group;
                groupNode.Tag = group.Id;
                groupNode.ItemContainerStyle = this.Resources["sTreeViewItem"] as Style;
                this.xTreeView.Items.Add(groupNode);
            }
            else
            {
                groupNode.Header = group;
                groupNode.DataContext = group;
                groupNode.Tag = group.Id;
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
            process.Id = xmlNode.Attributes["id"].Value;
            process.Name = xmlNode.Attributes["name"].Value;
            process.Type = xmlNode.Attributes["type"].Value;
            process.ParentTreeViewItem = treeViewItem;
            process.ParentGroup = treeViewItem.DataContext as Group;

            TreeViewItem processNode = new TreeViewItem();
            processNode.Header = process;
            processNode.DataContext = process;
            processNode.Tag = process.Id;
            processNode.ItemContainerStyle = this.Resources["sTreeViewItem"] as Style;
            treeViewItem.Items.Add(processNode);

            foreach (XmlNode node in xmlNode.ChildNodes)
            {
                if (node.Name == "item")
                {
                    Item item = new Item();
                    item.Name = node.Attributes["name"].Value;
                    item.Value = node.Attributes["value"].Value;
                    item.Type = node.Attributes["type"].Value;

                    process.Items.Add(item);

                    // Add Item Tree node
                    TreeViewItem itemNode = new TreeViewItem();
                    itemNode.Header = item;
                    itemNode.DataContext = item;
                    itemNode.ItemContainerStyle = this.Resources["sTreeViewItem"] as Style;
                    processNode.Items.Add(itemNode);
                }
            }

            return process;
        }

        private void SerializeToFile<T>(string xmlFilePath, T objectToSerialize) where T : class
        {
            using (var writer = new StreamWriter(xmlFilePath))
            {
                // Do this to avoid the serializer inserting default XML namespaces.
                var namespaces = new XmlSerializerNamespaces();
                namespaces.Add(string.Empty, string.Empty);

                var serializer = new XmlSerializer(objectToSerialize.GetType());
                serializer.Serialize(writer, objectToSerialize, namespaces);
            }
        }

        public static void OnModeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            XmlDiffControl? ctl = d as XmlDiffControl;
            if (ctl == null) return;

            ctl.XmlDataInit();
        }

        public static void OnScrollVerticalOffsetChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            XmlDiffControl? ctl = d as XmlDiffControl;
            if (ctl == null) return;

            if (ctl.xTreeView.HasItems is false) return;
            Decorator border = VisualTreeHelper.GetChild(ctl.xTreeView, 0) as Decorator;
            // Get scrollviewer
            ScrollViewer scrollViewer = border.Child as ScrollViewer;
            if (scrollViewer is not null)
            {
                scrollViewer.ScrollToVerticalOffset((double)e.NewValue);
            }

            //ctl.xScrollViewer.ScrollToVerticalOffset((double)e.NewValue);
            ctl.isScrollingByWheel = false;
        }

        private bool isScrollingByWheel = false;
        private void xScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            //// 스크롤 위치 복원
            //if (isScrollingByWheel is false &&
            //    this.xScrollViewer.Tag != null &&
            //    e.VerticalChange <= -(double)this.xScrollViewer.Tag)
            //{
            //    this.xScrollViewer.ScrollToVerticalOffset((double)this.xScrollViewer.Tag);
            //}

            //// 스크롤 위치 기록
            //if (e.VerticalChange != 0)
            //{
            //    this.xScrollViewer.Tag = e.VerticalOffset;
            //}

            ScrollVerticalOffset = e.VerticalOffset;
        }

        private void xScrollViewer_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            isScrollingByWheel = true;
            ScrollViewer scv = (ScrollViewer)sender;
            scv.ScrollToVerticalOffset(scv.VerticalOffset - e.Delta);
            e.Handled = true;
        }

        private void TreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if(e.NewValue is TreeViewItem treeViewItem &&
                treeViewItem.Tag is null)
            {
                treeViewItem.IsSelected = false;
            }
            if (e.NewValue is TreeViewItem treeViewItem2)
            {
                if((treeViewItem2.Header is Group group &&
                    group.IsEmpty is true) ||
                    (treeViewItem2.Header is Process process &&
                    process.IsEmpty is true))
                    treeViewItem2.IsSelected = false;
            }

            if (e.NewValue is TreeViewItem treeViewItem3)
            {
                treeViewItem3.BringIntoView();
                //Point relativePosition = treeViewItem3.TranslatePoint(new Point(0, 0), this.xScrollViewer);
                //this.xScrollViewer.ScrollToVerticalOffset(relativePosition.Y);
            }
        }

        private void TreeView_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (Mode == EModeType.ReadOnly)
            {
                return;
            }

            _tempRightClickNode = this.FindAncestor<TreeViewItem>((DependencyObject)e.OriginalSource);
        }

        private void xTreeView_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (Mode == EModeType.ReadOnly)
            {
                return;
            }

            var rightClickTreeViewItem = this.FindAncestor<TreeViewItem>((DependencyObject)e.OriginalSource);
            if (rightClickTreeViewItem is null) return;

            if(rightClickTreeViewItem.Header is Group group)
            {
                group.IsExpanded = rightClickTreeViewItem.IsExpanded;
                if (DiffType == EDiffType.from)
                {
                    if (ExpanderChanged is not null)
                    {
                        ExpanderChanged(this, group, group.MatchingGroupByTo);
                    }
                }
                else
                {
                    if (ExpanderChanged is not null)
                    {
                        ExpanderChanged(this, group.DiffFromGroup, group);
                    }
                }
            }

            if (DiffType == EDiffType.to)
            {
                if (rightClickTreeViewItem is not null &&
                    rightClickTreeViewItem.DataContext is Process toProcess)
                {
                    if (toProcess.ParentGroup.IsEmpty is true) return;

                    PropertyDetailWindow detailWindow = new();
                    detailWindow.ToProcess = toProcess;
                    detailWindow.FromProcess = toProcess.DiffFromProcess;
                    detailWindow.ShowDialog();

                    XmlDiffHelper.Diff2(diffRootArr[0], diffRootArr[1]);
                }
            }
            else
            {
                if (rightClickTreeViewItem is not null &&
                    rightClickTreeViewItem.DataContext is Process fromProcess)
                {
                    if(fromProcess.ParentGroup is not null &&
                        fromProcess.ParentGroup.MatchingGroupByTo is not null)
                    {
                        if (fromProcess.ParentGroup.IsEmpty is true) return;

                        var toProcess = fromProcess.ParentGroup.MatchingGroupByTo.Processes.FirstOrDefault(p => p.Id == fromProcess.Id);

                        PropertyDetailWindow detailWindow = new();
                        detailWindow.ToProcess = toProcess;
                        detailWindow.FromProcess = fromProcess;
                        detailWindow.ShowDialog();

                        XmlDiffHelper.Diff2(diffRootArr[0], diffRootArr[1]);
                    }
                }
            }
        }

        private void xBtnExpander_Click(object sender, RoutedEventArgs e)
        {
            if(sender is ToggleButton toggleBtn &&
                toggleBtn.DataContext is Group group)
            {
                if (DiffType == EDiffType.from)
                {
                    if (ExpanderChanged is not null)
                    {
                        ExpanderChanged(this, group, group.MatchingGroupByTo);
                    }
                }
                else
                {
                    if (ExpanderChanged is not null)
                    {
                        ExpanderChanged(this, group.DiffFromGroup, group);
                    }
                }
            }
        }

        private static TreeViewItem _draggedItem;
        private void xTreeView_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (Mode == EModeType.ReadOnly)
            {
                return;
            }

            _draggedItem = this.FindAncestor<TreeViewItem>((DependencyObject)e.OriginalSource);
        }

        private void xTreeView_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (Mode == EModeType.ReadOnly)
            {
                return;
            }

            if (_draggedItem == null || e.LeftButton != MouseButtonState.Pressed)
                return;

            DragDrop.DoDragDrop(_draggedItem, _draggedItem, DragDropEffects.Move);
        }

        private void xTreeView_Drop(object sender, DragEventArgs e)
        {
            if (Mode == EModeType.ReadOnly)
            {
                return;
            }

            bool isSameTree = false;

            if (_draggedItem == null)
                return;

            TreeView fromTreeView = this.FindAncestor<TreeView>(_draggedItem as DependencyObject);
            TreeView toTreeView = this.FindAncestor<TreeView>((DependencyObject)e.OriginalSource);
            if(fromTreeView == toTreeView)
            {
                isSameTree = true;
            }

            TreeViewItem targetItem = this.FindAncestor<TreeViewItem>((DependencyObject)e.OriginalSource);

            if (targetItem != null)
            {
                if (_draggedItem is null)
                    return;

                // 드롭 위치를 변경합니다.
                TreeViewItem parentItem = this.FindAncestor<TreeViewItem>(_draggedItem as DependencyObject);

                if (parentItem is null ||
                    targetItem is null) return;

                // 예외 처리
                // 같은 대상인 경우
                if (targetItem == parentItem) return;

                // 사용자가 드래그 드롭한 타겟
                Group? targetGroup = targetItem.DataContext as Group;
                Process? targetProcess = targetItem.DataContext as Process;
                // 이동 대상
                Group? parentGroup = parentItem.DataContext as Group;
                Process? parentProcess = parentItem.DataContext as Process;

                // 빈 노드로 이동 한 경우
                if ((targetGroup is not null &&
                    targetGroup.IsEmpty is true) ||
                    (targetProcess is not null &&
                    targetProcess.IsEmpty is true) ||
                    (parentGroup is not null &&
                    parentGroup.IsEmpty is true) ||
                    (parentProcess is not null &&
                    parentProcess.IsEmpty is true))
                    return;

                // Process -> 자신 부모 Group로 이동 한 경우
                if(isSameTree is true &&
                    parentProcess is not null &&
                    targetGroup is not null &&
                    parentProcess.ParentGroup == targetGroup)
                {
                    return;
                }

                // NestedGroup -> 자신 부모 Group로 이동 한 경우
                if (isSameTree is true &&
                    parentGroup is not null &&
                    targetGroup is not null &&
                    parentGroup.ParentGroup == targetGroup)
                {
                    return;
                }

                // Group -> 자기 자식에게 이동 한 경우
                if (isSameTree is true &&
                    parentGroup is not null &&
                    parentGroup.ParentGroup is null &&
                    targetProcess is not null &&
                    parentGroup.Processes.Any( p => p.Id == targetProcess.Id))
                {
                    return;
                }

                // Group -> 자기 자식의 NestedGroup 그룹으로 이동 한 경우
                if (isSameTree is true &&
                    parentGroup is not null &&
                    parentGroup.NestedGroup is not null &&
                    targetGroup is not null &&
                    parentGroup.NestedGroup.Any( p => p.Id == targetGroup.Id))
                {
                    return;
                }

                // Group -> 자기 자식의 NestedGroup 그룹 자식 으로 이동 한 경우
                if (isSameTree is true &&
                    parentGroup is not null &&
                    parentGroup.NestedGroup is not null &&
                    targetProcess is not null &&
                    parentGroup.NestedGroup.Any( p => p.Id == targetProcess.ParentGroup.Id))
                {
                    return;
                }

                // MoveNode
                this.MoveNode(parentItem, targetItem);
            }

            _draggedItem = null;
        }

        /// <summary>
        /// Group or Process Id 생성<para/>
        /// 
        /// 아이디 체계<para/>
        /// Group id : {Group id}.000000<para/>
        /// Process id : {Group id}.{Process id}<para/>
        /// 
        /// 총 12자리 숫자 이상으로 구성<para/>
        /// 앞 6자리 : Group id, 뒤 자리 Process id<para/>
        /// 
        /// [규칙]<para/>
        /// 01. 노드가 추가 되는 경우 이전 노드 id에서 +1 또는 자릿수 증가<para/>
        /// 
        /// 02. 이전 노드, 다음 노드 id 사이에 +1로 id 증가가 가는 한 경우 +1로 생성<para/>
        /// 예] 001000.001000 - {노드 추가} - 001000.003000 인 경우 001000.002000 로 id 생성<para/>
        /// 
        /// 03. +1로 id 증가 했을 경우 id가 중복 되는 경우 자릿수 증가<para/>
        /// 예] 001000.001000 - {노드 추가} - 001000.002000 인 경우 001000.011000 로 id 생성<para/>
        /// 
        /// 04. 자릿수 증가인 경우 최대 늘어나는 조건은 없다. 무한 증가 가능
        /// </summary>
        /// <param name="currentTreeViewItem"></param>
        /// <param name="isGroupId"></param>
        /// <returns></returns>
        private string GenerationId(TreeViewItem currentTreeViewItem, bool isGroupId)
        {
            var parent = currentTreeViewItem.Parent as ItemsControl;
            string? id = null;
            
            if(isGroupId)
            {
                bool isNestedGroup = false;
                string groupFullId = currentTreeViewItem.Tag.ToString().Split(new string[] { "." }, StringSplitOptions.RemoveEmptyEntries)[0];
                // NestedGroup인 경우
                if (parent is TreeViewItem)
                {
                    isNestedGroup = true;
                }
                int groupId = int.Parse(groupFullId.Substring(0, 3));
                double groupExtentionId = double.Parse(groupFullId.Substring(3, 3));
                double groupExtentionUnit = double.Parse(groupFullId.Substring(3, 1));

                var findNextIdNode = this.xTreeView.FindTreeViewItemById($"{(groupId+1).ToString("D3").PadRight(6).Replace(" ", "0")}.000000");
                // id 증가
                if(isNestedGroup is false && findNextIdNode is null)
                {
                    return $"{(groupId + 1).ToString("D3").PadRight(6).Replace(" ", "0")}.000000";
                }
                // 자릿수 증가
                else
                {
                    if (groupExtentionId == 0)
                    {
                        groupExtentionId = groupId * 100;
                    }
                    else
                    {
                        groupExtentionId = (((groupExtentionId / (double)100) * 10) + (groupExtentionUnit * 100)) * 1000;
                    }
                    string newGroupId = $"{(groupId.ToString("D3") + groupExtentionId).PadRight(6).Replace(" ", "0")}.000000";
                    findNextIdNode = this.xTreeView.FindTreeViewItemById(newGroupId);
                    
                    // 중복
                    if(findNextIdNode is not null)
                    {
                        return this.GenerationId(findNextIdNode, isGroupId);
                    }
                    else
                    {
                        return newGroupId;
                    }
                }
            }
            else
            {
                /// 이전 노드가 중첩 그룹인 경우 중첩 그룹 프로세스 아이디의
                /// 연장선으로 중첩 그룹 아이디를 적용해서 생성하는 로직
                /// * var groupFullId 변수 주석 처리 하고 사용
                /// ※ 새로 생성된 아이디의 중복 체크는 해당 부모에서 중복 프로세스 아이디 자식이 있는지만 체크한다.
                /// 해당 중첩 그룹안 프로세스에서도 중복 아이디가 있는지 체크가 필요하다.

                string? prevItemGroupFullId = null;
                var prevItem = currentTreeViewItem.PrevItem(false);
                if (prevItem is not null &&
                    prevItem.Tag is not null)
                {
                    prevItemGroupFullId = prevItem.Tag.ToString().Split(new string[] { "." }, StringSplitOptions.RemoveEmptyEntries)[0];
                }
                var groupFullId = parent.Tag.ToString().Split(new string[] { "." }, StringSplitOptions.RemoveEmptyEntries)[0];

                // 프로세스 id 생성시 이전 프로세스 노드의 그룹 아이디와 다른 경우 중첩 그룹의 프로세스
                // 아이디의 연장선으로 간주 하여 그룹 아이디를 해당 중첩 그룹 아이디로 적용 한다.
                if (string.IsNullOrWhiteSpace(prevItemGroupFullId) is false
                    && groupFullId != prevItemGroupFullId)
                {
                    groupFullId = prevItemGroupFullId;

                    if (prevItem.Header is Group)
                    {
                        // 중첩 그룹의 마지막 노드 아이디 기준으로 생성
                        if (prevItem.Items.Count > 0)
                        {
                            currentTreeViewItem.Tag = ((TreeViewItem)prevItem.Items[prevItem.Items.Count - 1]).Tag;
                        }
                        else
                        {
                            // 중첩 그룹에 자식 프로세스가 없는 경우 프로세스 아이디 000000 기준으로 생성
                            currentTreeViewItem.Tag = $"{prevItemGroupFullId}.000000";
                        }
                    }
                }




                //var groupFullId = parent.Tag.ToString().Split(new string[] { "." }, StringSplitOptions.RemoveEmptyEntries)[0];

                var processFullId = currentTreeViewItem.Tag.ToString().Split(new string[] { "." }, StringSplitOptions.RemoveEmptyEntries)[1];
                int processId = int.Parse(processFullId.Substring(0, 3));
                double processExtentionId = double.Parse(processFullId.Substring(3, 3));
                double processExtentionUnit = double.Parse(processFullId.Substring(3, 1));

                var findNextIdNode = this.xTreeView.FindTreeViewItemById($"{groupFullId}.{(processId + 1).ToString("D3").PadRight(6).Replace(" ", "0")}");
                // id 증가
                if (findNextIdNode is null)
                {
                    return $"{groupFullId}.{(processId + 1).ToString("D3").PadRight(6).Replace(" ", "0")}";
                }
                else
                {
                    if (processExtentionId == 0)
                    {
                        processExtentionId = processId * 100;
                    }
                    else
                    {
                        processExtentionId = (((processExtentionId / (double)100) * 10) + (processExtentionUnit * 100)) * 1000;
                    }

                    string newProcessId = $"{groupFullId}.{(processId.ToString("D3") + processExtentionId).PadRight(6).Replace(" ", "0")}";
                    findNextIdNode = this.xTreeView.FindTreeViewItemById(newProcessId);

                    // 중복
                    if (findNextIdNode is not null)
                    {
                        return this.GenerationId(findNextIdNode, isGroupId);
                    }
                    else
                    {
                        return newProcessId;
                    }
                }
            }
        }

        private void MoveNode(TreeViewItem parent, TreeViewItem target, bool isCopy = false)
        {
            // 1. TreeViewItem 처리
            var tempParentNode = parent;

            // TreeViewItem 처리
            // 이동 타겟 순서 알아내기
            var targetIdx = -1;
            // 이동 타겟이 Root Group임
            if (target.Parent == this.xTreeView)
            {
                targetIdx = this.xTreeView.Items.IndexOf(target);
            }
            else
            {
                targetIdx = ((TreeViewItem)target.Parent).Items.IndexOf(target);
            }

            // Root에서 Group 끼리 이동
            if (parent.Parent == this.xTreeView &&
                target.Parent == this.xTreeView)
            {
                // 삭제
                if (isCopy is false)
                    parent.CurrentRemoveItem();

                // 삽입
                var newTreeViewItem = this.DeepCopyTreeViewItem(tempParentNode, null, null);
                this.xTreeView.Items.Insert(targetIdx, newTreeViewItem);
                if (newTreeViewItem.IsCurrentFirst())
                {
                    newTreeViewItem.SetGroupId();
                }
                else
                {
                    var prevGroupItem = newTreeViewItem.PrevNestedGroupItem();
                    // 이전 노드가 빈 노드 인 경우
                    if (prevGroupItem.Tag is null)
                    {
                        newTreeViewItem.SetGroupId();
                    }
                    else
                    {
                        newTreeViewItem.Tag = prevGroupItem.Tag;
                    }
                }

                if (tempParentNode.Header is Group)
                {
                    string newId = this.GenerationId(newTreeViewItem, true);
                    newTreeViewItem.Tag = newId;
                    (newTreeViewItem.Header as Group).Id = newId;
                    this.ChangeGroupId(newTreeViewItem);
                }

                if(DiffType == EDiffType.from)
                {
                    diffRootArr[0].Groups.Insert(targetIdx, newTreeViewItem.Header as Group);
                }
                else
                {
                    diffRootArr[1].Groups.Insert(targetIdx, newTreeViewItem.Header as Group);
                }
            }
            // Root Group이 다른 Group으로 이동
            else if (parent.Parent == this.xTreeView &&
                target.Parent != this.xTreeView)
            {
                // 삭제
                if (isCopy is false)
                    parent.CurrentRemoveItem();

                // 삽입
                var newTreeViewItem = this.DeepCopyTreeViewItem(tempParentNode, (TreeViewItem)target.Parent, ((TreeViewItem)target.Parent).Header as Group);
                ((TreeViewItem)target.Parent).Items.Insert(targetIdx, newTreeViewItem);
                if (newTreeViewItem.IsCurrentFirst())
                {
                    newTreeViewItem.SetGroupId();
                }
                else
                {
                    var prevGroupItem = newTreeViewItem.PrevNestedGroupItem();
                    // 이전 노드가 빈 노드 인 경우
                    if (prevGroupItem.Tag is null)
                    {
                        newTreeViewItem.SetGroupId();
                    }
                    else
                    {
                        newTreeViewItem.Tag = prevGroupItem.Tag;
                    }
                }

                if (tempParentNode.Header is Group)
                {
                    string newId = this.GenerationId(newTreeViewItem, true);
                    newTreeViewItem.Tag = newId;
                    (newTreeViewItem.Header as Group).Id = newId;
                    this.ChangeGroupId(newTreeViewItem);
                }

                // 모델에서 중첩 그룹은 가시적 순서가 표현되지 않으므로 그냥 마지막에 추가 한다.
                (newTreeViewItem.Header as Group).ParentGroup.NestedGroup.Add(newTreeViewItem.Header as Group);
            }
            // 뎁스가 2Level 이상 이중 Nested group 이 Root Group으로 빠지는 경우
            // 또는 프로세스가 Root Group으로 이동
            else if (parent.Parent != this.xTreeView &&
                target.Parent == this.xTreeView)
            {
                // 삭제
                if (isCopy is false)
                    parent.CurrentRemoveItem();

                if (parent.Header is Group)
                {
                    // 삽입
                    var newTreeViewItem = this.DeepCopyTreeViewItem(tempParentNode, null, null);
                    this.xTreeView.Items.Insert(targetIdx, newTreeViewItem);
                    if (newTreeViewItem.IsCurrentFirst())
                    {
                        newTreeViewItem.SetGroupId();
                    }
                    else
                    {
                        var prevGroupItem = newTreeViewItem.PrevNestedGroupItem();
                        // 이전 노드가 빈 노드 인 경우
                        if (prevGroupItem.Tag is null)
                        {
                            newTreeViewItem.SetGroupId();
                        }
                        else
                        {
                            newTreeViewItem.Tag = prevGroupItem.Tag;
                        }
                    }

                    if (tempParentNode.Header is Group)
                    {
                        string newId = this.GenerationId(newTreeViewItem, true);
                        newTreeViewItem.Tag = newId;
                        (newTreeViewItem.Header as Group).Id = newId;
                        this.ChangeGroupId(newTreeViewItem);
                    }

                    if (DiffType == EDiffType.from)
                    {
                        diffRootArr[0].Groups.Insert(targetIdx, newTreeViewItem.Header as Group);
                    }
                    else
                    {
                        diffRootArr[1].Groups.Insert(targetIdx, newTreeViewItem.Header as Group);
                    }
                }
                else
                {
                    // 삽입
                    var newTreeViewItem = this.DeepCopyTreeViewItem(tempParentNode, target, target.Header as Group);
                    target.Items.Insert(targetIdx, newTreeViewItem);
                    if (newTreeViewItem.IsCurrentFirst())
                    {
                        newTreeViewItem.SetProcessId("001000");
                    }
                    else
                    {
                        var prevItem = newTreeViewItem.PrevItem();
                        // 이전 노드가 빈 노드 인 경우
                        if (prevItem.Tag is null)
                        {
                            newTreeViewItem.SetProcessId("001000");
                        }
                        else
                        {
                            newTreeViewItem.Tag = prevItem.Tag;
                        }
                    }

                    if (tempParentNode.Header is Process)
                    {
                        string newId = this.GenerationId(newTreeViewItem, false);
                        newTreeViewItem.Tag = newId;
                        (newTreeViewItem.Header as Process).Id = newId;
                    }

                    (newTreeViewItem.Header as Process).ParentGroup.Processes.Insert(targetIdx, newTreeViewItem.Header as Process);
                }
            }
            else
            {
                // 삭제
                if (isCopy is false)
                    parent.CurrentRemoveItem();

                var newTreeViewItem = this.DeepCopyTreeViewItem(tempParentNode, (TreeViewItem)target.Parent, ((TreeViewItem)target.Parent).Header as Group);
                // 삽입
                ((TreeViewItem)target.Parent).Items.Insert(targetIdx, newTreeViewItem);
                if (newTreeViewItem.IsCurrentFirst())
                {
                    if (newTreeViewItem.Header is Group)
                    {
                        newTreeViewItem.SetGroupId();
                    }
                    else
                    {
                        newTreeViewItem.SetProcessId("001000");
                    }
                }
                else
                {
                    if (newTreeViewItem.Header is Group)
                    {
                        var prevGroupItem = newTreeViewItem.PrevNestedGroupItem();
                        newTreeViewItem.Tag = prevGroupItem.Tag;
                    }
                    else
                    {
                        var prevItem = newTreeViewItem.PrevItem();
                        // 이전 노드가 빈 노드 인 경우
                        if (prevItem.Tag is null)
                        {
                            newTreeViewItem.SetProcessId("001000");
                        }
                        else
                        {
                            newTreeViewItem.Tag = prevItem.Tag;
                        }
                    }
                }

                if (tempParentNode.Header is Group)
                {
                    string newId = this.GenerationId(newTreeViewItem, true);
                    newTreeViewItem.Tag = newId;
                    (newTreeViewItem.Header as Group).Id = newId;
                    this.ChangeGroupId(newTreeViewItem);

                    // 모델에서 중첩 그룹은 가시적 순서가 표현되지 않으므로 그냥 마지막에 추가 한다.
                    (newTreeViewItem.Header as Group).ParentGroup.NestedGroup.Add(newTreeViewItem.Header as Group);
                }
                else
                {
                    string newId = this.GenerationId(newTreeViewItem, false);
                    newTreeViewItem.Tag = newId;
                    (newTreeViewItem.Header as Process).Id = newId;

                    (newTreeViewItem.Header as Process).ParentGroup.Processes.Insert(targetIdx, newTreeViewItem.Header as Process);
                }
            }

            if (diffRootArr[0] is not null &&
                    diffRootArr[1] is not null)
            {
                if (DiffType == EDiffType.from)
                {
                    this.SynchronizeTreeViewByRight();
                }
                else
                {
                    this.SynchronizeTreeViewByLeft();
                }
                XmlDiffHelper.Diff2(diffRootArr[0], diffRootArr[1]);
            }
        }

        private T FindAncestor<T>(DependencyObject current) where T : DependencyObject
        {
            if (current is null) return null;
            do
            {
                if (current is T ancestor)
                    return ancestor;

                current = VisualTreeHelper.GetParent(current);
            }
            while (current != null);

            return null;
        }

        private void xCopyMenu_Click(object sender, RoutedEventArgs e)
        {
            if (_tempRightClickNode is null || _tempRightClickNode.Tag is null) return;
            
            if (_tempRightClickNode is not null && _tempRightClickNode.Header is Group group) {
                if (group.IsEmpty)
                    return;
            }
            if (_tempRightClickNode is not null && _tempRightClickNode.Header is Process porocess)
            {
                if (porocess.IsEmpty)
                    return;
            }

            _clipboard = new();
            _clipboard.ItemNode = _tempRightClickNode;
        }

        private void xPasteMenu_Click(object sender, RoutedEventArgs e)
        {
            if(_clipboard.ItemNode is null || _tempRightClickNode is null) return;

            this.MoveNode(_clipboard.ItemNode, _tempRightClickNode, true);
        }

        private void xDeleteMenu_Click(object sender, RoutedEventArgs e)
        {
            if (_tempRightClickNode is null) return;

            if(_tempRightClickNode.Tag is null)
            {
                MessageBox.Show("The ID of the node could not be found.");
                return;
            }

            if(MessageBox.Show($"\"{_tempRightClickNode.Tag.ToString()}\" Are you sure you want to delete the node?",
                "Delete",
                MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                _tempRightClickNode.CurrentRemoveItem();
        }

        private TreeViewItem DeepCopyTreeViewItem(TreeViewItem sourceItem, TreeViewItem? parentTreeViewItem, Group? parentGroup, bool isEmpty = false)
        {
            TreeViewItem newItem = new TreeViewItem();
            newItem.Tag = sourceItem.Tag;
            if (sourceItem.Header is Group group)
            {
                var cloneGroup = group.CloneData();
                cloneGroup.IsAdded = true;
                if (isEmpty)
                {
                    cloneGroup.IsEmpty = true;
                    if (group.MatchingGroupByTo is not null)
                    {
                        cloneGroup.IsExpanded = group.MatchingGroupByTo.IsExpanded;
                    }
                    else if (group.DiffFromGroup is not null)
                    {
                        cloneGroup.IsExpanded = group.DiffFromGroup.IsExpanded;
                    }
                }
                cloneGroup.ParentGroup = parentGroup;
                cloneGroup.ParentTreeViewItem = parentTreeViewItem;

                foreach (TreeViewItem processTreeViewItem in sourceItem.Items)
                {
                    if (processTreeViewItem.Header is Group)
                    {
                        var nestedGroup = this.DeepCopyTreeViewItem(processTreeViewItem,
                            ((TreeViewItem)processTreeViewItem.Parent),
                            ((TreeViewItem)processTreeViewItem.Parent).Header as Group,
                            isEmpty);
                        newItem.Items.Add(nestedGroup);
                    }
                    else
                    {
                        Process p = (processTreeViewItem.Header as Process).CloneData();
                        p.IsAdded = true;
                        if(isEmpty)
                        {
                            p.IsEmpty = true;
                        }

                        TreeViewItem newProcessItem = new TreeViewItem();
                        newProcessItem.Header = p;
                        newProcessItem.DataContext = p;
                        newProcessItem.Tag = p.Id;
                        newProcessItem.ItemContainerStyle = this.Resources["sTreeViewItem"] as Style;

                        // Add Item
                        foreach (TreeViewItem itemTreeViewItem in processTreeViewItem.Items)
                        {
                            if (itemTreeViewItem.Header is Item)
                            {
                                Item itemModel = (itemTreeViewItem.Header as Item).CloneData();

                                TreeViewItem newItemNode = new TreeViewItem();
                                newItemNode.Header = itemModel;
                                newItemNode.DataContext = itemModel;
                                newItemNode.ItemContainerStyle = this.Resources["sTreeViewItem"] as Style;
                                newProcessItem.Items.Add(newItemNode);
                            }
                        }

                        // 자식 프로세스들 새로 생성한 그룹으로 ParentGroup 변경
                        p.ParentGroup = cloneGroup;

                        newItem.Items.Add(newProcessItem);
                    }
                }
                newItem.Header = cloneGroup;
                newItem.DataContext = cloneGroup;
            }
            else if (sourceItem.Header is Process process)
            {
                var cloneProcess = process.CloneData();
                cloneProcess.IsAdded = true;
                cloneProcess.IsEmpty = isEmpty;
                if(isEmpty)
                    cloneProcess.IsEmpty = true;
                cloneProcess.ParentGroup = parentGroup;
                cloneProcess.ParentTreeViewItem = parentTreeViewItem;

                foreach (TreeViewItem processTreeViewItem in sourceItem.Items)
                {
                    if (processTreeViewItem.Header is Item)
                    {
                        Item itemModel = (processTreeViewItem.Header as Item).CloneData();

                        TreeViewItem newItemNode = new TreeViewItem();
                        newItemNode.Header = itemModel;
                        newItemNode.DataContext = itemModel;
                        newItemNode.ItemContainerStyle = this.Resources["sTreeViewItem"] as Style;
                        newItem.Items.Add(newItemNode);
                    }
                }

                newItem.Header = cloneProcess;
                newItem.DataContext = cloneProcess;
            }
            newItem.ItemContainerStyle = this.Resources["sTreeViewItem"] as Style;

            return newItem;
        }

        /// <summary>
        /// 하위 Process 노드 id의 Group id를 해당 그룹 id기준으로 변경 <para/>
        /// 아이디 체계<para/>
        /// Group id : {Group id}.000000<para/>
        /// Process id : {Group id}.{Process id}
        /// </summary>
        /// <param name="groupTreeViewItem"></param>
        private void ChangeGroupId(TreeViewItem groupTreeViewItem)
        {
            var groupId = groupTreeViewItem.Tag.ToString().Split(new string[] { "." }, StringSplitOptions.RemoveEmptyEntries)[0];

            foreach (TreeViewItem item in groupTreeViewItem.Items)
            {
                if (item.Tag is null) continue;

                if (item.Header is Group group)
                {
                    // 그룹 아이디 생성
                    group.Id = this.GenerationId(groupTreeViewItem, true);
                    item.Tag = group.Id;
                    if(item.Items.Count > 0)
                    {
                        ChangeGroupId(item);
                    }
                }
                else if (item.Header is Process process)
                {
                    var processId = item.Tag.ToString().Split(new string[] { "." }, StringSplitOptions.RemoveEmptyEntries)[1];

                    process.Id = $"{groupId}.{processId}";
                }
            }
        }

        private void xSaveMenu_Click(object sender, RoutedEventArgs e)
        {
            string? saveFilePath = null;
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "Xml files (*.xml)|*.xml";
            if (saveFileDialog.ShowDialog() == true)
            {
                saveFilePath = saveFileDialog.FileName;
                this.SaveXmlFile(saveFilePath);
            }
        }

        private void xTreeViewContextMenu_Opened(object sender, RoutedEventArgs e)
        {
            if (Mode == EModeType.ReadOnly)
            {
                ((ContextMenu)sender).IsEnabled = false;
                ((ContextMenu)sender).IsOpen = false;
                return;
            }
            else
            {
                ((ContextMenu)sender).IsEnabled = true;
            }

            if (_clipboard is null || _clipboard.ItemNode is null)
            {
                this.xPasteMenu.IsEnabled = false;
            }
            else
            {
                this.xPasteMenu.IsEnabled = true;
            }
        }
    }
}
