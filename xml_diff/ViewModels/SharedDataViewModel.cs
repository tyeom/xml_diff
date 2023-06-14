using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows;
using xml_diff.Common.Base;
using XmlDiffLib;
using System.Windows.Input;

namespace xml_diff.ViewModels
{
    public class SharedDataViewModel : ViewModelBase
    {
        private double _scrollVerticalOffset;
        private Brush? _withoutColor;
        private Brush? _dupColor;
        private Brush? _diffColor;
        private Brush? _uncheckedColor;

        public double ScrollVerticalOffset
        {
            get => _scrollVerticalOffset;
            set
            {
                _scrollVerticalOffset = value;
                OnPropertyChanged();
            }
        }

        public Brush WithoutColor
        {
            get => _withoutColor;
            set
            {
                _withoutColor = value;
                OnPropertyChanged();
            }
        }

        public Brush DupColor
        {
            get => _dupColor;
            set
            {
                _dupColor = value;
                OnPropertyChanged();
            }
        }

        public Brush DiffColor
        {
            get => _diffColor;
            set
            {
                _diffColor = value;
                OnPropertyChanged();
            }
        }

        public Brush UncheckedColor
        {
            get => _uncheckedColor;
            set
            {
                _uncheckedColor = value;
                OnPropertyChanged();
            }
        }
    }
}
