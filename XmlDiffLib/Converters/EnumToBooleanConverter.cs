using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace XmlDiffLib.Converters
{
    public class EnumToBooleanConverter : IValueConverter
    {
        public object Convert(
            object value,
            Type targetType,
            object parameter,
            System.Globalization.CultureInfo culture)
        {
            string strEnum = value.ToString();

            if (parameter != null && parameter.ToString().Equals(strEnum))
            {
                return true;
            }
            else
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
            if (parameter == null) return null;

            try
            {
                if ((bool)value == false)
                {
                    int menuCategoryIdx = (int)Enum.Parse(targetType, parameter.ToString());
                    return menuCategoryIdx - 1;
                }
                else
                {
                    // 직접 선택됨.
                    return Enum.Parse(targetType, parameter.ToString());
                }
            }
            catch
            {
                return null;
            }
        }
    }
}
