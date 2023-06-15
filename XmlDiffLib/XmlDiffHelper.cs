using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Media;
using XmlDiffLib.Models;

namespace XmlDiffLib
{
    public class XmlDiffHelper
    {
        private static readonly Random _random = new Random();

        public static void Diff3(TreeView from, TreeView to)
        {
            for (int i = 0; i < from.Items.Count; i++)
            {
                var fromGroup = (from.Items[i] as TreeViewItem).DataContext as Group;
                if (fromGroup.Name == "Group01")
                {
                    //
                }

                // 중복 그룹 이름이 있는지 체크
                var matchingGroups = to.Items.Cast<TreeViewItem>().Where(g => ((Group)g.DataContext).Id == fromGroup.Id);
                if (matchingGroups != null && matchingGroups.Count() > 1)
                {
                    var groups = matchingGroups.ToList();
                    for (int dup = 1; dup < groups.Count; dup++)
                    {
                        ((Group)groups[dup].DataContext).IsChecked = true;
                        ((Group)groups[dup].DataContext).IsDup = true;
                    }
                }

                if (to.Items.Count > i)
                {
                    var toGroup = (to.Items[i] as TreeViewItem).DataContext as Group;
                    CompareGroups2(fromGroup, toGroup);
                }
                else
                {
                    fromGroup.IsChecked = true;
                    fromGroup.Without = true;
                }
            }
        }

        public static void Diff2(Root from, Root to)
        {
            for (int i = 0; i < from.Groups.Count; i++)
            {
                var fromGroup = from.Groups[i];

                if (fromGroup.IsEmpty is true) continue;

                if (fromGroup.Name == "Group01")
                {
                    //
                }

                // 중복 그룹 이름이 있는지 체크
                var matchingGroups = to.Groups.Where(g => g.Id == fromGroup.Id);
                if (matchingGroups != null && matchingGroups.Count() > 1)
                {
                    var groups = matchingGroups.ToList();
                    for (int dup = 1; dup < groups.Count; dup++)
                    {
                        groups[dup].IsChecked = true;
                        groups[dup].IsDup = true;
                    }
                }

                var toGroup = to.Groups.FirstOrDefault( p => p.Id == fromGroup.Id);
                if (toGroup is not null)
                {
                    CompareGroups2(fromGroup, toGroup);
                }
                else
                {
                    fromGroup.IsChecked = true;
                    fromGroup.Without = true;
                }
            }
        }

        public static void Diff(Root from, Root to)
        {
            foreach (var fromGroup in from.Groups)
            {
                if (fromGroup.Name == "특별 Group")
                {
                    //
                }

                var rndColor = GenerateRandomHexColor();
                var matchingGroups = to.Groups.Where(g => g.Id == fromGroup.Id);
                if (matchingGroups != null && matchingGroups.Count() > 1)
                {
                    var groups = matchingGroups.ToList();
                    for (int i = 1; i < groups.Count; i++)
                    {
                        groups[i].IsChecked = true;
                        groups[i].IsDup = true;
                    }

                    fromGroup.Color = rndColor;
                    matchingGroups.ToList()[0].Color = rndColor;
                    CompareGroups(fromGroup, matchingGroups.ToList()[0]);
                }
                else if (matchingGroups != null && matchingGroups.Count() == 1)
                {
                    fromGroup.Color = rndColor;
                    matchingGroups.ToList()[0].Color = rndColor;
                    CompareGroups(fromGroup, matchingGroups.ToList()[0]);
                }
                else
                {
                    fromGroup.IsChecked = true;
                    fromGroup.Without = true;
                }
            }
        }

        public static void CompareGroups2(Group from, Group to)
        {
            if (from is not null &&
                from.IsEmpty is true &&
                from.MatchingGroupByTo is not null &&
                from.MatchingGroupByTo.IsEmpty is true)
            {
                from.IsExpanded = false;
            }
            if (to is not null &&
                to.IsEmpty is true &&
                to.DiffFromGroup is not null &&
                to.DiffFromGroup.IsEmpty is true)
            {
                to.IsExpanded = false;
            }

            from.IsChecked = true;

            if (from.IsAdded is false && to == null)
            {
                from.Without = true;
                return;
            }

            // 같은 그룹 동일 색상 지정
            var rndColor = GenerateRandomHexColor();
            from.Color = rndColor;
            to.Color = rndColor;

            to.IsChecked = true;
            to.DiffFromGroup = from;
            from.MatchingGroupByTo = to;

            if (from.Id != to.Id)
            {
                from.IsDiff = true;
                to.IsDiff = true;
            }
            else
            {
                from.IsDiff = false;
                to.IsDiff = false;
            }

            CompareProcesses2(from.Processes, to.Processes);
            CompareNestedGroups2(from.NestedGroup, to.NestedGroup);
        }

