using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using xml_diff.Common;
using xml_diff.Common.Base;
using xml_diff.Common.EventAggregator;
using xml_diff.Messaging;
using XmlDiffLib;

namespace xml_diff.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        public delegate void XmlDataInitEventHandler();
        public event XmlDataInitEventHandler XmlDataInit;

        public delegate void FromFileLoadedEventHandler(string filePath);
        public event FromFileLoadedEventHandler FromFileLoaded;

        public delegate void ToFileLoadedEventHandler(string filePath);
        public event ToFileLoadedEventHandler ToFileLoaded;

        private Diff01ViewModel? _diff01;
        private Diff02ViewModel? _diff02;
        private string? _path;
        private XmlDiffControl.EModeType _modeType;

        public MainViewModel()
        {
            Diff01 = new();
            Diff02 = new();
        }

        public Diff01ViewModel? Diff01
        {
            get => _diff01;
            set
            {
                _diff01 = value;
                OnPropertyChanged();
            }
        }

        public Diff02ViewModel? Diff02
        {
            get => _diff02;
            set
            {
                _diff02 = value;
                OnPropertyChanged();
            }
        }

        public string? Path
        {
            get => _path;
            set
            {
                if (_path != value)
                {
                    _path = value;
                    OnPropertyChanged();
                }
            }
        }

        public XmlDiffControl.EModeType ModeType
        {
            get => _modeType;
            set
            {
                if (_modeType != value)
                {
                    _modeType = value;
                    OnPropertyChanged();
                }
            }
        }

        private RelayCommand<Object>? _browseCommand;
        public ICommand BrowseCommand
        {
            get
            {
                return _browseCommand ??
                    (_browseCommand = new RelayCommand<Object>(this.BrowseExecute));
            }
        }

        private RelayCommand<string>? _modeCommand;
        public ICommand ModeCommand
        {
            get
            {
                return _modeCommand ??
                    (_modeCommand = new RelayCommand<string>(this.ModeExecute));
            }
        }

        private RelayCommand<Object>? _closeCommand;
        public ICommand CloseCommand
        {
            get
            {
                return _closeCommand ??
                    (_closeCommand = new RelayCommand<Object>(this.CloseExecute));
            }
        }

        private void BrowseExecute(object param)
        {
            string? xmlFilePath = null;
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Xml files (*.xml)|*.xml";
            if (openFileDialog.ShowDialog() == true)
            {
                Path = openFileDialog.FileName;
                xmlFilePath = openFileDialog.FileName;

                if (XmlDataInit is not null)
                {
                    XmlDataInit();
                }

                if (FromFileLoaded is not null)
                {
                    FromFileLoaded(xmlFilePath);
                }

                if (ToFileLoaded is not null)
                {
                    ToFileLoaded(xmlFilePath);
                }
            }
        }

        private void ModeExecute(string mode)
        {
            XmlDiffControl.EModeType modeType;
            if (Enum.TryParse<XmlDiffControl.EModeType>(mode, out modeType) is false)
                return;

            Path = null;
            ModeType = modeType;
            EventAggregator.Instance.Publish<ChangeModeMessaging>(new(ModeType));

            App.Current.MainWindow.Title = $"XML file diff ({ModeType} mode)";
        }

        private void CloseExecute(object param)
        {
            App.Current.Shutdown();
        }
    }
}
