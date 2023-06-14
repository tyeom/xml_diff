using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using XmlDiffLib.Models;

namespace XmlDiffLib.ViewModels
{
    public class XmlDiffViewModel : INotifyPropertyChanged
    {
        private Root? _root;

        public Root? Root
        {
            get => _root;
            set
            {
                _root = value;
                OnPropertyChanged();
            }
        }

        public string? FilePath { get; set; }

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
