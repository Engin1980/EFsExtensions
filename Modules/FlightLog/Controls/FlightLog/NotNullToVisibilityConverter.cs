using Eng.EFsExtensions.EFsExtensionsModuleBase.ModuleUtils.WPF.Converters;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Eng.EFsExtensions.Modules.FlightLogModule.Controls.FlightLog
{
  public class NotNullToVisibilityConverter : TypedConverter<object?, Visibility>
  {
    protected override Visibility Convert(object? value, object parameter, CultureInfo culture)
    {
      return value != null ? Visibility.Visible : Visibility.Hidden;
    }

    protected override object? ConvertBack(Visibility value, object parameter, CultureInfo culture)
    {
      throw new NotImplementedException();
    }
  }
}
