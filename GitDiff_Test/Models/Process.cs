using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Schema;
using System.Xml;
using System.Xml.Serialization;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Media;
using System.Collections.ObjectModel;
using System.Windows.Controls;

namespace GitDiff_Test.Models
{
    [XmlRoot(ElementName = "process")]
    public class Process : INotifyPropertyChanged
    {
        private bool _isDiff = false;
        private bool _isDup = false;
        private bool _without = false;
        private bool _isChecked = false;
        private bool _isExpanded = true;
        private Brush _color = (SolidColorBrush)new BrushConverter().ConvertFrom("#FF000000")!;
        private ObservableCollection<Item>? _items;

        [XmlAttribute("name")]
        public string? Name { get; set; }

        [XmlAttribute("type")]
        public string? Type { get; set; }

        [XmlElement("item")]
        public ObservableCollection<Item>? Items
        {
            get => _items;
            set
            {
                _items = value;
                OnPropertyChanged();
            }
        }

        [XmlAttribute("IsDiff")]
        public bool IsDiff
        {
            get => _isDiff;
            set
            {
                _isDiff = value;
                OnPropertyChanged();
            }
        }

        [XmlAttribute("IsDup")]
        public bool IsDup
        {
            get => _isDup;
            set
            {
                _isDup = value;
                OnPropertyChanged();
            }
        }

        [XmlIgnore]
        public bool IsChecked
        {
            get => _isChecked;
            set
            {
                _isChecked = value;
                OnPropertyChanged();
            }
        }

        [XmlIgnore]
        public bool Without
        {
            get => _without;
            set
            {
                _without = value;
                OnPropertyChanged();
            }
        }

        [XmlIgnore]
        public Brush Color
        {
            get => _color;
            set
            {
                _color = value;
                OnPropertyChanged();
            }
        }

        [XmlIgnore]
        public Process DiffFromProcess
        {
            get; set;
        }

        [XmlIgnore]
        public bool IsExpanded
        {
            get => _isExpanded;
            set
            {
                _isExpanded = value;
                OnPropertyChanged();
            }
        }

        [XmlIgnore]
        public Group ParentGroup
        {
            get;set;
        }

        [XmlIgnore]
        public TreeViewItem ParentTreeViewItem
        {
            get; set;
        }

        //public override bool Equals(object? obj)
        //{
        //    if (obj is null || Name is null) return false;
        //    return (this.Name == ((Item)obj).Name &&
        //        this.Type == ((Item)obj).Type);
        //}

        //public override int GetHashCode()
        //{
        //    if (Name == null)
        //        throw new Exception("Group Name 속성이 없습니다.");
        //    if (Type == null)
        //        throw new Exception("Process Type 속성이 없습니다.");


        //    int hash = 17;
        //    hash = hash * 23 + Name?.GetHashCode() ?? 0;
        //    hash = hash * 23 + Type?.GetHashCode() ?? 0;

        //    return hash;
        //}

        #region INotifyPropertyChange Implementation
        public event PropertyChangedEventHandler? PropertyChanged = delegate { };
        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChangedEventHandler? handler = this.PropertyChanged;

            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        #endregion INotifyPropertyChange Implementation
    }
}
