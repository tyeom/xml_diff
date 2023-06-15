using System;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Xml;
using XmlDiffLib.Models;

namespace XmlDiffLib.Extensions
{
    public static class TreeViewExtensions
    {
        public static Style TreeViewItemStyle;

        public static bool IsCurrentFirst(this TreeViewItem treeViewItem)
        {
            if (treeViewItem is null) return false;
            var parent = treeViewItem.Parent as ItemsControl;
            if (parent is null) return false;
            return parent.Items.IndexOf(treeViewItem) == 0;
        }

        public static bool IsCurrentLast(this TreeViewItem treeViewItem)
        {
            if (treeViewItem is null) return false;
            var parent = treeViewItem.Parent as ItemsControl;
            if (parent is null) return false;
            var max = parent.Items.Count - 1;
            return parent.Items.IndexOf(treeViewItem) == max;
        }

        public static TreeViewItem? NextItem(this TreeViewItem treeViewItem, bool ignoreGroupItem = true)
        {
            if (treeViewItem is null) return null;
            if (treeViewItem.IsCurrentLast() is true) return treeViewItem;

            var parent = treeViewItem.Parent as ItemsControl;
            if (parent is null) return null;

            var nextItemIdx = parent.Items.IndexOf(treeViewItem) + 1;
            var nextItem = parent.Items[nextItemIdx] as TreeViewItem;
            if (nextItem.Tag is null ||
                (ignoreGroupItem && nextItem.Header is Group))
            {
                return nextItem.NextItem();
            }
            else
            {
                return nextItem;
            }
        }

        public static TreeViewItem? NextNestedGroupItem(this TreeViewItem treeViewItem)
        {
            if (treeViewItem is null) return null;
            if (treeViewItem.IsCurrentLast() is true) return treeViewItem;

            var parent = treeViewItem.Parent as ItemsControl;
            if (parent is null) return null;

            var nextItemIdx = parent.Items.IndexOf(treeViewItem) + 1;
            var nextItem = parent.Items[nextItemIdx] as TreeViewItem;
            if (nextItem.Tag is null ||
                nextItem.Header is Group == false ||
                (nextItem.Header is Group nestedGroup && nestedGroup.ParentGroup is null))
            {
                return nextItem.NextNestedGroupItem();
            }
            else
            {
                return nextItem;
            }
        }

        public static TreeViewItem? PrevItem(this TreeViewItem treeViewItem, bool ignoreGroupItem = true)
        {
            if (treeViewItem is null) return null;
            if (treeViewItem.IsCurrentFirst() is true) return treeViewItem;

            var parent = treeViewItem.Parent as ItemsControl;
            if (parent is null) return null;

            var prevItemIdx = parent.Items.IndexOf(treeViewItem) - 1;
            var prevItem = parent.Items[prevItemIdx] as TreeViewItem;
            if(prevItem.Tag is null ||
                (ignoreGroupItem && prevItem.Header is Group))
            {
                return prevItem.PrevItem();
            }
            else
            {
                return prevItem;
            }
        }

        public static TreeViewItem? PrevNestedGroupItem(this TreeViewItem treeViewItem)
        {
            if (treeViewItem is null) return null;
            if (treeViewItem.IsCurrentFirst() is true) return treeViewItem;

            var parent = treeViewItem.Parent as ItemsControl;
            if (parent is null) return null;

            var prevItemIdx = parent.Items.IndexOf(treeViewItem) - 1;
            var prevItem = parent.Items[prevItemIdx] as TreeViewItem;
            if (prevItem.Tag is null ||
                prevItem.Header is Group == false ||
                (prevItem.Header is Group nestedGroup && nestedGroup.ParentGroup is null))
            {
                return prevItem.PrevNestedGroupItem();
            }
            else
            {
                return prevItem;
            }
        }

        private static TreeView FindParentTreeView(TreeViewItem treeViewItem)
        {
            DependencyObject parent = VisualTreeHelper.GetParent(treeViewItem);

            while (parent != null && !(parent is TreeView))
            {
                parent = VisualTreeHelper.GetParent(parent);
            }

            return parent as TreeView;
        }

