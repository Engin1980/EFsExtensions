using Eng.Chlaot.Modules.ChecklistModule.Types.VM;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media;

namespace Eng.Chlaot.Modules.ChecklistModule.Converters
{
  internal class RunStateToSolidBrushColorConverter : IValueConverter
  {
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
      RunState runState = (RunState)value;
      SolidColorBrush ret;

      ret = runState switch
      {
        RunState.Current => new SolidColorBrush(Colors.Yellow),
        RunState.Runned => new SolidColorBrush(Colors.LightGreen),
        RunState.NotYet => new SolidColorBrush(Colors.White),
        _ => throw new NotSupportedException()
      };

      return ret;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
      throw new NotImplementedException();
    }
  }
}