        public static void CompareGroups(Group from, Group to)
        {
            from.IsChecked = true;

            if (to == null)
            {
                from.Without = true;
                return;
            }

            to.IsChecked = true;
            to.DiffFromGroup = from;
            from.MatchingGroupByTo = to;

            if (from.Name != to.Name)
            {
                from.IsDiff = true;
                to.IsDiff = true;
            }
            else
            {
                from.IsDiff = false;
                to.IsDiff = false;
            }

            CompareProcesses(from.Processes, to.Processes);
            CompareNestedGroups(from.NestedGroup, to.NestedGroup);
        }

        public static void CompareProcesses2(ObservableCollection<Process>? fromProcesses, ObservableCollection<Process>? toProcesses)
        {
            if (fromProcesses == null || toProcesses == null)
                return;

            for (int i = 0; i < fromProcesses.Count; i++)
            {
                var fromProcess = fromProcesses[i];
                
                if (fromProcess.IsEmpty is true) continue;

                fromProcess.IsChecked = true;

                var toProcess = toProcesses.FirstOrDefault( p => p.Id == fromProcess.Id );
                if (toProcess is null)
                {
                    fromProcess.Without = true;
                }
                else
                {
                    toProcess.IsChecked = true;
                    if (fromProcess.Id != toProcess.Id || fromProcess.Type != toProcess.Type || fromProcess.Name != toProcess.Name)
                    {
                        fromProcess.IsDiff = true;
                        toProcess.IsDiff = true;
                    }
                    else
                    {
                        fromProcess.IsDiff = false;
                        toProcess.IsDiff = false;
                    }

                    toProcess.DiffFromProcess = fromProcess;

                    CompareItems2(fromProcess.Items, toProcess.Items);

                    if (toProcess.Items is not null)
                    {
                        // Item 노드에 문제가 있는 경우 Process에 IsDiff 표시
                        if (toProcess.Items.Any(p => p.IsDiff == true || p.IsChecked == false))
                        {
                            toProcess.IsDiff = true;
                        }
                        else
                        {
                            toProcess.IsDiff = false;
                        }
                        if (toProcess.Items.Any(p => p.IsDup == true))
                        {
                            toProcess.IsDup = true;
                        }
                        else
                        {
                            toProcess.IsDup = false;
                        }
                    }

                    if (fromProcess.Items is not null)
                    {
                        if (fromProcess.Items.Any(p => p.IsDiff == true || p.Without == true))
                        {
                            fromProcess.IsDiff = true;
                        }
                        else
                        {
                            fromProcess.IsDiff = false;
                        }
                        if (fromProcess.Items.Any(p => p.IsDup))
                        {
                            fromProcess.IsDup = true;
                        }
                        else
                        {
                            fromProcess.IsDup = false;
                        }
                    }
                }
            }
        }

        public static void CompareProcesses(ObservableCollection<Process>? fromProcesses, ObservableCollection<Process>? toProcesses)
        {
            if (fromProcesses == null || toProcesses == null)
                return;

            foreach (var fromProcess in fromProcesses)
            {
                fromProcess.IsChecked = true;
                var toProcess = toProcesses.FirstOrDefault(p => p.Id == fromProcess.Id);

                if (toProcess == null)
                {
                    fromProcess.Without = true;
                    continue;
                }

                toProcess.IsChecked = true;
                if (fromProcess.Name != toProcess.Name || fromProcess.Type != toProcess.Type)
                {
                    fromProcess.IsDiff = true;
                    toProcess.IsDiff = true;
                }
                else
                {
                    fromProcess.IsDiff = false;
                    toProcess.IsDiff = false;
                }

                toProcess.DiffFromProcess = fromProcess;

                CompareItems(fromProcess.Items, toProcess.Items);

                // Item 노드에 문제가 있는 경우 Process에 IsDiff 표시
                if (toProcess.Items.Any(p => p.IsDiff == true || p.IsChecked == false))
                {
                    toProcess.IsDiff = true;
                }
                else
                {
                    toProcess.IsDiff = false;
                }
                if (toProcess.Items.Any(p => p.IsDup == true))
                {
                    toProcess.IsDup = true;
                }
                else
                {
                    toProcess.IsDup = false;
                }

                if (fromProcess.Items.Any(p => p.IsDiff == true || p.Without == true))
                {
                    fromProcess.IsDiff = true;
                }
                else
                {
                    fromProcess.IsDiff = false;
                }
                if (fromProcess.Items.Any(p => p.IsDup))
                {
                    fromProcess.IsDup = true;
                }
                else
                {
                    fromProcess.IsDup = false;
                }
            }
        }

