using System;
using System.Windows.Controls;
using XmlDiffLib.Models;

namespace XmlDiffLib
{
    public class Clipboard
    {
        public TreeViewItem? ItemNode { get; set; }
        public Group? GroupModel { get; set; }
        public Process? ProcessModel { get; set; }
    }
}
