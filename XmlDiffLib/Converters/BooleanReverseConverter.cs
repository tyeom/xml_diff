using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace XmlDiffLib.Converters
{
    public class BooleanReverseConverter : IValueConverter
    {
        #region IValueConverter 구현
        public object Convert(
            object value,
            Type targetType,
            object parameter,
            System.Globalization.CultureInfo culture)
        {
            try
            {
                return !(bool)value;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public object ConvertBack(
            object value,
            Type targetType,
            object parameter,
            System.Globalization.CultureInfo culture)
        {
            try
            {
                return !(bool)value;
            }
            catch (Exception)
            {
                return false;
            }
        }
        #endregion  // IValueConverter 구현
    }
}
