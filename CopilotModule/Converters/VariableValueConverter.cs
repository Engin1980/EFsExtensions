using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace Eng.Chlaot.Modules.CopilotModule.Converters
{
  public class VariableValueConverter : IValueConverter
  {
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
      string ret;
      double? val = (double?)value;
      if (val.HasValue)
        ret = val.ToString()!;
      else
        ret = "";
      return ret;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
      double? ret;
      string s = (string)value;
      if (string.IsNullOrWhiteSpace(s))
        ret = null;
      else
        try
        {
          ret = double.Parse(s);
        }
        catch
        {
          return DependencyProperty.UnsetValue;
        }
      return ret!;
    }
  }
}
