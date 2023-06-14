using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using xml_diff.Common.Base;

namespace xml_diff.ViewModels
{
    public class Diff02ViewModel : ViewModelBase
    {
        private string? _filePath;

        public string? FilePath
        {
            get => _filePath;
            set
            {
                _filePath = value;
                OnPropertyChanged();
            }
        }
    }
}
