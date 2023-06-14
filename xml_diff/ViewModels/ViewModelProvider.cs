using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using xml_diff.Common.Base;

namespace xml_diff.ViewModels
{
    public class ViewModelProvider : ViewModelBase
    {
        private ViewModelBase? _diff01;
        private ViewModelBase? _diff02;
        private SharedDataViewModel _sharedDataViewModel = new SharedDataViewModel();

        public ViewModelBase? Diff01
        {
            get => _diff01;
            set
            {
                _diff01 = value;
                OnPropertyChanged();
            }
        }

        public ViewModelBase? Diff02
        {
            get => _diff02;
            set
            {
                _diff02 = value;
                OnPropertyChanged();
            }
        }

        public ViewModelBase? SharedDataViewModel
        {
            get => _sharedDataViewModel;
        }
    }
}
