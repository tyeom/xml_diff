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
        private MainViewModel _mainViewModel = new();
        private SharedDataViewModel _sharedDataViewModel = new SharedDataViewModel();

        public MainViewModel? MainViewModel
        {
            get => _mainViewModel;
            set
            {
                _mainViewModel = value;
                OnPropertyChanged();
            }
        }

        public ViewModelBase? SharedDataViewModel
        {
            get => _sharedDataViewModel;
        }
    }
}
