using Eng.Chlaot.ChlaotModuleBase.ModuleUtils.WPF.Converters;
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
  public class DoubleHasValueToSolidBrushConverter : TypedConverter<double, SolidColorBrush>
  {
    protected override SolidColorBrush Convert(double value, object parameter, CultureInfo culture)
    {
      SolidColorBrush ret;
      ret = !double.IsNaN(value)
        ? new SolidColorBrush(Colors.LightGreen)
        : new SolidColorBrush(Colors.LightPink);

      return ret;
    }

    protected override double ConvertBack(SolidColorBrush value, object parameter, CultureInfo culture)
    {
      throw new NotImplementedException();
    }
  }
}
