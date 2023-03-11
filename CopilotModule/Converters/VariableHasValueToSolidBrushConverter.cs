using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media;

namespace Eng.Chlaot.Modules.CopilotModule.Converters
{
  public class VariableHasValueToSolidBrushConverter : IValueConverter
  {
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
      SolidColorBrush ret;
      bool val = (bool)value;
      ret = val
        ? new SolidColorBrush(Colors.LightGreen)
        : new SolidColorBrush(Colors.LightPink);

      return ret;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
      throw new NotImplementedException();
    }
  }
}
