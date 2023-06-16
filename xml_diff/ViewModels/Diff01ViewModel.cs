using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using xml_diff.Common.Base;
using xml_diff.Common.EventAggregator;
using xml_diff.Messaging;
using XmlDiffLib;
using static XmlDiffLib.XmlDiffControl;

namespace xml_diff.ViewModels
{
    public class Diff01ViewModel : ViewModelBase
    {
        private string? _filePath;
        private XmlDiffControl.EModeType _mode = XmlDiffControl.EModeType.Write;

        public Diff01ViewModel()
        {
            EventAggregator.Instance.Subscribe<ChangeModeMessaging>(this.OnChangeMode);
        }

        public string? FilePath
        {
            get => _filePath;
            set
            {
                _filePath = value;
                OnPropertyChanged();
            }
        }

        public XmlDiffControl.EModeType Mode
        {
            get => _mode;
            set
            {
                if(_mode != value)
                {
                    _mode = value;
                    OnPropertyChanged();
                }
            }
        }

        private void OnChangeMode(ChangeModeMessaging changeModeMessaging)
        {
            Mode = changeModeMessaging.ModeType;
        }
    }
}
