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

namespace GitDiff_Test.Models
{
    [XmlRoot(ElementName = "item")]
    public class Item : INotifyPropertyChanged
    {
        private bool _isDiff = false;
        private bool _isDup = false;
        private bool _without = false;
        private bool _isChecked = false;
        private bool _isExpanded = true;

        [XmlAttribute("name")]
        public string? Name { get; set; }

        [XmlAttribute("value")]
        public string? Value { get; set; }

        [XmlAttribute("type")]
        public string? Type { get; set; }

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
        public bool IsExpanded
        {
            get => _isExpanded;
            set
            {
                _isExpanded = value;
                OnPropertyChanged();
            }
        }

        //public override bool Equals(object? obj)
        //{
        //    if (obj is null || Name is null) return false;
        //    return (this.Name == ((Item)obj).Name &&
        //        this.Value == ((Item)obj).Value &&
        //        this.Type == ((Item)obj).Type);
        //}

        //public override int GetHashCode()
        //{
        //    if (Name == null)
        //        throw new Exception("Item Name 속성이 없습니다.");
        //    if (Value == null)
        //        throw new Exception("Item Name 속성이 없습니다.");
        //    if (Type == null)
        //        throw new Exception("Item Name 속성이 없습니다.");


        //    int hash = 17;
        //    hash = hash * 23 + Name?.GetHashCode() ?? 0;
        //    hash = hash * 23 + Value?.GetHashCode() ?? 0;
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