        public static void CurrentRemoveItem(this TreeViewItem treeViewItem)
        {
            if (treeViewItem is null) return;

            Root rootModel = FindParentTreeView(treeViewItem).Tag as Root;

            // 위치 찾기
            var parent = treeViewItem.Parent as ItemsControl;
            var index = parent.Items.IndexOf(treeViewItem);

            // 삭제
            if (treeViewItem.Parent is TreeViewItem parentItem)
            {
                parentItem.Items.Remove(treeViewItem);
            }
            else if (treeViewItem.Parent is TreeView rootTreeView)
            {
                rootTreeView.Items.Remove(treeViewItem);
            }

            // 삭제된 자리에 빈 노드 추가
            if (treeViewItem.Header is Group)
            {
                //Group emptyGroup = new Group() { IsEmpty = true };
                //TreeViewItem emptyNodeItem = new();
                //// 마지막 노드는 표시 되지 않도록 트리거 설정이 되어 있어 임의로 자식 노드를 추가
                //emptyNodeItem.Items.Add(new());
                //emptyNodeItem.Header = emptyGroup;
                //emptyNodeItem.DataContext = emptyGroup;
                //emptyNodeItem.Tag = treeViewItem.Tag;
                //emptyNodeItem.ItemContainerStyle = TreeViewItemStyle;
                //parent.Items.Insert(index, emptyNodeItem);



                Group emptyGroup = new Group() { IsEmpty = true,
                    DiffFromGroup = (treeViewItem.Header as Group).DiffFromGroup,
                    MatchingGroupByTo = (treeViewItem.Header as Group).MatchingGroupByTo };

                // 그룹 노드 확장/축소시 동기화 되도록
                // 반대쪽 트리 노드에 새로 추가된 빈 노드를 연결해 준다.
                if ((treeViewItem.Header as Group).DiffFromGroup is not null)
                {
                    (treeViewItem.Header as Group).DiffFromGroup.MatchingGroupByTo = emptyGroup;
                    emptyGroup.IsExpanded = (treeViewItem.Header as Group).DiffFromGroup.IsExpanded;
                }
                if ((treeViewItem.Header as Group).MatchingGroupByTo is not null)
                {
                    (treeViewItem.Header as Group).MatchingGroupByTo.DiffFromGroup = emptyGroup;
                    emptyGroup.IsExpanded = (treeViewItem.Header as Group).MatchingGroupByTo.IsExpanded;
                }

                emptyGroup.NestedGroup = new();
                emptyGroup.Processes = new();
                TreeViewItem emptyNodeItem = new();
                emptyNodeItem.Header = emptyGroup;
                emptyNodeItem.DataContext = emptyGroup;
                emptyNodeItem.Tag = treeViewItem.Tag;
                emptyNodeItem.ItemContainerStyle = TreeViewItemStyle;

                // 마지막 노드는 표시 되지 않도록 트리거 설정이 되어 있어 임의로 자식 노드를 추가
                foreach (TreeViewItem emptyTreeViewItem in treeViewItem.Items)
                {
                    if (emptyTreeViewItem.Header is Process)
                    {
                        Process process = new() { ParentGroup = emptyGroup, IsEmpty = true };
                        emptyGroup.Processes.Add(process);

                        TreeViewItem emptyProcessTreeViewItem = new();
                        // 마지막 노드는 표시 되지 않도록 트리거 설정이 되어 있어 임의로 자식 노드를 추가
                        emptyProcessTreeViewItem.Items.Add(new TreeViewItem());
                        emptyProcessTreeViewItem.Header = process;
                        emptyProcessTreeViewItem.DataContext = process;
                        emptyProcessTreeViewItem.ItemContainerStyle = TreeViewItemStyle;
                        emptyNodeItem.Items.Add(emptyProcessTreeViewItem);
                    }
                    else if (emptyTreeViewItem.Header is Group)
                    {
                        Group nestedGroup = new() { ParentGroup = emptyGroup,
                            IsEmpty = true,
                            DiffFromGroup = (emptyTreeViewItem.Header as Group).DiffFromGroup,
                            MatchingGroupByTo = (emptyTreeViewItem.Header as Group).MatchingGroupByTo,
                        };

                        // 그룹 노드 확장/축소시 동기화 되도록
                        // 반대쪽 트리 노드에 새로 추가된 빈 노드를 연결해 준다.
                        if ((emptyTreeViewItem.Header as Group).DiffFromGroup is not null)
                        {
                            (emptyTreeViewItem.Header as Group).DiffFromGroup.MatchingGroupByTo = nestedGroup;
                            nestedGroup.IsExpanded = (treeViewItem.Header as Group).DiffFromGroup.IsExpanded;
                        }
                        if ((emptyTreeViewItem.Header as Group).MatchingGroupByTo is not null)
                        {
                            (emptyTreeViewItem.Header as Group).MatchingGroupByTo.DiffFromGroup = nestedGroup;
                            nestedGroup.IsExpanded = (treeViewItem.Header as Group).MatchingGroupByTo.IsExpanded;
                        }

                        nestedGroup.NestedGroup = new();
                        nestedGroup.Processes = new();
                        emptyGroup.NestedGroup.Add(nestedGroup);

                        TreeViewItem emptyGroupTreeViewItem = new();
                        emptyGroupTreeViewItem.Header = nestedGroup;
                        emptyGroupTreeViewItem.DataContext = nestedGroup;
                        emptyGroupTreeViewItem.ItemContainerStyle = TreeViewItemStyle;
                        emptyNodeItem.Items.Add(emptyGroupTreeViewItem);

                        /// TODO : 1 Level 중첩 그룹의 프로세스 자식 깊이 까지만 빈노드를 생성한다.
                        foreach (TreeViewItem emptyChildTreeViewItem in emptyTreeViewItem.Items)
                        {
                            Process process = new() { ParentGroup = nestedGroup, IsEmpty = true };
                            nestedGroup.Processes.Add(process);

                            TreeViewItem emptyProcessTreeViewItem = new();
                            // 마지막 노드는 표시 되지 않도록 트리거 설정이 되어 있어 임의로 자식 노드를 추가
                            emptyProcessTreeViewItem.Items.Add(new TreeViewItem());
                            emptyProcessTreeViewItem.Header = process;
                            emptyProcessTreeViewItem.DataContext = process;
                            emptyProcessTreeViewItem.ItemContainerStyle = TreeViewItemStyle;
                            emptyGroupTreeViewItem.Items.Add(emptyProcessTreeViewItem);
                        }
                    }
                }

                parent.Items.Insert(index, emptyNodeItem);








                // 모델에도 적용
                Group groupModel = treeViewItem.Header as Group;
                if(groupModel.ParentGroup is null)
                {
                    if (rootModel.Groups.Count > index)
                    {
                        rootModel.Groups.Insert(index, emptyGroup);
                    }
                    else
                    {
                        rootModel.Groups.Add(emptyGroup);
                    }
                }
                else
                {
                    // 모델에서 중첩 그룹은 가시적 순서가 표현되지 않으므로 그냥 마지막에 추가 한다.
                    groupModel.ParentGroup.NestedGroup.Add(emptyGroup);
                }
            }
            else
            {
                Process emptyProcess = new Process() { IsEmpty = true };
                TreeViewItem emptyNodeItem = new();
                // 마지막 노드는 표시 되지 않도록 트리거 설정이 되어 있어 임의로 자식 노드를 추가
                emptyNodeItem.Items.Add(new TreeViewItem());
                emptyNodeItem.Header = emptyProcess;
                emptyNodeItem.DataContext = emptyProcess;
                emptyNodeItem.Tag = treeViewItem.Tag;
                emptyNodeItem.ItemContainerStyle = TreeViewItemStyle;
                parent.Items.Insert(index, emptyNodeItem);

                // 모델에도 적용
                Process processModel = treeViewItem.Header as Process;
                if (processModel.ParentGroup.Processes.Count > index)
                {
                    processModel.ParentGroup.Processes.Insert(index, emptyProcess);
                }
                else
                {
                    processModel.ParentGroup.Processes.Add(emptyProcess);
                }
            }

            // 모델에도 삭제 적용
            if (treeViewItem.Header is Group group1 &&
                group1.ParentGroup is null)
            {
                rootModel.Groups.Remove(group1);

                // 모델에 diff 처리
                if(group1.MatchingGroupByTo is not null)
                {
                    group1.MatchingGroupByTo.IsDiff = true;
                }
            }
            else if (treeViewItem.Header is Group group2 &&
                group2.ParentGroup is not null)
            {
                group2.ParentGroup.NestedGroup.Remove(group2);
            }
            else if (treeViewItem.Header is Process process)
            {
                process.ParentGroup.Processes.Remove(process);

                // 모델에 diff 처리
                if (process.ParentGroup is not null)
                {
                    var matchingGroupByTo = process.ParentGroup.MatchingGroupByTo;
                    if(matchingGroupByTo is not null)
                    {
                        var toDiffProcess = matchingGroupByTo.Processes.FirstOrDefault(p => p.Id == process.Id);
                        if (toDiffProcess is not null)
                        {
                            toDiffProcess.DiffFromProcess = null;
                            toDiffProcess.IsDiff = true;
                        }
                    }
                }
            }
        }
        public static void SetGroupId(this TreeViewItem treeViewItem)
        {
            if (treeViewItem is null) return;

            // 부모 Group
            var parentGroup = treeViewItem.Parent as TreeViewItem;
            if (parentGroup is null)
            {
                treeViewItem.Tag = "001000.000000";
            }
            else
            {
                treeViewItem.Tag = ((TreeViewItem)parentGroup).Tag;
            }
        }

