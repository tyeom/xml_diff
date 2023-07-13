using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;
using System.Xml.Serialization;
using System.Xml.XPath;

namespace XmlDiffLib.Models
{
    [XmlRoot("root")]
    public class Root : IXPathNavigable
    {
        [XmlElement("group")]
        public List<Group>? Groups { get; set; }

        public XPathNavigator? CreateNavigator()
        {
            XPathNavigator navigator;
            XmlSerializer serializer = new XmlSerializer(typeof(Root));

            using (System.IO.MemoryStream stream = new System.IO.MemoryStream())
            {
                serializer.Serialize(stream, this);
                stream.Position = 0;
                navigator = new XPathDocument(stream).CreateNavigator();
            }

            return navigator;
        }

        public Group? FindGroupByName(ObservableCollection<Group> groups, string name)
        {
            foreach (var group in groups)
            {
                if (group.Name == name)
                    return group;

                if (group.NestedGroup != null)
                {
                    var nestedGroup = FindGroupByName(group.NestedGroup, name);
                    if (nestedGroup != null)
                        return nestedGroup;
                }
            }

            return null;
        }
    }

    [XmlRoot(ElementName = "group")]
    public class Group : INotifyPropertyChanged
    {
        private string _name;
        private string _id;
        private bool _isDiff = false;
        private bool _isDup = false;
        private bool _without = false;
        private bool _isChecked = false;
        private bool _isAdded = false;
        private bool _isExpanded = true;
        private Group? _parentGroup = null;
        private Brush _color = (SolidColorBrush)new BrushConverter().ConvertFrom("#FF000000")!;
        private ObservableCollection<Process>? _processes;
        private ObservableCollection<Group>? _nestedGroup;

        [XmlAttribute("name")]
        public string? Name
        {
            get => _name;
            set
            {
                _name = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// 고유 값
        /// </summary>
        [XmlAttribute("id")]
        public string? Id
        {
            get => _id;
            set
            {
                _id = value;
                OnPropertyChanged();
            }
        }

        [XmlElement("process")]
        public ObservableCollection<Process>? Processes
        {
            get => _processes;
            set
            {
                _processes = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// 중첩 그룹
        /// </summary>
        [XmlElement("group")]
        public ObservableCollection<Group>? NestedGroup
        {
            get => _nestedGroup;
            set
            {
                _nestedGroup = value;
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
        public bool IsAdded
        {
            get => _isAdded;
            set
            {
                _isAdded = value;
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
        public Group DiffFromGroup
        {
            get;set;
        }

        /// <summary>
        /// diff 대상 to Group
        /// </summary>
        [XmlIgnore]
        public Group? MatchingGroupByTo
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
        public Group? ParentGroup
        {
            get => _parentGroup;
            set
            {
                _parentGroup = value;
                OnPropertyChanged();
            }
        }

        [XmlAttribute("IsEmpty")]
        public bool IsEmpty
        {
            get; set;
        }

        [XmlIgnore]
        public TreeViewItem ParentTreeViewItem
        {
            get; set;
        }

        [XmlIgnore]
        public CompositeCollection Children
        {
            get
            {
                return new CompositeCollection()
                {
                    new CollectionContainer() { Collection = Processes },
                    new CollectionContainer() { Collection = NestedGroup },
                };
            }
        }

        public Group CloneData()
        {
            return XmlSerializerHelper.CloneData<Group>(this);
        }

        //public override bool Equals(object? obj)
        //{
        //    if(obj is null || Name is null) return false;
        //    return (this.Name == ((Group)obj).Name);
        //}

        //public override int GetHashCode()
        //{
        //    if (Name == null)
        //        throw new Exception("Group Name 속성이 없습니다.");

        //    return Name.GetHashCode();
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
