using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using XmlDiffLib.Models;

namespace XmlDiffLib
{
    /// <summary>
    /// PropertyDetailWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class PropertyDetailWindow : Window, INotifyPropertyChanged
    {
        private Process _fromProcess;
        private Process _toProcess;

        public PropertyDetailWindow()
        {
            InitializeComponent();

            this.DataContext = this;
        }

        public Process FromProcess
        {
            get => _fromProcess;
            set
            {
                if (_fromProcess != value)
                {
                    _fromProcess = value;
                    OnPropertyChanged();
                }
            }
        }

        public Process ToProcess
        {
            get => _toProcess;
            set
            {
                if (_toProcess != value)
                {
                    _toProcess = value;
                    OnPropertyChanged();
                }
            }
        }

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
