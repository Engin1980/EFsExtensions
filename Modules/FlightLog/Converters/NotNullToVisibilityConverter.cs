using Eng.EFsExtensions.EFsExtensionsModuleBase.ModuleUtils.WPF.Converters;
using ESystem.WPF;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Eng.EFsExtensions.Modules.FlightLogModule.Converters
{
  public class NotNullToVisibilityConverter : TypedConverter<object?, Visibility>
  {
    protected override Visibility Convert(object? value, object parameter, CultureInfo culture)
    {
      return value != null ? Visibility.Visible : Visibility.Collapsed;
    }

    protected override object? ConvertBack(Visibility value, object parameter, CultureInfo culture)
    {
      throw new NotImplementedException();
    }
  }
}