        public static void SetProcessId(this TreeViewItem treeViewItem, string processId)
        {
            if (treeViewItem is null) return;

            // 부모 Group
            var parentGroup = treeViewItem.Parent as ItemsControl;
            if (parentGroup is null || parentGroup.Tag is null)
                return;

            var groupId = parentGroup.Tag.ToString().Split(new string[] { "." }, StringSplitOptions.RemoveEmptyEntries)[0];
            treeViewItem.Tag = $"{groupId}.{processId}";
        }

        public static TreeViewItem? FindTreeViewItemById(this TreeView treeView, string id)
        {
            return FindTreeViewItemByIdRecursive(treeView.Items, id);
        }

        private static TreeViewItem? FindTreeViewItemByIdRecursive(ItemCollection items, string id)
        {
            foreach (object item in items)
            {
                TreeViewItem? treeViewItem = item as TreeViewItem;
                if (treeViewItem is null) continue;

                if (treeViewItem.Tag is not null &&
                    treeViewItem.Tag.ToString() == id)
                {
                    return treeViewItem;
                }

                TreeViewItem? nestedTreeViewItem = FindTreeViewItemByIdRecursive(treeViewItem.Items, id);
                if (nestedTreeViewItem != null)
                {
                    return nestedTreeViewItem;
                }
            }

            return null;
        }

