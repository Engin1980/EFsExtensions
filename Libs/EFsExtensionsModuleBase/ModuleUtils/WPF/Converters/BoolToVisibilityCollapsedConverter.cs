using ESystem.WPF;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Eng.EFsExtensions.EFsExtensionsModuleBase.ModuleUtils.WPF.Converters
{
  public class BoolToVisibilityCollapsedConverter : TypedConverter<bool, System.Windows.Visibility>
  {
    protected override Visibility Convert(bool value, object parameter, CultureInfo culture)
    {
      return value ? Visibility.Visible : Visibility.Collapsed;
    }

    protected override bool ConvertBack(Visibility value, object parameter, CultureInfo culture)
    {
      throw new NotImplementedException();
    }
  }
}
