using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace Eng.EFsExtensions.EFsExtensionsModuleBase.ModuleUtils.WPF.Converters
{
  public class DoubleFormatter : IValueConverter
  {
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
      double d;
      if ((value is int) || (value is long) || (value is double))
        d = (double)value;
      else if (value == null)
        d = Double.NaN;
      else
        throw new NotImplementedException($"Expected type is double/long/int, provided ${value.GetType().Name}.");

      string ret;
      if (parameter is string s)
        ret = d.ToString(s);
      else
        ret = d.ToString();
      return ret;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
      throw new NotImplementedException();
    }
  }
}