        public static void CompareItems2(ObservableCollection<Item>? fromItems, ObservableCollection<Item>? toItems)
        {
            if (fromItems == null || toItems == null)
                return;

            for (int i = 0; i < fromItems.Count; i++)
            {
                var fromItem = fromItems[i];
                fromItem.IsChecked = true;

                if (toItems.Count > i)
                {
                    var toItem = toItems[i];
                    toItem.IsChecked = true;
                    if (fromItem.Name != toItem.Name || fromItem.Value != toItem.Value || fromItem.Type != toItem.Type)
                    {
                        fromItem.IsDiff = true;
                        toItem.IsDiff = true;
                    }
                    else
                    {
                        fromItem.IsDiff = false;
                        toItem.IsDiff = false;
                    }
                }
                else
                {
                    fromItem.Without = true;
                }
            }

            // Item 중복 체크
            //var duplicatedItems = toItems.GroupBy(i => i.Type).Where(g => g.Count() > 1);

            //foreach (var duplicatedGroup in duplicatedItems)
            //{
            //    foreach (var item in duplicatedGroup)
            //    {
            //        item.IsDup = true;
            //    }
            //}
        }

        public static void CompareItems(ObservableCollection<Item>? fromItems, ObservableCollection<Item>? toItems)
        {
            if (fromItems == null || toItems == null)
                return;

            foreach (var fromItem in fromItems)
            {
                fromItem.IsChecked = true;
                var toItem = toItems.FirstOrDefault(i => i.Type == fromItem.Type);

                if (toItem == null)
                {
                    fromItem.Without = true;
                    continue;
                }

                toItem.IsChecked = true;
                if (fromItem.Name != toItem.Name || fromItem.Value != toItem.Value || fromItem.Type != toItem.Type)
                {
                    fromItem.IsDiff = true;
                    toItem.IsDiff = true;
                }
                else
                {
                    fromItem.IsDiff = false;
                    toItem.IsDiff = false;
                }
            }

            // Item 중복 체크
            var duplicatedItems = toItems.GroupBy(i => i.Type).Where(g => g.Count() > 1);

            foreach (var duplicatedGroup in duplicatedItems)
            {
                foreach (var item in duplicatedGroup)
                {
                    item.IsDup = true;
                }
            }
        }

        public static void CompareNestedGroups2(ObservableCollection<Group>? fromNestedGroups, ObservableCollection<Group>? toNestedGroups)
        {
            if (fromNestedGroups == null || toNestedGroups == null)
                return;

            for (int i = 0; i < fromNestedGroups.Count; i++)
            {
                var fromGroup = fromNestedGroups[i];
                var toNestedGroup = toNestedGroups.FirstOrDefault(p => p.Id == fromGroup.Id);

                if (i < toNestedGroups.Count)
                {
                    CompareGroups2(fromNestedGroups[i], toNestedGroup);
                }
                else
                {
                    fromNestedGroups[i].IsDiff = true;
                }
            }

            if (toNestedGroups.Count > fromNestedGroups.Count)
            {
                for (int i = fromNestedGroups.Count; i < toNestedGroups.Count; i++)
                {
                    toNestedGroups[i].IsDiff = true;
                }
            }
        }

        public static void CompareNestedGroups(ObservableCollection<Group>? fromNestedGroups, ObservableCollection<Group>? toNestedGroups)
        {
            if (fromNestedGroups == null || toNestedGroups == null)
                return;

            for (int i = 0; i < fromNestedGroups.Count; i++)
            {
                if (i < toNestedGroups.Count)
                {
                    CompareGroups(fromNestedGroups[i], toNestedGroups[i]);
                }
                else
                {
                    fromNestedGroups[i].IsDiff = true;
                }
            }

            if (toNestedGroups.Count > fromNestedGroups.Count)
            {
                for (int i = fromNestedGroups.Count; i < toNestedGroups.Count; i++)
                {
                    toNestedGroups[i].IsDiff = true;
                }
            }
        }

        private static Brush GenerateRandomHexColor()
        {
            byte[] colorBytes = new byte[3];
            _random.NextBytes(colorBytes);

            string hexColor = "#" + BitConverter.ToString(colorBytes).Replace("-", string.Empty);
            return (SolidColorBrush)new BrushConverter().ConvertFrom(hexColor)!;
        }
    }
}
