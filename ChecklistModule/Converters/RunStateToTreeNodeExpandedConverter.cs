using ChecklistModule.Types.RunViews;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace ChecklistModule.Converters
{
  internal class RunStateToTreeNodeExpandedConverter : IValueConverter
  {
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
      RunState runState= (RunState)value;
      bool ret = runState == RunState.Current;
      return ret;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
      throw new NotImplementedException();
    }
  }
}