        public static void SaveToXml(this TreeView treeView, string filePath)
        {
            if (treeView.Items.Count == 0)
                return;

            var settings = new XmlWriterSettings
            {
                Indent = true,
                IndentChars = "\t"
            };

            using (var writer = XmlWriter.Create(filePath, settings))
            {
                writer.WriteStartDocument();
                writer.WriteStartElement("root");

                foreach (TreeViewItem item in treeView.Items)
                {
                    SaveTreeViewItemToXml(item, writer);
                }

                writer.WriteEndElement();
                writer.WriteEndDocument();
            }
        }

        private static void SaveTreeViewItemToXml(TreeViewItem item, XmlWriter writer)
        {
            if(item.Header is Group group)
            {
                if (group.IsEmpty is true) return;

                writer.WriteStartElement("group");
                writer.WriteAttributeString("name", group.Name);
                writer.WriteAttributeString("id", group.Id);
            }
            else if (item.Header is Process process)
            {
                if (process.IsEmpty is true) return;

                writer.WriteStartElement("process");
                writer.WriteAttributeString("name", process.Name);
                writer.WriteAttributeString("id", process.Id);
                writer.WriteAttributeString("type", process.Type);
            }
            else if (item.Header is Item itemModel)
            {
                writer.WriteStartElement("item");
                writer.WriteAttributeString("name", itemModel.Name);
                writer.WriteAttributeString("value", itemModel.Value);
                writer.WriteAttributeString("type", itemModel.Type);
            }

            if (item.HasItems)
            {
                foreach (TreeViewItem subItem in item.Items)
                {
                    SaveTreeViewItemToXml(subItem, writer);
                }
            }

            writer.WriteEndElement();
        }
    }
}
